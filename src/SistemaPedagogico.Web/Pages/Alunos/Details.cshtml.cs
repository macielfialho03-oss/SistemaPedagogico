using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaPedagogico.Core.Entities;
using SistemaPedagogico.Infrastructure.Data;

namespace SistemaPedagogico.Web.Pages.Alunos;

public class DetailsModel(SistemaPedagogicoDbContext db) : PageModel
{
    public Aluno Item { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var item = await db
            .Alunos.AsNoTracking()
            .Include(x => x.Responsaveis)
                .ThenInclude(x => x.Responsavel)
            .Include(x => x.Notas)
                .ThenInclude(x => x.Disciplina)
            .Include(x => x.Dificuldades)
                .ThenInclude(x => x.Disciplina)
            .Include(x => x.PlanosReforco)
                .ThenInclude(x => x.Disciplina)
            .Include(x => x.Acompanhamentos)
            .Include(x => x.Observacoes)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (item is null)
            return NotFound();
        Item = item;
        return Page();
    }
}
