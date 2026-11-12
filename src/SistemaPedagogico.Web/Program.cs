using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Claims;
using SistemaPedagogico.Web.Portal;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SistemaPedagogico.Core.Entities;
using SistemaPedagogico.Infrastructure.Backup;
using SistemaPedagogico.Infrastructure.Data;
using SistemaPedagogico.Infrastructure.Storage;

var builder = WebApplication.CreateBuilder(args);
var configuredDatabaseUrl = builder.Configuration["DATABASE_URL"];
var online = !string.IsNullOrWhiteSpace(configuredDatabaseUrl);
string? onlineDatabaseUrl = null;
if(online){
 var connection = new Npgsql.NpgsqlConnectionStringBuilder(configuredDatabaseUrl!) {
  Multiplexing = false,
  Pooling = true,
  KeepAlive = 30,
  ConnectionIdleLifetime = 60,
  ConnectionLifetime = 300
 };
 if(connection.Port==6543 && connection.Host?.EndsWith(".pooler.supabase.com",StringComparison.OrdinalIgnoreCase)==true)connection.Port=5432;
 onlineDatabaseUrl=connection.ConnectionString;
}
if(online && int.TryParse(builder.Configuration["PORT"],out var port))builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add services to the container.
builder
    .Services.AddRazorPages(options => { options.Conventions.AuthorizeFolder("/", "Administrator"); options.Conventions.AllowAnonymousToPage("/Error"); })
    .AddMvcOptions(options =>
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true
    );
var storagePaths = StoragePaths.Resolve(
    online?Path.Combine(Path.GetTempPath(),"tutoring-runtime"):builder.Configuration["Storage:DataDirectory"],
    builder.Environment.ContentRootPath
);
builder.Services.AddSingleton(storagePaths);
builder.Services.AddSingleton<BackupService>();
if(!online)builder.Services.AddDbContext<SistemaPedagogicoDbContext>(options => options.UseSqlite($"Data Source={storagePaths.DatabasePath};Foreign Keys=True"));

builder.Services.AddControllersWithViews();
builder.Services.AddAuthorization(options => options.AddPolicy("Administrator", p => p.RequireRole("Admin")));
if(online){
 builder.Services.AddDbContext<EducaDb>(options=>options.UseNpgsql(onlineDatabaseUrl));
 builder.Services.AddHostedService<OnlineBootstrapHostedService>();
 builder.Services.AddHttpClient<IAttachmentStorage,SupabaseAttachmentStorage>();
 builder.Services.AddDataProtection().PersistKeysToDbContext<EducaDb>().SetApplicationName("EducaMais");
}else{
 builder.Services.AddDbContext<EducaDb>(options => options.UseSqlite(new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder { DataSource = Path.Combine(storagePaths.DataDirectory, "educamais.db"), ForeignKeys = true }.ToString()));
 builder.Services.AddScoped<IAttachmentStorage,LocalAttachmentStorage>();
 builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(storagePaths.DataDirectory, "Chaves"))).SetApplicationName("EducaMais");
}
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => {
 options.LoginPath = "/conta/entrar"; options.AccessDeniedPath = "/conta/negado"; options.Cookie.Name = "EducaMais.Session";
 options.Cookie.HttpOnly = true; options.Cookie.SameSite = SameSiteMode.Lax; options.ExpireTimeSpan = TimeSpan.FromHours(8);
 if(online)options.Cookie.SecurePolicy=CookieSecurePolicy.Always;
 options.Events.OnValidatePrincipal = async context => {
  var db = context.HttpContext.RequestServices.GetRequiredService<EducaDb>();
  var id = int.TryParse(context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var n) ? n : 0;
  var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id && x.Active);
  var valid = user != null && user.Stamp == context.Principal?.FindFirstValue("stamp");
  if(valid && user!.Role == "Aluno") valid = await db.Students.AnyAsync(x => x.Id == user.EntityId && x.Active);
  if(valid && user!.Role == "Professor") valid = await db.Teachers.AnyAsync(x => x.Id == user.EntityId && x.Active);
  if(valid && user!.Role is not ("Admin" or "Professor" or "Aluno")) valid = false;
  if(!valid) { context.RejectPrincipal(); await Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions.SignOutAsync(context.HttpContext, CookieAuthenticationDefaults.AuthenticationScheme); }
 };
});
var app = builder.Build();

var ptBr = CultureInfo.GetCultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = ptBr;
CultureInfo.DefaultThreadCurrentUICulture = ptBr;

if(!online)using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SistemaPedagogicoDbContext>();
    var databaseJaExistia = File.Exists(storagePaths.DatabasePath);
    var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();
    if (databaseJaExistia && pending.Count > 0)
        scope.ServiceProvider.GetRequiredService<BackupService>().Criar("antes-migration");

    await db.Database.MigrateAsync();
    if (!await db.Disciplinas.AnyAsync())
    {
        db.Disciplinas.AddRange(
            new[]
            {
                "Língua Portuguesa",
                "Matemática",
                "Ciências",
                "História",
                "Geografia",
                "Inglês",
            }.Select(nome => new Disciplina { Nome = nome })
        );
        await db.SaveChangesAsync();
    }
}

if(!online)using (var scope = app.Services.CreateScope()) {
 await PortalBootstrap.ImportAsync(scope.ServiceProvider.GetRequiredService<EducaDb>(), scope.ServiceProvider.GetRequiredService<SistemaPedagogicoDbContext>());
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.Use(async (context, next) => {
 if(context.Request.Path == "/") { context.Response.Redirect("/portal"); return; }
 context.Response.Headers["X-Content-Type-Options"] = "nosniff";
 context.Response.Headers["Referrer-Policy"] = "same-origin";
 await next();
});
app.UseRouting();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();
app.MapStaticAssets();
if(!online)app.MapRazorPages().WithStaticAssets();

app.Run();
