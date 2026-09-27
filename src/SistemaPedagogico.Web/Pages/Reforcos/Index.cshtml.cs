using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaPedagogico.Core.Entities;
using SistemaPedagogico.Infrastructure.Data;

namespace SistemaPedagogico.Web.Pages.Reforcos;

public class IndexModel(SistemaPedagogicoDbContext db) : PageModel
{
    public List<PlanoReforco> Itens { get; set; } = [];
    public SelectList Dificuldades { get; set; } = null!;

    [BindProperty]
    public PlanoReforco Item { get; set; } = new();

    public async Task OnGetAsync(int? dificuldadeId, int? alunoId)
    {
        Item.DificuldadeId = dificuldadeId ?? 0;
        await Carregar(alunoId);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var dificuldade = await db.Dificuldades.FindAsync(Item.DificuldadeId);
        if (dificuldade is null)
            ModelState.AddModelError("Item.DificuldadeId", "Selecione uma dificuldade válida.");
        if (!ModelState.IsValid)
        {
            await Carregar(null);
            return Page();
        }
        Item.AlunoId = dificuldade!.AlunoId;
        Item.DisciplinaId = dificuldade.DisciplinaId;
        db.PlanosReforco.Add(Item);
        dificuldade.Status = StatusDificuldade.EmReforco;
        await db.SaveChangesAsync();
        TempData["Mensagem"] = "Plano de reforço criado.";
        return RedirectToPage();
    }

    private async Task Carregar(int? alunoId)
    {
        var q = db
            .PlanosReforco.AsNoTracking()
            .Include(x => x.Aluno)
            .Include(x => x.Disciplina)
            .Include(x => x.Dificuldade)
            .Include(x => x.Sessoes)
            .AsQueryable();
        if (alunoId.HasValue)
            q = q.Where(x => x.AlunoId == alunoId);
        Itens = await q.OrderBy(x => x.Status == StatusPlanoReforco.Concluido)
            .ThenByDescending(x => x.DataInicio)
            .ToListAsync();
        var difs = await db
            .Dificuldades.Include(x => x.Aluno)
            .Include(x => x.Disciplina)
            .Where(x =>
                x.Status != StatusDificuldade.Superada && x.Status != StatusDificuldade.Arquivada
            )
            .OrderBy(x => x.Aluno.Nome)
            .ToListAsync();
        Dificuldades = new SelectList(
            difs.Select(x => new
            {
                x.Id,
                Nome = $"{x.Aluno.Nome} · {x.Disciplina.Nome} · {x.Conteudo}",
            }),
            "Id",
            "Nome",
            Item.DificuldadeId
        );
    }
}
