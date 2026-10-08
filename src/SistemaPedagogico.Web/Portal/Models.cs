using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
namespace SistemaPedagogico.Web.Portal;

public class Student {
 public string Status {get;set;}="Ativo";
 public string ServiceType {get;set;}="Reforço escolar"; public string RegistrationKey {get;set;}="";
 public int Id {get;set;} [Required,MaxLength(120)] public string Name {get;set;}="";
 public DateTime? BirthDate {get;set;} public string Email {get;set;}=""; public string Phone {get;set;}="";
 public string Level {get;set;}="Ensino Fundamental II"; public string Shift {get;set;}="Tarde";
 public string Guardian {get;set;}=""; public string GuardianPhone {get;set;}="";
 public string SubjectIds {get;set;}=""; public bool Active {get;set;}=true; public int? LegacyId {get;set;}
}
public class Teacher {
 public int Id {get;set;} public string Name {get;set;}=""; public string Email {get;set;}=""; public string Phone {get;set;}="";
 public string SubjectIds {get;set;}=""; public string Days {get;set;}="1,2,3,4,5";
 public int StartHour {get;set;}=8; public int EndHour {get;set;}=18; public bool Active {get;set;}=true;
}
public class Subject { public int Id {get;set;} public string Name {get;set;}=""; public string Description {get;set;}=""; public bool Active {get;set;}=true; public int? LegacyId {get;set;} }
public class Enrollment { public int Id {get;set;} public int StudentId {get;set;} public int TeacherId {get;set;} }
public class PortalUser { public int Id {get;set;} public string Name {get;set;}=""; public string Email {get;set;}=""; public string PasswordHash {get;set;}=""; public string Role {get;set;}="Aluno"; public int? EntityId {get;set;} public bool Active {get;set;}=true; public string Stamp {get;set;}=Guid.NewGuid().ToString(); }
public class Lesson { public int Id {get;set;} public int StudentId {get;set;} public int TeacherId {get;set;} public int SubjectId {get;set;} public DateTime Start {get;set;} public int Minutes {get;set;}=60; public string Mode {get;set;}="Presencial"; public string Status {get;set;}="Agendada"; public string Attendance {get;set;}="Não registrada"; public string Notes {get;set;}=""; public string OnlineUrl {get;set;}=""; }
public class Grade { public int Id {get;set;} public int StudentId {get;set;} public int? TeacherId {get;set;} public int SubjectId {get;set;} public string Title {get;set;}=""; public double Value {get;set;} public DateTime Date {get;set;}=DateTime.Today; public string Comment {get;set;}=""; public string Source {get;set;}="Avaliação da professora"; }
public class Activity { public int Id {get;set;} public int StudentId {get;set;} public int? TeacherId {get;set;} public int? SubjectId {get;set;} public string Title {get;set;}=""; public string Description {get;set;}=""; public DateTime Due {get;set;}=DateTime.Today; public string Status {get;set;}="Pendente"; public string Answer {get;set;}=""; public string? FileKey {get;set;} public string? FileName {get;set;} public DateTime? Submitted {get;set;} public double? Score {get;set;} public string Feedback {get;set;}=""; public string Origin {get;set;}="Proposta da professora"; public string? AssignmentKey {get;set;} public string? AssignmentName {get;set;} }
public class Material { public int Id {get;set;} public int StudentId {get;set;} public int TeacherId {get;set;} public int SubjectId {get;set;} public string Title {get;set;}=""; public string FileKey {get;set;}=""; public string FileName {get;set;}=""; public DateTime Created {get;set;}=DateTime.Now; }
public class Payment { public int Id {get;set;} public int StudentId {get;set;} public decimal Amount {get;set;} public DateTime Due {get;set;}=DateTime.Today; public DateTime? Paid {get;set;} public string Method {get;set;}="PIX"; public string Notes {get;set;}=""; }
public class Progress { public int Id {get;set;} public int StudentId {get;set;} public int TeacherId {get;set;} public string Text {get;set;}=""; public string ServiceType {get;set;}="Reforço escolar"; public int? SubjectId {get;set;} public string Difficulty {get;set;}=""; public string Goal {get;set;}=""; public string Worked {get;set;}=""; public string Evolution {get;set;}=""; public string NextSteps {get;set;}=""; public string Tasks {get;set;}=""; public string Autonomy {get;set;}=""; public DateTime Date {get;set;}=DateTime.Today; }
public class PortalSetting { public int Id {get;set;} public string Key {get;set;}=""; public string Value {get;set;}=""; }
public class EducaDb(DbContextOptions<EducaDb> options):DbContext(options) {
 public DbSet<Student> Students=>Set<Student>(); public DbSet<Teacher> Teachers=>Set<Teacher>(); public DbSet<Subject> Subjects=>Set<Subject>(); public DbSet<Enrollment> Enrollments=>Set<Enrollment>(); public DbSet<PortalUser> Users=>Set<PortalUser>(); public DbSet<Lesson> Lessons=>Set<Lesson>(); public DbSet<Grade> Grades=>Set<Grade>(); public DbSet<Activity> Activities=>Set<Activity>(); public DbSet<Material> Materials=>Set<Material>(); public DbSet<Payment> Payments=>Set<Payment>(); public DbSet<Progress> Progress=>Set<Progress>(); public DbSet<PortalSetting> Settings=>Set<PortalSetting>();
 protected override void OnModelCreating(ModelBuilder m) { m.Entity<Payment>().Property(x=>x.Amount).HasConversion(v=>decimal.ToInt64(v*100),v=>v/100m); m.Entity<PortalUser>().HasIndex(x=>x.Email).IsUnique(); m.Entity<Enrollment>().HasIndex(x=>new{x.StudentId,x.TeacherId}).IsUnique(); m.Entity<Lesson>().HasIndex(x=>x.Start); m.Entity<PortalSetting>().HasIndex(x=>x.Key).IsUnique(); }
}
public class PortalVm {
 public string Section {get;set;}="painel"; public string Role {get;set;}=""; public int Entity {get;set;} public string Query {get;set;}="";
 public List<Student> Students {get;set;}=[]; public List<Teacher> Teachers {get;set;}=[]; public List<Subject> Subjects {get;set;}=[]; public List<Lesson> Lessons {get;set;}=[]; public List<Grade> Grades {get;set;}=[]; public List<Activity> Activities {get;set;}=[]; public List<Material> Materials {get;set;}=[]; public List<Payment> Payments {get;set;}=[]; public List<Progress> Progress {get;set;}=[]; public List<PortalUser> Users {get;set;}=[]; public List<Enrollment> Enrollments {get;set;}=[];
 public string StudentName(int id)=>Students.FirstOrDefault(x=>x.Id==id)?.Name??"Aluno arquivado";
 public string TeacherName(int? id)=>Teachers.FirstOrDefault(x=>x.Id==id)?.Name??"Importado";
 public string SubjectName(int? id)=>Subjects.FirstOrDefault(x=>x.Id==id)?.Name??"Geral";
 public string SubjectNames(string ids)=>string.Join(", ",Subjects.Where(x=>PortalRules.Ids(ids).Contains(x.Id)).Select(x=>x.Name));
 public static string Weekdays(string days)=>string.Join(", ",PortalRules.Ids(days).Select(x=>new[]{"","Segunda","Terça","Quarta","Quinta","Sexta","Sábado","Domingo"}[x<=7?x:0]));
 public static string Initials(string name)=>string.Concat(name.Split(' ',StringSplitOptions.RemoveEmptyEntries).Take(2).Select(x=>x[0])).ToUpperInvariant();
 public static string PaymentStatus(Payment p)=>p.Paid!=null?"Pago":p.Due.Date<DateTime.Today?"Atrasado":"Pendente";
}
public static class PortalRules { public static int[] Ids(string? s)=>(s??"").Split(',',StringSplitOptions.RemoveEmptyEntries).Select(x=>int.TryParse(x,out var n)?n:0).Where(x=>x>0).Distinct().ToArray(); }
