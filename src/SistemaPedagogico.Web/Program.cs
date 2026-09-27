using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SistemaPedagogico.Core.Entities;
using SistemaPedagogico.Infrastructure.Backup;
using SistemaPedagogico.Infrastructure.Data;
using SistemaPedagogico.Infrastructure.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder
    .Services.AddRazorPages()
    .AddMvcOptions(options =>
        options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true
    );
var storagePaths = StoragePaths.Resolve(
    builder.Configuration["Storage:DataDirectory"],
    builder.Environment.ContentRootPath
);
builder.Services.AddSingleton(storagePaths);
builder.Services.AddSingleton<BackupService>();
builder.Services.AddDbContext<SistemaPedagogicoDbContext>(options =>
    options.UseSqlite($"Data Source={storagePaths.DatabasePath};Foreign Keys=True")
);

var app = builder.Build();

var ptBr = CultureInfo.GetCultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = ptBr;
CultureInfo.DefaultThreadCurrentUICulture = ptBr;

using (var scope = app.Services.CreateScope())
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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
