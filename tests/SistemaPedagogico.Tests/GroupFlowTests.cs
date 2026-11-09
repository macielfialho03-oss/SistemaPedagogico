using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaPedagogico.Web.Portal;
namespace SistemaPedagogico.Tests;

public partial class PortalTests
{
    Microsoft.AspNetCore.Http.FormCollection GroupLesson(string ids="1,2", string id="0", DateTime? start=null, string repeat="1") => Form(("Id",id),("StudentIds",ids),("SubjectId","1"),("Start",(start??DateTime.Today.AddDays(3).AddHours(10)).ToString("yyyy-MM-ddTHH:mm")),("Minutes","60"),("Mode","Presencial"),("RepeatCount",repeat));
    Microsoft.AspNetCore.Http.FormCollection GroupActivity(string id="0", string title="Leitura em grupo") => Form(("Id",id),("StudentIds","1,2"),("SubjectId","1"),("Title",title),("Description","Leia e responda."),("Due",DateTime.Today.AddDays(7).ToString("yyyy-MM-dd")));

    [Fact] public async Task Aula_em_grupo_cria_participacoes_sem_conflitar_entre_si()
    {
        var c=Controller("Admin",0);await c.Save("lesson",GroupLesson("1,2,1"),null);
        Assert.Null(c.TempData["Error"]);var rows=await db.Lessons.ToListAsync();Assert.Equal(2,rows.Count);Assert.Single(rows.Select(x=>x.GroupKey).Distinct());Assert.NotEmpty(rows[0].GroupKey);
        Assert.Single(Assert.IsType<PortalVm>(Assert.IsType<ViewResult>(await Controller("Aluno",1).Index("aulas")).Model).Lessons);
    }
    [Theory][InlineData("1,999")][InlineData("")]
    public async Task Aula_em_grupo_invalida_nao_salva_nenhum_participante(string ids)
    {
        var c=Controller("Admin",0);await c.Save("lesson",GroupLesson(ids),null);Assert.NotNull(c.TempData["Error"]);db.ChangeTracker.Clear();Assert.Empty(await db.Lessons.ToListAsync());
    }
    [Fact] public async Task Materia_incompativel_em_um_aluno_recusa_grupo_inteiro()
    {
        (await db.Students.FindAsync(2))!.SubjectIds="";await db.SaveChangesAsync();var c=Controller("Admin",0);await c.Save("lesson",GroupLesson(),null);Assert.NotNull(c.TempData["Error"]);Assert.Empty(await db.Lessons.ToListAsync());
    }
    [Fact] public async Task Conflito_com_outra_aula_recusa_grupo_sem_parcial()
    {
        db.Lessons.Add(new Lesson{StudentId=1,Start=DateTime.Today.AddDays(3).AddHours(10),Minutes=60});await db.SaveChangesAsync();var c=Controller("Admin",0);await c.Save("lesson",GroupLesson(),null);Assert.Contains("conflito",c.TempData["Error"]?.ToString());db.ChangeTracker.Clear();Assert.Single(await db.Lessons.ToListAsync());
    }
    [Fact] public async Task Remarcar_grupo_atualiza_todos_e_preserva_cancelamento_individual()
    {
        await Controller("Admin",0).Save("lesson",GroupLesson(),null);var rows=await db.Lessons.OrderBy(x=>x.StudentId).ToListAsync();
        await Controller("Aluno",2).Save("lesson-status",Form(("Id",rows[1].Id.ToString()),("Status","Cancelada"),("CancellationReason","Compromisso familiar")),null);
        var start=DateTime.Today.AddDays(4).AddHours(11);var c=Controller("Admin",0);await c.Save("lesson",GroupLesson("1",rows[0].Id.ToString(),start),null);Assert.Null(c.TempData["Error"]);
        Assert.All(await db.Lessons.ToListAsync(),x=>Assert.Equal(start,x.Start));Assert.Equal("Cancelada",rows[1].Status);Assert.Equal("Agendada",rows[0].Status);
    }
    [Fact] public async Task Remarcacao_conflitante_preserva_todos_os_horarios()
    {
        await Controller("Admin",0).Save("lesson",GroupLesson(),null);var rows=await db.Lessons.ToListAsync();var start=DateTime.Today.AddDays(4).AddHours(11);db.Lessons.Add(new Lesson{StudentId=1,Start=start,Minutes=60});await db.SaveChangesAsync();
        var c=Controller("Admin",0);await c.Save("lesson",GroupLesson("1,2",rows[0].Id.ToString(),start),null);Assert.NotNull(c.TempData["Error"]);db.ChangeTracker.Clear();Assert.All(await db.Lessons.Where(x=>x.GroupKey==rows[0].GroupKey).ToListAsync(),x=>Assert.NotEqual(start,x.Start));
    }
    [Fact] public async Task Presenca_e_cancelamento_sao_individuais()
    {
        await Controller("Admin",0).Save("lesson",GroupLesson(),null);var rows=await db.Lessons.OrderBy(x=>x.StudentId).ToListAsync();Assert.IsType<ForbidResult>(await Controller("Aluno",1).Save("lesson-group-cancel",Form(("Id",rows[0].Id.ToString())),null));
        foreach(var x in rows)x.Start=DateTime.Today.AddDays(-1);await db.SaveChangesAsync();
        await Controller("Admin",0).Save("lesson-status",Form(("Id",rows[0].Id.ToString()),("Status","Concluída"),("Attendance","Presente")),null);
        Assert.Equal("Agendada",rows[1].Status);await Controller("Admin",0).Save("lesson-status",Form(("Id",rows[1].Id.ToString()),("Status","Concluída"),("Attendance","Falta")),null);
        Assert.Equal("Presente",rows[0].Attendance);Assert.Equal("Falta",rows[1].Attendance);
    }
    [Fact] public async Task Cancelar_grupo_cancela_apenas_participacoes_agendadas()
    {
        await Controller("Admin",0).Save("lesson",GroupLesson(),null);var rows=await db.Lessons.ToListAsync();await Controller("Admin",0).Save("lesson-group-cancel",Form(("Id",rows[0].Id.ToString()),("CancellationReason","Feriado municipal")),null);Assert.All(rows,x=>Assert.Equal("Cancelada",x.Status));Assert.All(rows,x=>Assert.Equal("Feriado municipal",x.CancellationReason));
    }
    [Fact] public async Task Agendamento_automatico_cria_aulas_semanais_para_todo_o_grupo()
    {
        var start=DateTime.Today.AddDays(3).AddHours(10);var c=Controller("Admin",0);await c.Save("lesson",GroupLesson(start:start,repeat:"3"),null);Assert.Null(c.TempData["Error"]);
        var rows=await db.Lessons.OrderBy(x=>x.Start).ThenBy(x=>x.StudentId).ToListAsync();Assert.Equal(6,rows.Count);Assert.Equal(3,rows.Select(x=>x.GroupKey).Distinct().Count());Assert.Equal(new[]{start,start.AddDays(7),start.AddDays(14)},rows.Select(x=>x.Start).Distinct().ToArray());
    }
    [Fact] public async Task Conflito_em_repeticao_futura_impede_toda_a_serie()
    {
        var start=DateTime.Today.AddDays(3).AddHours(10);db.Lessons.Add(new Lesson{StudentId=1,Start=start.AddDays(7),Minutes=60});await db.SaveChangesAsync();var c=Controller("Admin",0);await c.Save("lesson",GroupLesson(start:start,repeat:"3"),null);Assert.Contains("conflito",c.TempData["Error"]?.ToString());db.ChangeTracker.Clear();Assert.Single(await db.Lessons.ToListAsync());
    }
    [Fact] public async Task Cancelamento_exige_justificativa_e_preserva_a_aula_sem_ela()
    {
        await Controller("Admin",0).Save("lesson",GroupLesson(),null);var row=await db.Lessons.FirstAsync();var c=Controller("Admin",0);await c.Save("lesson-group-cancel",Form(("Id",row.Id.ToString())),null);Assert.NotNull(c.TempData["Error"]);db.ChangeTracker.Clear();Assert.All(await db.Lessons.ToListAsync(),x=>Assert.Equal("Agendada",x.Status));
    }
    [Fact] public async Task Atividade_em_grupo_tem_um_identificador_e_entregas_isoladas()
    {
        var c=Controller("Admin",0);await c.Save("activity",GroupActivity(),null);Assert.Null(c.TempData["Error"]);var rows=await db.Activities.OrderBy(x=>x.StudentId).ToListAsync();Assert.Equal(2,rows.Count);Assert.Single(rows.Select(x=>x.GroupKey).Distinct());
        var staff=Assert.IsType<PortalVm>(Assert.IsType<ViewResult>(await c.Index("atividade",activityId:rows[0].Id)).Model);Assert.Equal(2,staff.Activities.Count);
        var pupil=Assert.IsType<PortalVm>(Assert.IsType<ViewResult>(await Controller("Aluno",1).Index("atividade",activityId:rows[0].Id)).Model);Assert.Single(pupil.Activities);
        Assert.IsType<ForbidResult>(await Controller("Aluno",1).Index("atividade",activityId:rows[1].Id));
        await Controller("Aluno",1).Save("submit",Form(("Id",rows[0].Id.ToString()),("Answer","Resposta só minha")),null);
        await c.Save("review",Form(("Id",rows[0].Id.ToString()),("Score","8,5"),("Feedback","Boa leitura")),null);
        Assert.Equal("Corrigida",rows[0].Status);Assert.Equal(8.5,rows[0].Score);Assert.Equal("Pendente",rows[1].Status);Assert.Equal("",rows[1].Answer);Assert.Null(rows[1].Score);
    }
    [Fact] public async Task Publicacoes_iguais_feitas_separadamente_nao_se_misturam()
    {
        await Controller("Admin",0).Save("activity",GroupActivity(),null);await Controller("Admin",0).Save("activity",GroupActivity(),null);Assert.Equal(2,(await db.Activities.ToListAsync()).GroupBy(x=>x.GroupKey).Count());
    }
    [Fact] public async Task Edicao_de_enunciado_vale_para_todos_e_bloqueia_apos_entrega()
    {
        var c=Controller("Admin",0);await c.Save("activity",GroupActivity(),null);var rows=await db.Activities.OrderBy(x=>x.StudentId).ToListAsync();await c.Save("activity",GroupActivity(rows[0].Id.ToString(),"Novo título"),null);Assert.All(rows,x=>Assert.Equal("Novo título",x.Title));
        await Controller("Aluno",2).Save("submit",Form(("Id",rows[1].Id.ToString()),("Answer","Entrega")),null);
        await c.Save("activity",GroupActivity(rows[0].Id.ToString(),"Não pode mudar"),null);Assert.NotNull(c.TempData["Error"]);db.ChangeTracker.Clear();Assert.All(await db.Activities.ToListAsync(),x=>Assert.Equal("Novo título",x.Title));
    }
    [Fact] public async Task Atualizacao_agrupa_atividades_antigas_sem_perder_entregas()
    {
        db.Activities.AddRange(new Activity{Id=10,StudentId=1,Title="Mesmo texto",Status="Corrigida",Answer="A",Score=9,Feedback="Ótimo"},new Activity{Id=11,StudentId=2,Title="Mesmo texto"},new Activity{Id=12,StudentId=1,Title="Mesmo texto"});db.Lessons.Add(new Lesson{Id=5,StudentId=1});await db.SaveChangesAsync();
        await GroupUpgrade.ApplyAsync(db);await GroupUpgrade.ApplyAsync(db);var rows=await db.Activities.OrderBy(x=>x.Id).ToListAsync();Assert.Equal(rows[0].GroupKey,rows[1].GroupKey);Assert.NotEqual(rows[0].GroupKey,rows[2].GroupKey);Assert.Equal("A",rows[0].Answer);Assert.Equal(9,rows[0].Score);Assert.Equal("Ótimo",rows[0].Feedback);Assert.Equal(3,rows.Count);Assert.NotEmpty((await db.Lessons.SingleAsync()).GroupKey);
    }
}
