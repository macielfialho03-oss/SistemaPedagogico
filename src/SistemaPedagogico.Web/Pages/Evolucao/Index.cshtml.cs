using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaPedagogico.Core.Entities;
using SistemaPedagogico.Infrastructure.Data;

namespace SistemaPedagogico.Web.Pages.Evolucao;

public class IndexModel(SistemaPedagogicoDbContext db) : PageModel
{
    public SelectList Alunos { get; set; } = null!;
    public Aluno? Aluno { get; set; }
    public List<Nota> Notas { get; set; } = [];
    public List<SessaoReforco> Sessoes { get; set; } = [];
    public int Superadas { get; set; }
    public decimal MediaNotas =>
        Notas.Count == 0 ? 0 : Math.Round(Notas.Average(x => x.Percentual), 1);
    public decimal MediaExercicios =>
        Sessoes.Count == 0 ? 0 : Math.Round(Sessoes.Average(x => x.Aproveitamento), 1);

    public async Task OnGetAsync(int? alunoId)
    {
        var alunos = await db.Alunos.Where(x => x.Ativo).OrderBy(x => x.Nome).ToListAsync();
        Alunos = new SelectList(alunos, "Id", "Nome", alunoId);
        if (!alunoId.HasValue)
            return;
        Aluno = await db.Alunos.FindAsync(alunoId);
        Notas = await db
            .Notas.AsNoTracking()
            .Include(x => x.Disciplina)
            .Where(x => x.AlunoId == alunoId)
            .OrderBy(x => x.Data)
            .ToListAsync();
        Sessoes = await db
            .SessoesReforco.AsNoTracking()
            .Include(x => x.PlanoReforco)
                .ThenInclude(x => x.Disciplina)
            .Include(x => x.Exercicios)
            .Where(x => x.AlunoId == alunoId)
            .OrderBy(x => x.DataHora)
            .ToListAsync();
        Superadas = await db.Dificuldades.CountAsync(x =>
            x.AlunoId == alunoId && x.Status == StatusDificuldade.Superada
        );
    }
}
