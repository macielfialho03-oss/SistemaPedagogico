using System.Globalization;
using System.IO.Compression;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaPedagogico.Infrastructure.Storage;
namespace SistemaPedagogico.Web.Portal;
[Authorize,Route("portal")]
public class PortalController(EducaDb db,StoragePaths paths,IAttachmentStorage files,IConfiguration config):Controller {
 string Role=>User.FindFirstValue(ClaimTypes.Role)??"";int Entity=>int.TryParse(User.FindFirstValue("entity"),out var n)?n:0;
 bool Admin=>Role=="Admin";bool Staff=>Admin||Role=="Professor";
 async Task<bool> CanStudent(int id)=>Admin||(Role=="Aluno"?Entity==id:Role=="Professor"&&await db.Enrollments.AnyAsync(x=>x.StudentId==id&&x.TeacherId==Entity));
 async Task DemandStudent(int id){if(!await CanStudent(id))throw new UnauthorizedAccessException();if(!await db.Students.AnyAsync(x=>x.Id==id&&x.Active))throw new ArgumentException("Aluno inexistente ou inativo.");}
 static string F(IFormCollection f,string k,int max=3000)=>f[k].ToString().Trim() is var s&&s.Length<=max?s:throw new ArgumentException("Campo muito longo: "+k);
 static string Required(IFormCollection f,string k,int max=120)=>F(f,k,max) is var s&&s.Length>0?s:throw new ArgumentException("Preencha o campo "+k+".");
 static string FullName(IFormCollection f,string key,bool optional=false){
 var value=System.Text.RegularExpressions.Regex.Replace(F(f,key,120),@"\s+"," ").Trim();
 if(optional&&value.Length==0)return "";
 if(value.Split(' ',StringSplitOptions.RemoveEmptyEntries).Length<2||!System.Text.RegularExpressions.Regex.IsMatch(value,@"^[\p{L}\p{M}]+(?:['’\- ][\p{L}\p{M}]+)*$"))throw new ArgumentException("Informe nome e sobrenome, usando apenas letras, no campo "+(key=="Guardian"?"responsável":"nome")+".");
 return CultureInfo.GetCultureInfo("pt-BR").TextInfo.ToTitleCase(value.ToLower(CultureInfo.GetCultureInfo("pt-BR")));
 }
 async Task Compatible(int sid,int subject,int? teacher=null){
 await DemandStudent(sid);await SubjectCheck(subject);
 var student=await db.Students.FindAsync(sid);
 if(!PortalRules.Ids(student!.SubjectIds).Contains(subject))throw new ArgumentException("Selecione uma matéria do aluno.");
 var tid=Role=="Professor"?Entity:teacher;
 if(tid!=null){var t=await db.Teachers.FindAsync(tid);if(t==null||!t.Active||!await db.Enrollments.AnyAsync(x=>x.StudentId==sid&&x.TeacherId==tid))throw new ArgumentException("Escolha um professor vinculado ao aluno que lecione esta matéria.");}
 }
 static string OptionalChoice(IFormCollection f,string key,string fallback,params string[] values)=>F(f,key)==""?fallback:Choice(f,key,values);
 static string Phone(IFormCollection f,string key){
 var value=F(f,key,40);if(value.Length==0)return "";
 if(value.Any(c=>!(c>='0'&&c<='9')&&!" ()-+".Contains(c)))throw new ArgumentException("Telefone inválido: informe somente o DDD e o número.");
 var digits=new string(value.Where(c=>c>='0'&&c<='9').ToArray());
 if((digits.Length==12||digits.Length==13)&&digits.StartsWith("55"))digits=digits[2..];
 if((digits.Length!=10&&digits.Length!=11)||digits[0]=='0'||digits[1]=='0'||(digits.Length==11&&digits[2]!='9')||(digits.Length==10&&(digits[2]<'2'||digits[2]>'5')))throw new ArgumentException("Telefone inválido: use DDD e número completo, como (69) 3333-3333 ou (69) 99999-9999.");
 var split=digits.Length==11?5:4;return "("+digits[..2]+") "+digits.Substring(2,split)+"-"+digits[(2+split)..];
 }
 static string ContactEmail(IFormCollection f){var email=F(f,"Email",150);if(email.Length>0&&!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))throw new ArgumentException("Informe um e-mail válido.");return email;}
 static int N(IFormCollection f,string k)=>int.TryParse(F(f,k),out var n)?n:0;
 static double D(IFormCollection f,string k)=>double.TryParse(F(f,k).Replace(',','.'),NumberStyles.AllowDecimalPoint|NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture,out var n)&&double.IsFinite(n)?n:throw new ArgumentException("Valor numérico inválido.");
 static decimal Money(IFormCollection f,string k)=>decimal.TryParse(F(f,k).Replace(',','.'),NumberStyles.AllowDecimalPoint|NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture,out var n)&&decimal.Round(n,2)==n?n:throw new ArgumentException("Valor monetário inválido: use até duas casas decimais.");
 static DateTime T(IFormCollection f,string k)=>DateTime.TryParse(F(f,k),CultureInfo.InvariantCulture,DateTimeStyles.None,out var t)?t:throw new ArgumentException("Data inválida.");
 static string Choice(IFormCollection f,string k,params string[] allowed)=>allowed.Contains(F(f,k))?F(f,k):throw new ArgumentException("Opção inválida: "+k);
 async Task SubjectCheck(int id){if(!await db.Subjects.AnyAsync(x=>x.Id==id&&x.Active))throw new ArgumentException("Selecione uma matéria ativa.");if(Role=="Professor"){var t=await db.Teachers.FindAsync(Entity);if(t==null||!PortalRules.Ids(t.SubjectIds).Contains(id))throw new UnauthorizedAccessException();}}
 async Task<string> Subjects(IFormCollection f){var ids=PortalRules.Ids(F(f,"SubjectIds"));if(ids.Length!=await db.Subjects.CountAsync(x=>ids.Contains(x.Id)&&x.Active))throw new ArgumentException("Matéria inválida.");return string.Join(",",ids);}
 [HttpGet("")] public async Task<IActionResult> Index(string section="painel",string q="",int? studentId=null,int? activityId=null){
 if(section is "professores" or "disponibilidade")return RedirectToAction(nameof(Index),new{section="aulas"});
 var adminOnly=new[]{"materias","mensalidades","relatorios","backup"};
 if(adminOnly.Contains(section)&&!Admin)return Forbid();
 var allowed=new[]{"painel","alunos","materias","aulas","notas","atividades","atividade","materiais","progresso","mensalidades","usuarios","relatorios","backup","ficha"};if(!allowed.Contains(section))return NotFound();
 if((section=="alunos"||section=="ficha")&&Role=="Aluno")return Forbid();
 var ids=Admin?await db.Students.Select(x=>x.Id).ToListAsync():Role=="Aluno"?new List<int>{Entity}:await db.Enrollments.Where(x=>x.TeacherId==Entity).Select(x=>x.StudentId).ToListAsync();
 var vm=new PortalVm{Section=section,Role=Role,Entity=Entity,Query=q,OwnerTeacherId=Admin?await SingleTeacher.ProfileIdAsync(db):0,Students=await db.Students.Where(x=>ids.Contains(x.Id)).OrderBy(x=>x.Name).ToListAsync(),Teachers=await db.Teachers.OrderBy(x=>x.Name).ToListAsync(),Subjects=await db.Subjects.OrderBy(x=>x.Name).ToListAsync(),Enrollments=await db.Enrollments.Where(x=>ids.Contains(x.StudentId)).ToListAsync()};
 // Aluno só recebe professores vinculados e suas matérias.
 if(Role=="Aluno"){var teacherIds=vm.Enrollments.Select(x=>x.TeacherId).ToList();vm.Teachers=vm.Teachers.Where(x=>teacherIds.Contains(x.Id)).ToList();}
 vm.Lessons=await db.Lessons.Where(x=>ids.Contains(x.StudentId)&&(Role!="Professor"||x.TeacherId==Entity)).OrderBy(x=>x.Start).ToListAsync();
 vm.Grades=await db.Grades.Where(x=>ids.Contains(x.StudentId)&&(Role!="Professor"||x.TeacherId==Entity||x.TeacherId==null)).OrderByDescending(x=>x.Date).ToListAsync();
 vm.Activities=await db.Activities.Where(x=>ids.Contains(x.StudentId)&&(Role!="Professor"||x.TeacherId==Entity||x.TeacherId==null)).OrderBy(x=>x.Due).ToListAsync();
 vm.Materials=await db.Materials.Where(x=>ids.Contains(x.StudentId)&&(Role!="Professor"||x.TeacherId==Entity)).OrderByDescending(x=>x.Created).ToListAsync();
 vm.Progress=await db.Progress.AsNoTracking().Where(x=>ids.Contains(x.StudentId)&&(Role!="Professor"||x.TeacherId==Entity)).OrderByDescending(x=>x.Date).ToListAsync();
 if(Admin){vm.Payments=await db.Payments.OrderBy(x=>x.Due).ToListAsync();vm.Users=await db.Users.OrderBy(x=>x.Role).ThenBy(x=>x.Name).ToListAsync();}
 else if(Role=="Professor")vm.Users=await db.Users.Where(x=>x.Role=="Professor"||x.Role=="Aluno"&&x.EntityId.HasValue&&ids.Contains(x.EntityId.Value)).OrderBy(x=>x.Role).ThenBy(x=>x.Name).ToListAsync();
 if(section=="ficha") { if(studentId==null||!ids.Contains(studentId.Value))return Forbid();vm.SelectedStudent=studentId; }
 if(section=="atividade"){
 var selected=vm.Activities.FirstOrDefault(x=>x.Id==activityId);if(selected==null)return Forbid();
 vm.SelectedActivity=selected.Id;vm.Activities=vm.Activities.Where(x=>PortalRules.ActivityGroup(x)==PortalRules.ActivityGroup(selected)).ToList();
 }
 if(Role=="Aluno")foreach(var item in vm.Progress)item.InternalNotes="";
 vm.LastBackup=(await db.Settings.FirstOrDefaultAsync(x=>x.Key=="last-backup"))?.Value??"";
 return View("Index",vm);
 }
 [HttpPost("salvar"),ValidateAntiForgeryToken,RequestSizeLimit(4_400_000)] public async Task<IActionResult> Save(string kind,IFormCollection f,IFormFile? attachment){
 var section=kind switch{"student" or "student-status" or "delete-student"=>"alunos","subject" or "subject-status"=>"materias","enrollment"=>"alunos","lesson" or "lesson-status" or "lesson-group-cancel"=>"aulas","grade" or "delete-grade"=>"notas","activity" or "submit" or "review"=>"atividades","material"=>"materiais","progress" or "delete-progress"=>"progresso","payment" or "paid" or "payment-edit" or "unpay"=>"mensalidades","user" or "user-password" or "user-status"=>"usuarios",_=>"painel"};
 string? written=null;string? replaced=null;var returnActivityId=N(f,"ReturnActivityId");Activity? published=null;
 try {
 await using var transaction = await db.Database.BeginTransactionAsync();
 if(new[]{"student","student-status","delete-student","availability","teacher","subject","subject-status","enrollment","payment","paid","payment-edit","unpay"}.Contains(kind)&&!Admin)return Forbid();
 if(new[]{"user","user-password","user-status"}.Contains(kind)&&!Staff)return Forbid();
 if(new[]{"grade","delete-grade","activity","review","material","progress","delete-progress"}.Contains(kind)&&!Staff)return Forbid();
 if(kind is "teacher" or "enrollment" or "availability")return Forbid();
 if((kind is "lesson" or "lesson-group-cancel")&&!Admin)return Forbid();
 var id=N(f,"Id");
 switch(kind){
 case "student":{
 var x=id==0?new Student():await db.Students.FindAsync(id)??throw new ArgumentException("Aluno não encontrado.");
 var name=FullName(f,"Name");var sub=await Subjects(f);var birth=F(f,"BirthDate")==""?(DateTime?)null:T(f,"BirthDate");if(birth>DateTime.Today.AddYears(-4))throw new ArgumentException("O aluno deve ter pelo menos 4 anos. Confira a data de nascimento.");
 x.Name=name;x.Email=ContactEmail(f);x.Phone=Phone(f,"Phone");x.BirthDate=birth;x.Level=Required(f,"Level",40);x.Shift=Choice(f,"Shift","Manhã","Tarde","Noite");x.Guardian=FullName(f,"Guardian",true);x.GuardianPhone=Phone(f,"GuardianPhone");x.SubjectIds=sub;x.Status=F(f,"Status")==""?(F(f,"Active")=="true"?"Ativo":"Inativo"):Choice(f,"Status","Ativo","Inativo","Formado");x.Active=x.Status=="Ativo";if(!x.Active&&await db.Lessons.AnyAsync(a=>a.StudentId==id&&a.Status=="Agendada"&&a.Start>DateTime.Now))throw new ArgumentException("Cancele ou conclua as aulas futuras antes de mudar a situação do aluno.");x.ServiceType=OptionalChoice(f,"ServiceType",x.ServiceType,"Reforço escolar","Acompanhamento escolar","Ambos");
 if(id==0){
 var key=F(f,"RegistrationKey",40);if(!Guid.TryParse(key,out _))throw new ArgumentException("Reabra o formulário para cadastrar o aluno.");
 if(await db.Students.AnyAsync(a=>a.RegistrationKey==key)){TempData["Success"]="Este cadastro já foi salvo. Nenhuma mensalidade foi duplicada.";return RedirectToAction(nameof(Index),new{section});}
 x.RegistrationKey=key;db.Students.Add(x);
 var assignedTeacher=await SingleTeacher.ProfileIdAsync(db);await db.SaveChangesAsync();db.Enrollments.Add(new Enrollment{StudentId=x.Id,TeacherId=assignedTeacher});
 var months=F(f,"BillingMode")=="no"?0:N(f,"BillingMonths");if(months<0||months>60||(F(f,"BillingMode")=="yes"&&months<1))throw new ArgumentException("Escolha entre 1 e 60 mensalidades, ou deixe sem mensalidades.");
 if(months>0){var amount=Money(f,"MonthlyAmount");if(amount<=0||amount>1000000)throw new ArgumentException("Informe um valor de mensalidade válido.");var first=T(f,"FirstDue").Date;await db.SaveChangesAsync();for(var m=0;m<months;m++)db.Payments.Add(new Payment{StudentId=x.Id,Amount=amount,Due=first.AddMonths(m),Method="",Notes="Mensalidade "+(m+1)+" de "+months});}
 }break;}
 case "student-status":{
 var student=await db.Students.FindAsync(id)??throw new ArgumentException("Aluno não encontrado.");
 var status=Choice(f,"Status","Ativo","Inativo","Formado");
 if(status!="Ativo"&&await db.Lessons.AnyAsync(x=>x.StudentId==id&&x.Status=="Agendada"&&x.Start>DateTime.Now))throw new ArgumentException("Cancele ou conclua as aulas futuras antes de mudar a situação do aluno.");
 student.Status=status;student.Active=status=="Ativo";break;}
 case "delete-student":{
 if(F(f,"ConfirmDelete")!="true")throw new ArgumentException("Confirme a exclusão do cadastro.");
 var student=await db.Students.FindAsync(id)??throw new ArgumentException("Aluno não encontrado.");
 if(student.LegacyId!=null||await db.Lessons.AnyAsync(x=>x.StudentId==id)||await db.Grades.AnyAsync(x=>x.StudentId==id)||await db.Activities.AnyAsync(x=>x.StudentId==id)||await db.Materials.AnyAsync(x=>x.StudentId==id)||await db.Progress.AnyAsync(x=>x.StudentId==id)||await db.Payments.AnyAsync(x=>x.StudentId==id))throw new ArgumentException("Este aluno possui histórico. Marque como Inativo ou Formado para preservá-lo.");
 db.Enrollments.RemoveRange(await db.Enrollments.Where(x=>x.StudentId==id).ToListAsync());
 db.Users.RemoveRange(await db.Users.Where(x=>x.Role=="Aluno"&&x.EntityId==id).ToListAsync());
 db.Students.Remove(student);break;}
 case "subject":{var name=Required(f,"Name",80);if(await db.Subjects.AnyAsync(x=>x.Name==name&&x.Id!=id))throw new ArgumentException("Matéria já cadastrada.");var x=id==0?new Subject():await db.Subjects.FindAsync(id)??throw new ArgumentException("Matéria não encontrada.");x.Name=name;x.Description=F(f,"Description",500);x.Active=F(f,"Active")=="true";if(id==0)db.Subjects.Add(x);break;}
 case "subject-status":{var x=await db.Subjects.FindAsync(id)??throw new ArgumentException("Matéria não encontrada.");x.Active=F(f,"Active")=="true";break;}
 case "enrollment":{var sid=N(f,"StudentId");var tid=N(f,"TeacherId");await DemandStudent(sid);if(!await db.Teachers.AnyAsync(x=>x.Id==tid&&x.Active))throw new ArgumentException("Professor inválido.");if(!await db.Enrollments.AnyAsync(x=>x.StudentId==sid&&x.TeacherId==tid))db.Enrollments.Add(new Enrollment{StudentId=sid,TeacherId=tid});break;}
 case "lesson":{
 var old=id==0?null:await db.Lessons.FindAsync(id)??throw new ArgumentException("Aula não encontrada.");
 var group=old==null?new List<Lesson>():old.GroupKey==""?new List<Lesson>{old}:await db.Lessons.Where(x=>x.GroupKey==old.GroupKey).ToListAsync();
 if(old!=null&&(group.Any(x=>x.Status=="Concluída")||!group.Any(x=>x.Status=="Agendada")||group.Any(x=>x.Start<=DateTime.Now)))throw new ArgumentException("Somente aulas futuras ainda agendadas podem ser remarcadas.");
 var sub=N(f,"SubjectId");await SubjectCheck(sub);
 var autoRecipients=old==null&&F(f,"RecipientMode")=="subject";
 var recipients=autoRecipients?(await db.Students.Where(x=>x.Active).Select(x=>new{x.Id,x.SubjectIds}).ToListAsync()).Where(x=>PortalRules.Ids(x.SubjectIds).Contains(sub)).Select(x=>x.Id).ToArray():PortalRules.Ids(F(f,"StudentIds"));
 if(!autoRecipients&&recipients.Length==0&&N(f,"StudentId")>0)recipients=new[]{N(f,"StudentId")};
 if(old!=null){var expected=group.Where(x=>x.Status=="Agendada").Select(x=>x.StudentId).Distinct().Order().ToArray();if(recipients.Length>0&&!recipients.Order().SequenceEqual(expected))throw new ArgumentException("A remarcação mantém os participantes da aula.");recipients=group.Where(x=>x.Status=="Agendada").Select(x=>x.StudentId).ToArray();}
 if(recipients.Length==0||recipients.Length>200)throw new ArgumentException(autoRecipients?"Nenhum aluno ativo está cadastrado nessa matéria (limite: 200).":"Selecione de 1 a 200 alunos para a aula.");
 foreach(var sid in recipients)await Compatible(sid,sub);
 var tid=await SingleTeacher.ProfileIdAsync(db);
 var start=T(f,"Start");var min=N(f,"Minutes");if(min<15||min>240||start<=DateTime.Now)throw new ArgumentException("Escolha um horário futuro e duração entre 15 e 240 minutos.");
 var repeatCount=old==null?Math.Max(1,N(f,"RepeatCount")):1;if(repeatCount>52)throw new ArgumentException("Agende no máximo 52 aulas semanais de uma vez.");
 var starts=Enumerable.Range(0,repeatCount).Select(week=>start.AddDays(week*7)).ToList();
 var excluded=group.Select(x=>x.Id).ToList();var lastEnd=starts[^1].AddMinutes(min);var conflicts=await db.Lessons.Where(x=>!excluded.Contains(x.Id)&&x.Status!="Cancelada"&&x.Start<lastEnd).ToListAsync();
 var conflictDate=starts.FirstOrDefault(date=>conflicts.Any(x=>x.Start<date.AddMinutes(min)&&x.Start.AddMinutes(x.Minutes)>date));
 if(conflictDate!=default)throw new ArgumentException($"Há conflito com outra aula em {conflictDate:dd/MM/yyyy 'às' HH:mm}. Escolha outro horário ou reduza as repetições.");
 var mode=Choice(f,"Mode","Presencial","Online");var notes=F(f,"Notes",1000);var url=F(f,"OnlineUrl",500);if(url!=""&&(!Uri.TryCreate(url,UriKind.Absolute,out var uri)||!(uri.Scheme=="https"||uri.Scheme=="http")))throw new ArgumentException("Informe um link de aula válido, começando com https://.");
 if(old==null){
  foreach(var occurrence in starts){var key=Guid.NewGuid().ToString("N");var rows=recipients.Select(sid=>new Lesson{StudentId=sid,GroupKey=key,TeacherId=tid,SubjectId=sub,Start=occurrence,Minutes=min,Mode=mode,Notes=notes,OnlineUrl=mode=="Online"?url:""}).ToList();db.Lessons.AddRange(rows);}
 }else{var key=old.GroupKey is {Length:>0}?old.GroupKey:Guid.NewGuid().ToString("N");foreach(var x in group){x.GroupKey=key;x.TeacherId=tid;x.SubjectId=sub;x.Start=start;x.Minutes=min;x.Mode=mode;x.Notes=notes;x.OnlineUrl=mode=="Online"?url:"";}}
 break;}
 case "lesson-group-cancel":{
 var lesson=await db.Lessons.FindAsync(id)??throw new ArgumentException("Aula não encontrada.");
 var group=lesson.GroupKey==""?new List<Lesson>{lesson}:await db.Lessons.Where(x=>x.GroupKey==lesson.GroupKey).ToListAsync();
 var reason=Required(f,"CancellationReason",500);foreach(var row in group.Where(x=>x.Status=="Agendada")){row.Status="Cancelada";row.CancellationReason=reason;}
 break;}
 case "lesson-status":{
 var x=await db.Lessons.FindAsync(id)??throw new ArgumentException("Aula não encontrada.");await DemandStudent(x.StudentId);if(Role=="Professor"&&x.TeacherId!=Entity)return Forbid();var status=Choice(f,"Status","Cancelada","Concluída");if(x.Status!="Agendada")throw new ArgumentException("Aula já encerrada.");if(Role=="Aluno"&&(status!="Cancelada"||x.Start<=DateTime.Now))return Forbid();if(status=="Concluída"&&x.Start>DateTime.Now)throw new ArgumentException("A aula ainda não começou.");var reason=status=="Cancelada"?Required(f,"CancellationReason",500):"";x.Status=status;if(status=="Concluída"){x.Attendance=Choice(f,"Attendance","Presente","Falta");x.CancellationReason="";}else x.CancellationReason=reason;break;}
 case "delete-grade":{
 var x=await db.Grades.FindAsync(id)??throw new ArgumentException("Nota não encontrada.");if(Role=="Professor"&&x.TeacherId!=Entity)return Forbid();if(!await CanStudent(x.StudentId))return Forbid();if(F(f,"ConfirmDelete")!="true")throw new ArgumentException("Confirme a exclusão da nota.");db.Grades.Remove(x);break;}
 case "grade":{
 var existing=id==0?null:await db.Grades.FindAsync(id)??throw new ArgumentException("Nota não encontrada.");if(existing!=null&&(Role=="Professor"&&existing.TeacherId!=Entity||!await CanStudent(existing.StudentId)))return Forbid();
 var sid=N(f,"StudentId");if(existing!=null&&sid!=existing.StudentId)throw new ArgumentException("Mantenha o aluno da avaliação.");await DemandStudent(sid);var sub=N(f,"SubjectId");await Compatible(sid,sub);var value=D(f,"Value");if(value<0||value>10)throw new ArgumentException("Nota deve estar entre 0 e 10.");var date=T(f,"Date");if(date.Date>DateTime.Today)throw new ArgumentException("A data da avaliação não pode estar no futuro.");
 var x=existing??new Grade{StudentId=sid,TeacherId=Admin?await SingleTeacher.ProfileIdAsync(db):Entity};x.SubjectId=sub;x.Title=Required(f,"Title",150);x.Value=value;x.Date=date;x.Comment=F(f,"Comment",1000);x.Source=OptionalChoice(f,"Source","Avaliação da professora","Nota da escola","Avaliação da professora");if(existing==null)db.Grades.Add(x);break;}
 case "activity":{
 var existing=id==0?null:await db.Activities.FindAsync(id)??throw new ArgumentException("Atividade não encontrada.");
 var group=existing==null?new List<Activity>():existing.GroupKey==""?new List<Activity>{existing}:await db.Activities.Where(x=>x.GroupKey==existing.GroupKey).ToListAsync();
 var recipients=PortalRules.Ids(F(f,"StudentIds"));if(recipients.Length==0&&N(f,"StudentId")>0)recipients=new[]{N(f,"StudentId")};
 if(existing!=null){
 foreach(var row in group){if(!await CanStudent(row.StudentId)||Role=="Professor"&&row.TeacherId!=Entity&&row.TeacherId!=null)return Forbid();}
 if(group.Any(x=>x.Submitted!=null||x.Status!="Pendente"))throw new ArgumentException("Esta atividade já recebeu uma entrega ou correção. Preserve seu enunciado.");
 recipients=group.Select(x=>x.StudentId).ToArray();}
 if(recipients.Length==0||recipients.Length>200)throw new ArgumentException("Selecione de 1 a 200 alunos.");
 var sub=N(f,"SubjectId");foreach(var sid in recipients)await Compatible(sid,sub);
 var title=Required(f,"Title",150);var description=F(f,"Description",3000);var due=T(f,"Due");var origin=OptionalChoice(f,"Origin","Proposta da professora","Tarefa da escola","Proposta da professora");
 if(description.Length==0&&attachment==null&&existing?.AssignmentKey==null)throw new ArgumentException("Escreva o enunciado ou anexe a atividade.");
 string? key=existing?.AssignmentKey,name=existing?.AssignmentName;if(attachment!=null){replaced=existing?.AssignmentKey;var file=await Upload(attachment);written=file.key;key=file.key;name=file.name;}
 var groupKey=existing?.GroupKey is {Length:>0}?existing.GroupKey:Guid.NewGuid().ToString("N");
 if(existing==null){var tid=Admin?await SingleTeacher.ProfileIdAsync(db):Entity;group=recipients.Select(sid=>new Activity{StudentId=sid,TeacherId=tid}).ToList();db.Activities.AddRange(group);}
 foreach(var x in group){x.GroupKey=groupKey;x.SubjectId=sub;x.Title=title;x.Description=description;x.Due=due;x.Origin=origin;x.AssignmentKey=key;x.AssignmentName=name;}
 published=group[0];break;}
 case "submit":{
 if(Role!="Aluno")return Forbid();var x=await db.Activities.FindAsync(id)??throw new ArgumentException("Atividade não encontrada.");await DemandStudent(x.StudentId);if(x.Status=="Corrigida")throw new ArgumentException("Atividade já corrigida.");var answer=F(f,"Answer",5000);if(answer.Length==0&&attachment==null&&x.FileKey==null)throw new ArgumentException("Escreva uma resposta ou envie um arquivo.");if(attachment!=null){replaced=x.FileKey;var uploaded=await Upload(attachment);written=uploaded.key;x.FileKey=uploaded.key;x.FileName=uploaded.name;}x.Answer=answer;x.Submitted=DateTime.Now;x.Status="Entregue";break;}
 case "review":{var x=await db.Activities.FindAsync(id)??throw new ArgumentException("Atividade não encontrada.");await DemandStudent(x.StudentId);if(Role=="Professor"&&x.TeacherId!=Entity&&x.TeacherId!=null)return Forbid();if(x.Status!="Entregue")throw new ArgumentException("Aguarde a entrega do aluno.");var score=D(f,"Score");if(score<0||score>10)throw new ArgumentException("Nota deve estar entre 0 e 10.");x.Score=score;x.Feedback=Required(f,"Feedback",3000);x.Status="Corrigida";break;}
 case "material":{var sid=N(f,"StudentId");await DemandStudent(sid);var sub=N(f,"SubjectId");var tid=Admin?await SingleTeacher.ProfileIdAsync(db):Entity;await Compatible(sid,sub,tid);if(!await db.Enrollments.AnyAsync(x=>x.StudentId==sid&&x.TeacherId==tid))throw new ArgumentException("Professor não vinculado ao aluno.");var title=Required(f,"Title",150);if(attachment==null)throw new ArgumentException("Selecione um arquivo.");var uploaded=await Upload(attachment);written=uploaded.key;db.Materials.Add(new Material{StudentId=sid,TeacherId=tid,SubjectId=sub,Title=title,FileKey=uploaded.key,FileName=uploaded.name});break;}
 case "delete-progress":{
 var x=await db.Progress.FindAsync(id)??throw new ArgumentException("Progresso não encontrado.");if(Role=="Professor"&&x.TeacherId!=Entity||!await CanStudent(x.StudentId))return Forbid();if(F(f,"ConfirmDelete")!="true")throw new ArgumentException("Confirme a exclusão do registro.");db.Progress.Remove(x);break;}
 case "progress":{
 var existing=id==0?null:await db.Progress.FindAsync(id)??throw new ArgumentException("Progresso não encontrado.");if(existing!=null&&(Role=="Professor"&&existing.TeacherId!=Entity||!await CanStudent(existing.StudentId)))return Forbid();
 var sid=N(f,"StudentId");if(existing!=null&&sid!=existing.StudentId)throw new ArgumentException("Mantenha o aluno do registro.");await DemandStudent(sid);var tid=Admin?await SingleTeacher.ProfileIdAsync(db):Entity;
 if(!await db.Teachers.AnyAsync(x=>x.Id==tid&&x.Active)||!await db.Enrollments.AnyAsync(x=>x.StudentId==sid&&x.TeacherId==tid))throw new ArgumentException("Selecione um professor vinculado ao aluno.");
 var student=await db.Students.FindAsync(sid);var service=Choice(f,"ServiceType","Reforço escolar","Acompanhamento escolar");if(student!.ServiceType!="Ambos"&&student.ServiceType!=service)throw new ArgumentException("Selecione o atendimento definido no cadastro do aluno.");
 var subject=N(f,"SubjectId");if(subject>0)await Compatible(sid,subject,tid);var date=T(f,"Date");if(date.Date>DateTime.Today)throw new ArgumentException("A data do progresso não pode estar no futuro.");
 var lessonId=N(f,"LessonId");if(lessonId>0&&!await db.Lessons.AnyAsync(x=>x.Id==lessonId&&x.StudentId==sid&&x.Start<=DateTime.Now&&x.Status=="Concluída"&&(subject==0||x.SubjectId==subject)))throw new ArgumentException("Escolha uma aula concluída deste aluno e professor.");
 var x=existing??new Progress{StudentId=sid};x.TeacherId=tid;x.Date=date;x.ServiceType=service;x.SubjectId=subject>0?subject:null;x.Text=F(f,"Text",3000);x.InternalNotes=F(f,"InternalNotes",3000);x.LessonId=lessonId>0?lessonId:null;x.Goal=Required(f,"Goal",500);x.Worked=Required(f,"Worked",1000);x.Evolution=Required(f,"Evolution",1000);x.NextSteps=F(f,"NextSteps",1000);
 x.Difficulty=service=="Reforço escolar"?Required(f,"Difficulty",1000):"";x.Tasks=service=="Acompanhamento escolar"?F(f,"Tasks",1000):"";x.Autonomy=service=="Acompanhamento escolar"?Choice(f,"Autonomy","Precisa de ajuda frequente","Realiza com alguma ajuda","Realiza com autonomia"):"";
 if(existing==null)db.Progress.Add(x);break;}
 case "payment":{var sid=N(f,"StudentId");await DemandStudent(sid);var amount=Money(f,"Amount");if(amount<=0||amount>1000000)throw new ArgumentException("Informe um valor positivo válido.");db.Payments.Add(new Payment{StudentId=sid,Amount=amount,Due=T(f,"Due"),Method="",Notes=F(f,"Notes",1000)});break;}
 case "paid":{var x=await db.Payments.FindAsync(id)??throw new ArgumentException("Registro não encontrado.");if(x.Paid!=null)throw new ArgumentException("Pagamento já registrado.");var paid=T(f,"Paid");if(paid.Date>DateTime.Today)throw new ArgumentException("Pagamento não pode estar no futuro.");x.History+=$"{DateTime.Now:dd/MM/yyyy HH:mm} · {User.Identity?.Name??"Administrador"}: pagamento registrado em {paid:dd/MM/yyyy}.\n";x.Paid=paid;x.Method=OptionalChoice(f,"Method","","PIX","Dinheiro","Cartão","Boleto","Transferência");break;}
 case "payment-edit":{
 var x=await db.Payments.FindAsync(id)??throw new ArgumentException("Mensalidade não encontrada.");var reason=Required(f,"Reason",500);var amount=Money(f,"Amount");if(amount<=0||amount>1000000)throw new ArgumentException("Informe um valor positivo válido.");var due=T(f,"Due");
 x.History+=$"{DateTime.Now:dd/MM/yyyy HH:mm} · {User.Identity?.Name??"Administrador"}: valor {x.Amount:C} → {amount:C}; vencimento {x.Due:dd/MM/yyyy} → {due:dd/MM/yyyy}. Motivo: {reason}\n";x.Amount=amount;x.Due=due;x.Notes=F(f,"Notes",1000);break;}
 case "unpay":{
 var x=await db.Payments.FindAsync(id)??throw new ArgumentException("Mensalidade não encontrada.");if(x.Paid==null)throw new ArgumentException("Esta mensalidade já está em aberto.");var reason=Required(f,"Reason",500);
 x.History+=$"{DateTime.Now:dd/MM/yyyy HH:mm} · {User.Identity?.Name??"Administrador"}: desfeito pagamento de {x.Paid:dd/MM/yyyy} ({x.Method}). Motivo: {reason}\n";x.Paid=null;x.Method="";break;}
 case "user-password":{
 var x=await db.Users.FindAsync(id)??throw new ArgumentException("Acesso não encontrado.");var password=Required(f,"Password",256);if(password.Length<10)throw new ArgumentException("Senha precisa ter pelo menos 10 caracteres.");
 if(!Admin&&x.Role=="Admin")return Forbid();
 if(x.Id.ToString()==User.FindFirstValue(ClaimTypes.NameIdentifier)&&new PasswordHasher<PortalUser>().VerifyHashedPassword(x,x.PasswordHash,Required(f,"CurrentPassword",256))==PasswordVerificationResult.Failed)throw new ArgumentException("Senha atual incorreta.");
 x.PasswordHash=new PasswordHasher<PortalUser>().HashPassword(x,password);x.Stamp=Guid.NewGuid().ToString();break;}
 case "user-status":{
 var x=await db.Users.FindAsync(id)??throw new ArgumentException("Acesso não encontrado.");if(!Admin&&x.Role=="Admin")return Forbid();var active=F(f,"Active")=="true";if(!active&&x.Id.ToString()==User.FindFirstValue(ClaimTypes.NameIdentifier))throw new ArgumentException("Não desative sua própria conta.");x.Active=active;x.Stamp=Guid.NewGuid().ToString();break;}
 case "user":{
 var x=id==0?new PortalUser():await db.Users.FindAsync(id)??throw new ArgumentException("Usuário não encontrado.");var role=Choice(f,"Role","Admin","Professor","Aluno");var eid=N(f,"EntityId");
 if(!Admin&&(role=="Admin"||x.Role=="Admin"))return Forbid();
 var student=role=="Aluno"?await db.Students.SingleOrDefaultAsync(a=>a.Id==eid&&a.Active):null;if(role=="Aluno"&&student==null)throw new ArgumentException("Selecione um aluno ativo.");
 if(!Admin&&role=="Aluno"&&!await CanStudent(eid))return Forbid();
 var teacher=role=="Professor"&&eid>0?await db.Teachers.SingleOrDefaultAsync(a=>a.Id==eid&&a.Active):null;if(role=="Professor"&&eid>0&&teacher==null)throw new ArgumentException("Selecione um professor ativo.");
 var email=Required(f,"Email",150).ToLowerInvariant();if(!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))throw new ArgumentException("E-mail inválido.");if(await db.Users.AnyAsync(a=>a.Email==email&&a.Id!=id))throw new ArgumentException("E-mail já utilizado.");if(role!="Admin"&&eid>0&&await db.Users.AnyAsync(a=>a.Role==role&&a.EntityId==eid&&a.Id!=id))throw new ArgumentException("Este cadastro já possui uma conta.");
 var active=F(f,"Active")=="true";if(x.Id.ToString()==User.FindFirstValue(ClaimTypes.NameIdentifier)&&(!active||role!="Admin"))throw new ArgumentException("Não desative nem rebaixe sua própria conta.");var password=F(f,"Password",256);if((id==0||password.Length>0)&&password.Length<10)throw new ArgumentException("Senha precisa ter pelo menos 10 caracteres.");
 var name=role=="Aluno"?student!.Name:role=="Professor"&&teacher!=null?teacher.Name:FullName(f,"Name");
 if(role=="Professor"&&teacher==null){teacher=new Teacher{Name=name,Email=email,SubjectIds=string.Join(',',await db.Subjects.Where(a=>a.Active).Select(a=>a.Id).ToListAsync())};db.Teachers.Add(teacher);await db.SaveChangesAsync();eid=teacher.Id;}else if(teacher!=null)teacher.Email=email;
 if(role=="Professor"){
  var selected=PortalRules.Ids(F(f,"TeacherStudentIds"));if(selected.Length!=await db.Students.CountAsync(a=>selected.Contains(a.Id)&&a.Active))throw new ArgumentException("Selecione somente alunos ativos para o professor.");
  if(!Admin)foreach(var sid in selected)if(!await CanStudent(sid))return Forbid();
  var links=await db.Enrollments.Where(a=>a.TeacherId==eid).ToListAsync();db.Enrollments.RemoveRange(links.Where(a=>!selected.Contains(a.StudentId)));db.Enrollments.AddRange(selected.Where(sid=>links.All(a=>a.StudentId!=sid)).Select(sid=>new Enrollment{StudentId=sid,TeacherId=eid}));
 }
 x.Name=name;x.Email=email;x.Role=role;x.EntityId=role=="Admin"?null:eid;x.Active=active;x.Stamp=Guid.NewGuid().ToString();if(password.Length>0)x.PasswordHash=new PasswordHasher<PortalUser>().HashPassword(x,password);if(id==0)db.Users.Add(x);break;}
 default: return BadRequest();
 }
 await db.SaveChangesAsync();await transaction.CommitAsync();if(published!=null)returnActivityId=published.Id;if(replaced!=null&&!await db.Activities.AnyAsync(x=>x.FileKey==replaced||x.AssignmentKey==replaced)&&!await db.Materials.AnyAsync(x=>x.FileKey==replaced)){try{await files.DeleteAsync(replaced);}catch(HttpRequestException){/* The saved record remains valid; obsolete object can be cleaned later. */}}TempData["Success"]=kind switch{"student"=>"Cadastro do aluno salvo.","teacher"=>"Cadastro do professor salvo.","subject"=>"Matéria atualizada.","activity"=>"Atividade salva para os alunos selecionados.","progress"=>"Progresso registrado.","lesson"=>"Agendamento salvo.","paid"=>"Pagamento registrado.",_=>"Alteração salva."};
 }catch(UnauthorizedAccessException){if(written!=null)await files.DeleteAsync(written);return Forbid();}
 catch(DbUpdateException){if(written!=null)await files.DeleteAsync(written);TempData["Error"]="Este registro já foi salvo ou foi alterado em outra janela. Atualize a página antes de tentar novamente.";}
 catch(ArgumentException e){if(written!=null)await files.DeleteAsync(written);TempData["Error"]=e.Message;TempData["DraftKind"]=kind;TempData["DraftId"]=N(f,"Id").ToString();}
 if(F(f,"Continue")=="true"&&kind=="grade"&&TempData["Error"]==null)return RedirectToAction(nameof(Index),new{section,again=true});
 if(returnActivityId>0&&(kind is "activity" or "submit" or "review"))return RedirectToAction(nameof(Index),new{section="atividade",activityId=returnActivityId});
 return RedirectToAction(nameof(Index),new{section});
 }
 [HttpGet("horarios")] public async Task<IActionResult> Slots(int studentId,int teacherId,int subjectId,DateTime date,int minutes=60,int lessonId=0){
 try{
 if(!Admin)return Forbid();teacherId=await SingleTeacher.ProfileIdAsync(db);
 if(!new[]{30,60,90,120}.Contains(minutes))return BadRequest();
 await Compatible(studentId,subjectId,teacherId);
 var slots=new List<string>();var excluded=new List<int>();
 if(lessonId>0){var lesson=await db.Lessons.FindAsync(lessonId);if(lesson==null||lesson.StudentId!=studentId)return Forbid();excluded=lesson.GroupKey==""?new List<int>{lesson.Id}:await db.Lessons.Where(x=>x.GroupKey==lesson.GroupKey).Select(x=>x.Id).ToListAsync();}
 var from=date.Date;var until=from.AddDays(1);var lessons=await db.Lessons.Where(x=>!excluded.Contains(x.Id)&&x.Status!="Cancelada"&&x.Start<until&&x.Start>=from.AddDays(-1)).ToListAsync();
 for(var start=from;start.AddMinutes(minutes)<=until;start=start.AddMinutes(30))if(start>DateTime.Now&&!lessons.Any(x=>x.Start<start.AddMinutes(minutes)&&x.Start.AddMinutes(x.Minutes)>start))slots.Add(start.ToString("yyyy-MM-ddTHH:mm"));
 return Json(slots);
 }catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException){return Json(Array.Empty<string>());}
 }
 async Task<(string key,string name)> Upload(IFormFile file){
 var bytes=await AttachmentRules.ReadAsync(file);var ext=Path.GetExtension(file.FileName).ToLowerInvariant();
 var key=Guid.NewGuid().ToString("N")+ext;
 await files.PutAsync(key,bytes,"application/octet-stream");
 return(key,Path.GetFileName(file.FileName));
 }
 [HttpGet("arquivo/{type}/{id:int}")] public async Task<IActionResult> Download(string type,int id,bool preview=false){string key,name;int sid;int? tid;
 if(type=="material"){var x=await db.Materials.FindAsync(id);if(x==null)return NotFound();key=x.FileKey;name=x.FileName;sid=x.StudentId;tid=x.TeacherId;}
 else if(type=="enunciado"){var x=await db.Activities.FindAsync(id);if(x?.AssignmentKey==null)return NotFound();key=x.AssignmentKey;name=x.AssignmentName??"atividade";sid=x.StudentId;tid=x.TeacherId;}
 else if(type=="atividade"){var x=await db.Activities.FindAsync(id);if(x?.FileKey==null)return NotFound();key=x.FileKey;name=x.FileName??"entrega";sid=x.StudentId;tid=x.TeacherId;}else return NotFound();
 if(!await CanStudent(sid)||Role=="Professor"&&tid!=Entity&&tid!=null)return Forbid();var bytes=await files.GetAsync(key);if(bytes==null)return NotFound();if(preview){var mime=Path.GetExtension(name).ToLowerInvariant() switch{".pdf"=>"application/pdf",".png"=>"image/png",".jpg" or ".jpeg"=>"image/jpeg",".txt"=>"text/plain; charset=utf-8",_=>null};if(mime!=null){Response.Headers["X-Content-Type-Options"]="nosniff";Response.Headers["Content-Security-Policy"]="sandbox; default-src 'none'";return File(bytes,mime);}}
 return File(bytes,"application/octet-stream",name);}
 [Authorize(Roles="Admin"),HttpGet("relatorio")] public async Task<IActionResult> Report(){var rows=await db.Students.OrderBy(x=>x.Name).ToListAsync();static string Cell(string s)=>"\""+((s.Length>0&&"=+-@".Contains(s[0]))?"'"+s:s).Replace("\"","\"\"")+"\"";var csv="Nome;Nível;Turno;Responsável;Situação\r\n"+string.Join("\r\n",rows.Select(x=>string.Join(";",new[]{x.Name,x.Level,x.Shift,x.Guardian,x.Status}.Select(Cell))));return File(System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csv)).ToArray(),"text/csv","alunos.csv");}
 [Authorize(Roles="Admin"),HttpPost("backup"),ValidateAntiForgeryToken] public async Task<IActionResult> Backup(){if(!string.IsNullOrWhiteSpace(config["DATABASE_URL"]))return BadRequest("No modo online, faça o backup fora da Vercel com scripts/online-backup.sh.");var temp=Path.Combine(Path.GetTempPath(),"educamais-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(temp);try{foreach(var file in new[]{"sistema-pedagogico.db","educamais.db"}){var source=Path.Combine(paths.DataDirectory,file);if(!System.IO.File.Exists(source))continue;using var a=new SqliteConnection(new SqliteConnectionStringBuilder{DataSource=source}.ToString());using var b=new SqliteConnection(new SqliteConnectionStringBuilder{DataSource=Path.Combine(temp,file)}.ToString());a.Open();b.Open();a.BackupDatabase(b);}var dir=Path.Combine(paths.DataDirectory,"Arquivos");if(Directory.Exists(dir)){Directory.CreateDirectory(Path.Combine(temp,"Arquivos"));foreach(var p in Directory.GetFiles(dir))System.IO.File.Copy(p,Path.Combine(temp,"Arquivos",Path.GetFileName(p)));}var zip=temp+".zip";ZipFile.CreateFromDirectory(temp,zip);var bytes=System.IO.File.ReadAllBytes(zip);System.IO.File.Delete(zip);var setting=await db.Settings.FirstOrDefaultAsync(x=>x.Key=="last-backup");if(setting==null){setting=new PortalSetting{Key="last-backup"};db.Settings.Add(setting);}setting.Value=DateTime.Now.ToString("dd/MM/yyyy HH:mm");await db.SaveChangesAsync();return File(bytes,"application/zip","Tutoring-backup-"+DateTime.Now.ToString("yyyyMMdd-HHmm")+".zip");}finally{Directory.Delete(temp,true);}}
}
