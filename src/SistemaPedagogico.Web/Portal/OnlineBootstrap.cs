using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Security.Cryptography;
using System.Text;

namespace SistemaPedagogico.Web.Portal;

public static class OnlineBootstrap {
 public static async Task InitializeAsync(EducaDb db,IConfiguration config){
  await db.Database.ExecuteSqlRawAsync("CREATE SCHEMA IF NOT EXISTS tutoring");
  await using(var tx=await db.Database.BeginTransactionAsync()){
   await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(73149281)");
   await using var command=db.Database.GetDbConnection().CreateCommand();
   command.Transaction=tx.GetDbTransaction();command.CommandText="SELECT to_regclass('tutoring.\"Students\"')";
   if(await command.ExecuteScalarAsync() is DBNull or null){
    var script=db.Database.GenerateCreateScript().Replace("CREATE SCHEMA tutoring;","CREATE SCHEMA IF NOT EXISTS tutoring;",StringComparison.OrdinalIgnoreCase);
    await db.Database.ExecuteSqlRawAsync(script);
   }
   await tx.CommitAsync();
  }
  if(!await db.Subjects.AnyAsync()){
   db.Subjects.AddRange(new[]{"Língua Portuguesa","Matemática","Ciências","História","Geografia","Inglês"}.Select(x=>new Subject{Name=x}));
   await db.SaveChangesAsync();
  }
  if(await db.Users.AnyAsync()){
   var recovery=config["RECOVERY_ADMIN_PASSWORD"];
   if(!string.IsNullOrWhiteSpace(recovery)){
    if(recovery.Length<12)throw new InvalidOperationException("RECOVERY_ADMIN_PASSWORD exige pelo menos 12 caracteres.");
    var digest=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(recovery)));
    await using var recoveryTx=await db.Database.BeginTransactionAsync();
    await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(73149283)");
    var marker=await db.Settings.SingleOrDefaultAsync(x=>x.Key=="recovery-password-digest");
    if(marker?.Value!=digest){
     var ownerId=await db.Settings.Where(x=>x.Key==SingleTeacher.OwnerKey).Select(x=>x.Value).SingleAsync();
     var owner=await db.Users.SingleAsync(x=>x.Id==int.Parse(ownerId));
     owner.PasswordHash=new PasswordHasher<PortalUser>().HashPassword(owner,recovery);owner.Stamp=Guid.NewGuid().ToString();
     if(marker==null)db.Settings.Add(new PortalSetting{Key="recovery-password-digest",Value=digest});else marker.Value=digest;
     await db.SaveChangesAsync();
    }
    await recoveryTx.CommitAsync();
   }
   return;
  }
  var name=config["INITIAL_ADMIN_NAME"];var email=config["INITIAL_ADMIN_EMAIL"];var password=config["INITIAL_ADMIN_PASSWORD"];
  if(string.IsNullOrWhiteSpace(name)||string.IsNullOrWhiteSpace(email)||string.IsNullOrWhiteSpace(password)||password.Length<12)
   throw new InvalidOperationException("Configure INITIAL_ADMIN_NAME, INITIAL_ADMIN_EMAIL e INITIAL_ADMIN_PASSWORD (mínimo 12 caracteres) antes da primeira publicação.");
  await using var seed=await db.Database.BeginTransactionAsync();
  await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(73149282)");
  if(!await db.Users.AnyAsync()){
   var teacher=new Teacher{Name=name.Trim(),Email=email.Trim().ToLowerInvariant(),SubjectIds=string.Join(',',await db.Subjects.Select(x=>x.Id).ToListAsync())};
   db.Teachers.Add(teacher);await db.SaveChangesAsync();
   var admin=new PortalUser{Name=teacher.Name,Email=teacher.Email,Role="Admin",EntityId=teacher.Id};
   admin.PasswordHash=new PasswordHasher<PortalUser>().HashPassword(admin,password);
   db.Users.Add(admin);await db.SaveChangesAsync();
   db.Settings.AddRange(new PortalSetting{Key=SingleTeacher.OwnerKey,Value=admin.Id.ToString()},new PortalSetting{Key=SingleTeacher.TeacherKey,Value=teacher.Id.ToString()});
   await db.SaveChangesAsync();
  }
  await seed.CommitAsync();
 }
}
