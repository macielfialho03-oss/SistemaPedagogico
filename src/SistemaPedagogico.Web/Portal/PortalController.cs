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
public class PortalController(EducaDb db,StoragePaths paths):Controller {
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
 if(tid!=null){var t=await db.Teachers.FindAsync(tid);if(t==null||!t.Active||!PortalRules.Ids(t.SubjectIds).Contains(subject)||!await db.Enrollments.AnyAsync(x=>x.StudentId==sid&&x.TeacherId==tid))throw new ArgumentException("Escolha um professor vinculado ao aluno que lecione esta matéria.");}
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
 [HttpGet("")] public async Task<IActionResult> Index(string section="painel",string q=""){
 var adminOnly=new[]{"professores","materias","mensalidades","usuarios","relatorios","backup"};
 if(adminOnly.Contains(section)&&!Admin)return Forbid();
 var allowed=new[]{"painel","alunos","professores","materias","aulas","notas","atividades","materiais","progresso","mensalidades","usuarios","relatorios","backup"};if(!allowed.Contains(section))return NotFound();
 if(section=="alunos"&&Role=="Aluno")return Forbid();
 var ids=Admin?await db.Students.Select(x=>x.Id).ToListAsync():Role=="Aluno"?new List<int>{Entity}:await db.Enrollments.Where(x=>x.TeacherId==Entity).Select(x=>x.StudentId).ToListAsync();
 var vm=new PortalVm{Section=section,Role=Role,Entity=Entity,Query=q,Students=await db.Students.Where(x=>ids.Contains(x.Id)).OrderBy(x=>x.Name).ToListAsync(),Teachers=await db.Teachers.OrderBy(x=>x.Name).ToListAsync(),Subjects=await db.Subjects.OrderBy(x=>x.Name).ToListAsync(),Enrollments=await db.Enrollments.Where(x=>ids.Contains(x.StudentId)).ToListAsync()};
 // Aluno só recebe professores vinculados e suas matérias.
 if(Role=="Aluno"){var teacherIds=vm.Enrollments.Select(x=>x.TeacherId).ToList();vm.Teachers=vm.Teachers.Where(x=>teacherIds.Contains(x.Id)).ToList();}
 vm.Lessons=await db.Lessons.Where(x=>ids.Contains(x.StudentId)&&(Role!="Professor"||x.TeacherId==Entity)).OrderBy(x=>x.Start).ToListAsync();
 vm.Grades=await db.Grades.Where(x=>ids.Contains(x.StudentId)&&(Role!="Professor"||x.TeacherId==Entity||x.TeacherId==null)).OrderByDescending(x=>x.Date).ToListAsync();
 vm.Activities=await db.Activities.Where(x=>ids.Contains(x.StudentId)&&(Role!="Professor"||x.TeacherId==Entity||x.TeacherId==null)).OrderBy(x=>x.Due).ToListAsync();
 vm.Materials=await db.Materials.Where(x=>ids.Contains(x.StudentId)&&(Role!="Professor"||x.TeacherId==Entity)).OrderByDescending(x=>x.Created).ToListAsync();
 vm.Progress=await db.Progress.Where(x=>ids.Contains(x.StudentId)&&(Role!="Professor"||x.TeacherId==Entity)).OrderByDescending(x=>x.Date).ToListAsync();
 if(Admin){vm.Payments=await db.Payments.OrderBy(x=>x.Due).ToListAsync();vm.Users=await db.Users.OrderBy(x=>x.Name).ToListAsync();}
 return View("Index",vm);
 }
 [HttpPost("salvar"),ValidateAntiForgeryToken,RequestSizeLimit(12_000_000)] public async Task<IActionResult> Save(string kind,IFormCollection f,IFormFile? attachment){
 var section=kind switch{"student" or "student-status" or "delete-student"=>"alunos","teacher"=>"professores","subject" or "subject-status"=>"materias","enrollment"=>"alunos","lesson" or "lesson-status"=>"aulas","grade"=>"notas","activity" or "submit" or "review"=>"atividades","material"=>"materiais","progress"=>"progresso","payment" or "paid"=>"mensalidades","user" or "user-password" or "user-status"=>"usuarios",_=>"painel"};
 string? written=null;
 try {
 await using var transaction = await db.Database.BeginTransactionAsync();
 if(new[]{"student","student-status","delete-student","teacher","subject","subject-status","enrollment","payment","paid","user","user-password","user-status"}.Contains(kind)&&!Admin)return Forbid();
 if(new[]{"grade","activity","review","material","progress"}.Contains(kind)&&!Staff)return Forbid();
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
 var assignedTeacher=N(f,"AssignedTeacherId");if(assignedTeacher>0){var teacher=await db.Teachers.FindAsync(assignedTeacher);if(teacher==null||!teacher.Active)throw new ArgumentException("Selecione um professor ativo.");if(!PortalRules.Ids(teacher.SubjectIds).Intersect(PortalRules.Ids(x.SubjectIds)).Any())throw new ArgumentException("O professor deve lecionar pelo menos uma matéria do aluno.");await db.SaveChangesAsync();db.Enrollments.Add(new Enrollment{StudentId=x.Id,TeacherId=assignedTeacher});}
 var months=N(f,"BillingMonths");if(months<0||months>60)throw new ArgumentException("Escolha entre 1 e 60 mensalidades, ou deixe sem mensalidades.");
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
 case "teacher":{
 var x=id==0?new Teacher():await db.Teachers.FindAsync(id)??throw new ArgumentException("Professor não encontrado.");var name=FullName(f,"Name");var sub=await Subjects(f);var days=string.Join(",",PortalRules.Ids(F(f,"Days")).Where(x=>x<=7));var sh=N(f,"StartHour");var eh=N(f,"EndHour");if(sh<0||eh>24||sh>=eh||days.Length==0)throw new ArgumentException("Marque pelo menos um dia e escolha um horário de término posterior ao início.");x.Name=name;x.Email=ContactEmail(f);x.Phone=Phone(f,"Phone");x.SubjectIds=sub;x.Days=days;x.StartHour=sh;x.EndHour=eh;x.Active=F(f,"Active")=="true";if(id==0)db.Teachers.Add(x);break;}
 case "subject":{var name=Required(f,"Name",80);if(await db.Subjects.AnyAsync(x=>x.Name==name&&x.Id!=id))throw new ArgumentException("Matéria já cadastrada.");var x=id==0?new Subject():await db.Subjects.FindAsync(id)??throw new ArgumentException("Matéria não encontrada.");x.Name=name;x.Description=F(f,"Description",500);x.Active=F(f,"Active")=="true";if(id==0)db.Subjects.Add(x);break;}
 case "subject-status":{var x=await db.Subjects.FindAsync(id)??throw new ArgumentException("Matéria não encontrada.");x.Active=F(f,"Active")=="true";break;}
 case "enrollment":{var sid=N(f,"StudentId");var tid=N(f,"TeacherId");await DemandStudent(sid);if(!await db.Teachers.AnyAsync(x=>x.Id==tid&&x.Active))throw new ArgumentException("Professor inválido.");if(!await db.Enrollments.AnyAsync(x=>x.StudentId==sid&&x.TeacherId==tid))db.Enrollments.Add(new Enrollment{StudentId=sid,TeacherId=tid});break;}
 case "lesson":{
 var old=id==0?null:await db.Lessons.FindAsync(id)??throw new ArgumentException("Aula não encontrada.");if(old!=null){await DemandStudent(old.StudentId);if(Role=="Professor"&&old.TeacherId!=Entity)return Forbid();if(old.Status!="Agendada")throw new ArgumentException("Somente aulas agendadas podem ser remarcadas.");if(Role=="Aluno"&&old.Start<=DateTime.Now)throw new ArgumentException("A aula já começou.");}
 var sid=Role=="Aluno"?Entity:N(f,"StudentId");await DemandStudent(sid);if(old!=null&&sid!=old.StudentId)throw new ArgumentException("Ao remarcar, mantenha o aluno.");var tid=Role=="Professor"?Entity:N(f,"TeacherId");var teacher=await db.Teachers.FindAsync(tid);if(teacher==null||!teacher.Active)throw new ArgumentException("Professor inválido.");if(!await db.Enrollments.AnyAsync(x=>x.StudentId==sid&&x.TeacherId==tid))throw new ArgumentException("Vincule o aluno ao professor antes de agendar.");var sub=N(f,"SubjectId");await SubjectCheck(sub);if(!PortalRules.Ids(teacher.SubjectIds).Contains(sub))throw new ArgumentException("Professor não leciona esta matéria.");var student=await db.Students.FindAsync(sid);if(!PortalRules.Ids(student!.SubjectIds).Contains(sub))throw new ArgumentException("Matéria não incluída no cadastro do aluno.");
 var start=T(f,"Start");var min=N(f,"Minutes");if(min<15||min>240||start<=DateTime.Now)throw new ArgumentException("Escolha um horário futuro e duração entre 15 e 240 minutos.");var end=start.AddMinutes(min);var day=(int)start.DayOfWeek;day=day==0?7:day;if(!PortalRules.Ids(teacher.Days).Contains(day)||start<start.Date.AddHours(teacher.StartHour)||end>start.Date.AddHours(teacher.EndHour))throw new ArgumentException("Horário fora da disponibilidade do professor.");
 var conflicts=await db.Lessons.Where(x=>x.Id!=id&&x.Status!="Cancelada"&&(x.StudentId==sid||x.TeacherId==tid)&&x.Start<end).ToListAsync();if(conflicts.Any(x=>x.Start.AddMinutes(x.Minutes)>start))throw new ArgumentException("Há conflito de horário para o aluno ou professor.");
 var x=old??new Lesson();x.StudentId=sid;x.TeacherId=tid;x.SubjectId=sub;x.Start=start;x.Minutes=min;x.Mode=Choice(f,"Mode","Presencial","Online");x.Notes=F(f,"Notes",1000);var url=F(f,"OnlineUrl",500);if(url!=""&&(!Uri.TryCreate(url,UriKind.Absolute,out var uri)||!(uri.Scheme=="https"||uri.Scheme=="http")))throw new ArgumentException("Informe um link de aula válido, começando com https://.");x.OnlineUrl=x.Mode=="Online"?url:"";if(old==null)db.Lessons.Add(x);break;}
 case "lesson-status":{
 var x=await db.Lessons.FindAsync(id)??throw new ArgumentException("Aula não encontrada.");await DemandStudent(x.StudentId);if(Role=="Professor"&&x.TeacherId!=Entity)return Forbid();var status=Choice(f,"Status","Cancelada","Concluída");if(x.Status!="Agendada")throw new ArgumentException("Aula já encerrada.");if(Role=="Aluno"&&(status!="Cancelada"||x.Start<=DateTime.Now))return Forbid();if(status=="Concluída"&&x.Start>DateTime.Now)throw new ArgumentException("A aula ainda não começou.");x.Status=status;if(status=="Concluída")x.Attendance=Choice(f,"Attendance","Presente","Falta");break;}
 case "grade":{
 var sid=N(f,"StudentId");await DemandStudent(sid);var sub=N(f,"SubjectId");await Compatible(sid,sub);var value=D(f,"Value");if(value<0||value>10)throw new ArgumentException("Nota deve estar entre 0 e 10.");db.Grades.Add(new Grade{StudentId=sid,TeacherId=Role=="Professor"?Entity:null,SubjectId=sub,Title=Required(f,"Title",150),Value=value,Date=T(f,"Date"),Comment=F(f,"Comment",1000),Source=OptionalChoice(f,"Source","Avaliação da professora","Nota da escola","Avaliação da professora")});break;}
 case "activity":{
 var recipients=PortalRules.Ids(F(f,"StudentIds"));if(recipients.Length==0)recipients=new[]{N(f,"StudentId")};
 var existing=id==0?null:await db.Activities.FindAsync(id)??throw new ArgumentException("Atividade não encontrada.");
 if(existing!=null){await DemandStudent(existing.StudentId);if(Role=="Professor"&&existing.TeacherId!=Entity&&existing.TeacherId!=null)return Forbid();if(existing.Submitted!=null)throw new ArgumentException("Esta atividade já recebeu uma entrega. Preserve seu enunciado.");recipients=new[]{existing.StudentId};}
 var sub=N(f,"SubjectId");foreach(var sid in recipients)await Compatible(sid,sub);
 var title=Required(f,"Title",150);var description=F(f,"Description",3000);var due=T(f,"Due");var origin=OptionalChoice(f,"Origin","Proposta da professora","Tarefa da escola","Proposta da professora");
 if(description.Length==0&&attachment==null&&existing?.AssignmentKey==null)throw new ArgumentException("Escreva o enunciado ou anexe a atividade.");
 string? key=existing?.AssignmentKey,name=existing?.AssignmentName;if(attachment!=null){var file=await Upload(attachment);written=file.key;key=file.key;name=file.name;}
 foreach(var sid in recipients){var x=existing??new Activity{StudentId=sid,TeacherId=Role=="Professor"?Entity:null};x.SubjectId=sub;x.Title=title;x.Description=description;x.Due=due;x.Origin=origin;x.AssignmentKey=key;x.AssignmentName=name;if(existing==null)db.Activities.Add(x);}
 break;}
 case "submit":{
 if(Role!="Aluno")return Forbid();var x=await db.Activities.FindAsync(id)??throw new ArgumentException("Atividade não encontrada.");await DemandStudent(x.StudentId);if(x.Status=="Corrigida")throw new ArgumentException("Atividade já corrigida.");var answer=F(f,"Answer",5000);if(answer.Length==0&&attachment==null)throw new ArgumentException("Escreva uma resposta ou envie um arquivo.");if(attachment!=null){var uploaded=await Upload(attachment);written=uploaded.key;x.FileKey=uploaded.key;x.FileName=uploaded.name;}x.Answer=answer;x.Submitted=DateTime.Now;x.Status="Entregue";break;}
 case "review":{var x=await db.Activities.FindAsync(id)??throw new ArgumentException("Atividade não encontrada.");await DemandStudent(x.StudentId);if(Role=="Professor"&&x.TeacherId!=Entity&&x.TeacherId!=null)return Forbid();if(x.Status!="Entregue")throw new ArgumentException("Aguarde a entrega do aluno.");var score=D(f,"Score");if(score<0||score>10)throw new ArgumentException("Nota deve estar entre 0 e 10.");x.Score=score;x.Feedback=Required(f,"Feedback",3000);x.Status="Corrigida";break;}
 case "material":{var sid=N(f,"StudentId");await DemandStudent(sid);var sub=N(f,"SubjectId");var tid=Role=="Professor"?Entity:N(f,"TeacherId");await Compatible(sid,sub,tid);if(!await db.Enrollments.AnyAsync(x=>x.StudentId==sid&&x.TeacherId==tid))throw new ArgumentException("Professor não vinculado ao aluno.");var title=Required(f,"Title",150);if(attachment==null)throw new ArgumentException("Selecione um arquivo.");var uploaded=await Upload(attachment);written=uploaded.key;db.Materials.Add(new Material{StudentId=sid,TeacherId=tid,SubjectId=sub,Title=title,FileKey=uploaded.key,FileName=uploaded.name});break;}
 case "progress":{
 var sid=N(f,"StudentId");await DemandStudent(sid);var tid=Role=="Professor"?Entity:N(f,"TeacherId");
 if(!await db.Teachers.AnyAsync(x=>x.Id==tid&&x.Active)||!await db.Enrollments.AnyAsync(x=>x.StudentId==sid&&x.TeacherId==tid))throw new ArgumentException("Selecione um professor vinculado ao aluno.");
 var student=await db.Students.FindAsync(sid);var service=Choice(f,"ServiceType","Reforço escolar","Acompanhamento escolar");if(student!.ServiceType!="Ambos"&&student.ServiceType!=service)throw new ArgumentException("Selecione o atendimento definido no cadastro do aluno.");
 var subject=N(f,"SubjectId");if(subject>0)await Compatible(sid,subject,tid);
 var x=new Progress{StudentId=sid,TeacherId=tid,Date=T(f,"Date"),ServiceType=service,SubjectId=subject>0?subject:null,Text=F(f,"Text",3000),Goal=Required(f,"Goal",500),Worked=Required(f,"Worked",1000),Evolution=Required(f,"Evolution",1000),NextSteps=F(f,"NextSteps",1000)};
 if(service=="Reforço escolar")x.Difficulty=Required(f,"Difficulty",1000);else{x.Tasks=F(f,"Tasks",1000);x.Autonomy=Choice(f,"Autonomy","Precisa de ajuda frequente","Realiza com alguma ajuda","Realiza com autonomia");}
 db.Progress.Add(x);break;}
 case "payment":{var sid=N(f,"StudentId");await DemandStudent(sid);var amount=Money(f,"Amount");if(amount<=0||amount>1000000)throw new ArgumentException("Informe um valor positivo válido.");db.Payments.Add(new Payment{StudentId=sid,Amount=amount,Due=T(f,"Due"),Method="",Notes=F(f,"Notes",1000)});break;}
 case "paid":{var x=await db.Payments.FindAsync(id)??throw new ArgumentException("Registro não encontrado.");if(x.Paid!=null)throw new ArgumentException("Pagamento já registrado.");var paid=T(f,"Paid");if(paid.Date>DateTime.Today)throw new ArgumentException("Pagamento não pode estar no futuro.");x.Paid=paid;x.Method=OptionalChoice(f,"Method","","PIX","Dinheiro","Cartão","Boleto","Transferência");break;}
 case "user-password":{
 var x=await db.Users.FindAsync(id)??throw new ArgumentException("Acesso não encontrado.");var password=Required(f,"Password",256);if(password.Length<10)throw new ArgumentException("Senha precisa ter pelo menos 10 caracteres.");x.PasswordHash=new PasswordHasher<PortalUser>().HashPassword(x,password);x.Stamp=Guid.NewGuid().ToString();break;}
 case "user-status":{
 var x=await db.Users.FindAsync(id)??throw new ArgumentException("Acesso não encontrado.");var active=F(f,"Active")=="true";if(!active&&x.Id.ToString()==User.FindFirstValue(ClaimTypes.NameIdentifier))throw new ArgumentException("Não desative sua própria conta.");x.Active=active;x.Stamp=Guid.NewGuid().ToString();break;}
 case "user":{
 var x=id==0?new PortalUser():await db.Users.FindAsync(id)??throw new ArgumentException("Usuário não encontrado.");var role=Choice(f,"Role","Admin","Professor","Aluno");var eid=N(f,"EntityId");if(role=="Aluno"&&!await db.Students.AnyAsync(x=>x.Id==eid&&x.Active)||role=="Professor"&&!await db.Teachers.AnyAsync(x=>x.Id==eid&&x.Active))throw new ArgumentException("Selecione um aluno ou professor ativo.");var email=Required(f,"Email",150).ToLowerInvariant();if(!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))throw new ArgumentException("E-mail inválido.");if(await db.Users.AnyAsync(x=>x.Email==email&&x.Id!=id))throw new ArgumentException("E-mail já utilizado.");if(role!="Admin"&&await db.Users.AnyAsync(x=>x.Role==role&&x.EntityId==eid&&x.Id!=id))throw new ArgumentException("Este cadastro já possui uma conta.");var active=F(f,"Active")=="true";if(x.Id.ToString()==User.FindFirstValue(ClaimTypes.NameIdentifier)&&(!active||role!="Admin"))throw new ArgumentException("Não desative nem rebaixe sua própria conta.");var password=F(f,"Password",256);if((id==0||password.Length>0)&&password.Length<10)throw new ArgumentException("Senha precisa ter pelo menos 10 caracteres.");x.Name=role=="Aluno"?(await db.Students.FindAsync(eid))!.Name:role=="Professor"?(await db.Teachers.FindAsync(eid))!.Name:FullName(f,"Name");x.Email=email;x.Role=role;x.EntityId=role=="Admin"?null:eid;x.Active=active;x.Stamp=Guid.NewGuid().ToString();if(password.Length>0)x.PasswordHash=new PasswordHasher<PortalUser>().HashPassword(x,password);if(id==0)db.Users.Add(x);break;}
 default: return BadRequest();
 }
 await db.SaveChangesAsync();await transaction.CommitAsync();TempData["Success"]=kind switch{"student"=>"Cadastro do aluno salvo.","teacher"=>"Cadastro do professor salvo.","subject"=>"Matéria atualizada.","activity"=>"Atividade salva para os alunos selecionados.","progress"=>"Progresso registrado.","lesson"=>"Agendamento salvo.","paid"=>"Pagamento registrado.",_=>"Alteração salva."};
 }catch(UnauthorizedAccessException){if(written!=null)System.IO.File.Delete(Path.Combine(paths.DataDirectory,"Arquivos",written));return Forbid();}
 catch(DbUpdateException){if(written!=null)System.IO.File.Delete(Path.Combine(paths.DataDirectory,"Arquivos",written));TempData["Error"]="Este registro já foi salvo ou foi alterado em outra janela. Atualize a página antes de tentar novamente.";}
 catch(ArgumentException e){if(written!=null)System.IO.File.Delete(Path.Combine(paths.DataDirectory,"Arquivos",written));TempData["Error"]=e.Message;}
 return RedirectToAction(nameof(Index),new{section});
 }
 [HttpGet("horarios")] public async Task<IActionResult> Slots(int studentId,int teacherId,int subjectId,DateTime date,int minutes=60,int lessonId=0){
 try{
 if(Role=="Aluno")studentId=Entity;if(Role=="Professor")teacherId=Entity;
 if(!new[]{30,60,90,120}.Contains(minutes))return BadRequest();
 await Compatible(studentId,subjectId,teacherId);
 var teacher=(await db.Teachers.FindAsync(teacherId))!;var day=(int)date.DayOfWeek;day=day==0?7:day;
 var slots=new List<string>();if(!PortalRules.Ids(teacher.Days).Contains(day))return Json(slots);
 if(lessonId>0){var lesson=await db.Lessons.FindAsync(lessonId);if(lesson==null||lesson.StudentId!=studentId||Role=="Professor"&&lesson.TeacherId!=Entity)return Forbid();}
 var from=date.Date;var until=from.AddDays(1);var lessons=await db.Lessons.Where(x=>x.Id!=lessonId&&x.Status!="Cancelada"&&(x.StudentId==studentId||x.TeacherId==teacherId)&&x.Start<until&&x.Start>=from.AddDays(-1)).ToListAsync();
 for(var start=from.AddHours(teacher.StartHour);start.AddMinutes(minutes)<=from.AddHours(teacher.EndHour);start=start.AddMinutes(30))if(start>DateTime.Now&&!lessons.Any(x=>x.Start<start.AddMinutes(minutes)&&x.Start.AddMinutes(x.Minutes)>start))slots.Add(start.ToString("yyyy-MM-ddTHH:mm"));
 return Json(slots);
 }catch(UnauthorizedAccessException){return Forbid();}catch(ArgumentException){return Json(Array.Empty<string>());}
 }
 async Task<(string key,string name)> Upload(IFormFile file){var ext=Path.GetExtension(file.FileName).ToLowerInvariant();if(!new[]{".pdf",".docx",".xlsx",".pptx",".txt",".png",".jpg",".jpeg"}.Contains(ext)||file.Length<1||file.Length>10*1024*1024)throw new ArgumentException("Arquivo permitido: PDF, Office, TXT, PNG ou JPG, até 10 MB.");var dir=Path.Combine(paths.DataDirectory,"Arquivos");Directory.CreateDirectory(dir);var key=Guid.NewGuid().ToString("N")+ext;await using var stream=System.IO.File.Create(Path.Combine(dir,key));await file.CopyToAsync(stream);return(key,Path.GetFileName(file.FileName));}
 [HttpGet("arquivo/{type}/{id:int}")] public async Task<IActionResult> Download(string type,int id){string key,name;int sid;int? tid;
 if(type=="material"){var x=await db.Materials.FindAsync(id);if(x==null)return NotFound();key=x.FileKey;name=x.FileName;sid=x.StudentId;tid=x.TeacherId;}
 else if(type=="enunciado"){var x=await db.Activities.FindAsync(id);if(x?.AssignmentKey==null)return NotFound();key=x.AssignmentKey;name=x.AssignmentName??"atividade";sid=x.StudentId;tid=x.TeacherId;}
 else if(type=="atividade"){var x=await db.Activities.FindAsync(id);if(x?.FileKey==null)return NotFound();key=x.FileKey;name=x.FileName??"entrega";sid=x.StudentId;tid=x.TeacherId;}else return NotFound();
 if(!await CanStudent(sid)||Role=="Professor"&&tid!=Entity&&tid!=null)return Forbid();var path=Path.Combine(paths.DataDirectory,"Arquivos",key);if(!System.IO.File.Exists(path))return NotFound();return PhysicalFile(path,"application/octet-stream",name);}
 [Authorize(Roles="Admin"),HttpGet("relatorio")] public async Task<IActionResult> Report(){var rows=await db.Students.OrderBy(x=>x.Name).ToListAsync();static string Cell(string s)=>"\""+((s.Length>0&&"=+-@".Contains(s[0]))?"'"+s:s).Replace("\"","\"\"")+"\"";var csv="Nome;Nível;Turno;Responsável;Situação\r\n"+string.Join("\r\n",rows.Select(x=>string.Join(";",new[]{x.Name,x.Level,x.Shift,x.Guardian,x.Status}.Select(Cell))));return File(System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csv)).ToArray(),"text/csv","alunos.csv");}
 [Authorize(Roles="Admin"),HttpPost("backup"),ValidateAntiForgeryToken] public IActionResult Backup(){var temp=Path.Combine(Path.GetTempPath(),"educamais-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(temp);try{foreach(var file in new[]{"sistema-pedagogico.db","educamais.db"}){var source=Path.Combine(paths.DataDirectory,file);if(!System.IO.File.Exists(source))continue;using var a=new SqliteConnection(new SqliteConnectionStringBuilder{DataSource=source}.ToString());using var b=new SqliteConnection(new SqliteConnectionStringBuilder{DataSource=Path.Combine(temp,file)}.ToString());a.Open();b.Open();a.BackupDatabase(b);}var dir=Path.Combine(paths.DataDirectory,"Arquivos");if(Directory.Exists(dir)){Directory.CreateDirectory(Path.Combine(temp,"Arquivos"));foreach(var p in Directory.GetFiles(dir))System.IO.File.Copy(p,Path.Combine(temp,"Arquivos",Path.GetFileName(p)));}var zip=temp+".zip";ZipFile.CreateFromDirectory(temp,zip);var bytes=System.IO.File.ReadAllBytes(zip);System.IO.File.Delete(zip);return File(bytes,"application/zip","EducaMais-backup-"+DateTime.Now.ToString("yyyyMMdd-HHmm")+".zip");}finally{Directory.Delete(temp,true);}}
}
