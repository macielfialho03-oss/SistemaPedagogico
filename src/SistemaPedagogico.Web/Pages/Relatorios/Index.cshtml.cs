using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaPedagogico.Core.Entities;
using SistemaPedagogico.Infrastructure.Data;

namespace SistemaPedagogico.Web.Pages.Relatorios;

public class IndexModel(SistemaPedagogicoDbContext db) : PageModel
{
    public SelectList Alunos { get; set; } = null!;
    public Aluno? Aluno { get; set; }

    public async Task OnGetAsync(int? alunoId)
    {
        var alunos = await db.Alunos.OrderBy(x => x.Nome).ToListAsync();
        Alunos = new SelectList(alunos, "Id", "Nome", alunoId);
        if (!alunoId.HasValue)
            return;
        Aluno = await db
            .Alunos.AsNoTracking()
            .Include(x => x.Responsaveis)
                .ThenInclude(x => x.Responsavel)
            .Include(x => x.Notas)
                .ThenInclude(x => x.Disciplina)
            .Include(x => x.Acompanhamentos)
            .Include(x => x.Dificuldades)
                .ThenInclude(x => x.Disciplina)
            .Include(x => x.PlanosReforco)
                .ThenInclude(x => x.Sessoes)
            .Include(x => x.Observacoes)
            .FirstOrDefaultAsync(x => x.Id == alunoId);
    }
}
