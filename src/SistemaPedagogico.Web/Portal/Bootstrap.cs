using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using SistemaPedagogico.Infrastructure.Data;
namespace SistemaPedagogico.Web.Portal;
public static class PortalBootstrap {
 public static async Task ImportAsync(EducaDb db,SistemaPedagogicoDbContext old) {
 await db.Database.EnsureCreatedAsync();
 // Existing SQLite databases need the new column before EF queries students.
 await db.Database.OpenConnectionAsync();
 try {
 var connection=db.Database.GetDbConnection();
 using var command=connection.CreateCommand();command.CommandText="PRAGMA table_info(Students)";
 var hasStatus=false;
 await using(var reader=await command.ExecuteReaderAsync()){while(await reader.ReadAsync())if(reader.GetString(1)=="Status")hasStatus=true;}
 if(!hasStatus){
 await using var upgrade=await db.Database.BeginTransactionAsync();
 await db.Database.ExecuteSqlRawAsync("ALTER TABLE Students ADD COLUMN Status TEXT NOT NULL DEFAULT 'Ativo'");
 await db.Database.ExecuteSqlRawAsync("UPDATE Students SET Status='Inativo' WHERE Active=0");
 await upgrade.CommitAsync();
 }
 }finally{await db.Database.CloseConnectionAsync();}

 await db.Database.OpenConnectionAsync();
 try {
 var connection=db.Database.GetDbConnection();
 // Snapshot only when an on-disk database still needs the version 4 columns.
 using(var check=connection.CreateCommand()){
 check.CommandText="SELECT COUNT(*) FROM pragma_table_info('Progress') WHERE name='InternalNotes'";
 if(Convert.ToInt32(await check.ExecuteScalarAsync())==0&&connection is Microsoft.Data.Sqlite.SqliteConnection sqlite&&File.Exists(sqlite.DataSource)){
 var backupDir=Path.Combine(Path.GetDirectoryName(Path.GetFullPath(sqlite.DataSource))!,"Backups");Directory.CreateDirectory(backupDir);
 using var backup=new Microsoft.Data.Sqlite.SqliteConnection(new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder{DataSource=Path.Combine(backupDir,"tutoring-antes-v4.1-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+"-"+Guid.NewGuid().ToString("N")+".db")}.ToString());backup.Open();sqlite.BackupDatabase(backup);
 }
 }
 await using var upgrade=await db.Database.BeginTransactionAsync();
 var additions=new (string table,string column,string definition)[]{
 ("Progress","InternalNotes","TEXT NOT NULL DEFAULT ''"),("Progress","LessonId","INTEGER NULL"),("Payments","History","TEXT NOT NULL DEFAULT ''"),
 ("Students","ServiceType","TEXT NOT NULL DEFAULT 'Reforço escolar'"),("Students","RegistrationKey","TEXT NOT NULL DEFAULT ''"),
 ("Lessons","OnlineUrl","TEXT NOT NULL DEFAULT ''"),("Grades","Source","TEXT NOT NULL DEFAULT 'Avaliação da professora'"),
 ("Activities","Origin","TEXT NOT NULL DEFAULT 'Proposta da professora'"),("Activities","AssignmentKey","TEXT NULL"),("Activities","AssignmentName","TEXT NULL"),
 ("Progress","ServiceType","TEXT NOT NULL DEFAULT 'Reforço escolar'"),("Progress","SubjectId","INTEGER NULL"),
 ("Progress","Difficulty","TEXT NOT NULL DEFAULT ''"),("Progress","Goal","TEXT NOT NULL DEFAULT ''"),("Progress","Worked","TEXT NOT NULL DEFAULT ''"),
 ("Progress","Evolution","TEXT NOT NULL DEFAULT ''"),("Progress","NextSteps","TEXT NOT NULL DEFAULT ''"),("Progress","Tasks","TEXT NOT NULL DEFAULT ''"),("Progress","Autonomy","TEXT NOT NULL DEFAULT ''")};
 foreach(var item in additions){
 using var command=connection.CreateCommand();command.Transaction=upgrade.GetDbTransaction();command.CommandText="PRAGMA table_info("+item.table+")";
 var exists=false;await using(var reader=await command.ExecuteReaderAsync()){while(await reader.ReadAsync())if(reader.GetString(1)==item.column)exists=true;}
 if(!exists){using var add=connection.CreateCommand();add.Transaction=upgrade.GetDbTransaction();add.CommandText="ALTER TABLE "+item.table+" ADD COLUMN "+item.column+" "+item.definition;await add.ExecuteNonQueryAsync();}
 }
 await db.Database.ExecuteSqlRawAsync("CREATE UNIQUE INDEX IF NOT EXISTS IX_Students_RegistrationKey ON Students(RegistrationKey) WHERE RegistrationKey <> ''");
 await upgrade.CommitAsync();
 }finally{await db.Database.CloseConnectionAsync();}

 if(await db.Settings.AnyAsync(x=>x.Key=="legacy-import")) return;
 await using var tx=await db.Database.BeginTransactionAsync();
 var subjectMap=new Dictionary<int,int>(); var studentMap=new Dictionary<int,int>();
 foreach(var s in await old.Disciplinas.AsNoTracking().ToListAsync()) { var n=new Subject{Name=s.Nome,Description=s.Descricao??"",Active=s.Ativa,LegacyId=s.Id};db.Subjects.Add(n);await db.SaveChangesAsync();subjectMap[s.Id]=n.Id; }
 foreach(var s in await old.Alunos.Include(x=>x.Responsaveis).ThenInclude(x=>x.Responsavel).AsNoTracking().ToListAsync()) {var g=s.Responsaveis.OrderByDescending(x=>x.ContatoPrincipal).FirstOrDefault()?.Responsavel;var n=new Student{Name=s.Nome,BirthDate=s.DataNascimento,Level=s.SerieAno??"Ensino Fundamental II",Shift=s.Turno??"Tarde",Guardian=g?.Nome??"",GuardianPhone=g?.Telefone??"",Active=s.Ativo,Status=s.Ativo?"Ativo":"Inativo",LegacyId=s.Id};db.Students.Add(n);await db.SaveChangesAsync();studentMap[s.Id]=n.Id;}
 foreach(var n in await old.Notas.AsNoTracking().ToListAsync()) if(studentMap.ContainsKey(n.AlunoId)&&subjectMap.ContainsKey(n.DisciplinaId)) db.Grades.Add(new Grade{StudentId=studentMap[n.AlunoId],SubjectId=subjectMap[n.DisciplinaId],Title=n.TipoAvaliacao+" · "+n.Periodo,Value=(double)(n.ValorMaximo>0?n.Valor/n.ValorMaximo*10:0),Date=n.Data,Comment=n.Observacao??""});
 foreach(var a in await old.TarefasEscolares.AsNoTracking().ToListAsync()) if(studentMap.ContainsKey(a.AlunoId)) db.Activities.Add(new Activity{StudentId=studentMap[a.AlunoId],SubjectId=a.DisciplinaId is int d&&subjectMap.ContainsKey(d)?subjectMap[d]:null,Title=a.Titulo,Description=a.Descricao??"",Due=a.Prazo,Status=a.Status==SistemaPedagogico.Core.Entities.StatusTarefa.Concluida?"Corrigida":"Pendente",Feedback=a.Status==SistemaPedagogico.Core.Entities.StatusTarefa.Concluida?"Conclusão importada da versão anterior.":""});
 db.Settings.Add(new PortalSetting{Key="legacy-import",Value=DateTime.Now.ToString("O")});await db.SaveChangesAsync();await tx.CommitAsync();
 }
}
