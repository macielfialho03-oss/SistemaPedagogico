using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaPedagogico.Core.Entities;
using SistemaPedagogico.Infrastructure.Data;

namespace SistemaPedagogico.Web.Pages.Observacoes;

public class IndexModel(SistemaPedagogicoDbContext db) : PageModel
{
    public List<Observacao> Itens { get; set; } = [];
    public SelectList Alunos { get; set; } = null!;

    [BindProperty]
    public Observacao Item { get; set; } = new();

    public async Task OnGetAsync(int? alunoId)
    {
        Item.AlunoId = alunoId ?? 0;
        await Carregar(alunoId);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await Carregar(Item.AlunoId);
            return Page();
        }
        db.Observacoes.Add(Item);
        await db.SaveChangesAsync();
        TempData["Mensagem"] = "Observação registrada.";
        return RedirectToPage(new { alunoId = Item.AlunoId });
    }

    private async Task Carregar(int? alunoId)
    {
        var q = db.Observacoes.AsNoTracking().Include(x => x.Aluno).AsQueryable();
        if (alunoId.HasValue)
            q = q.Where(x => x.AlunoId == alunoId);
        Itens = await q.OrderByDescending(x => x.Destaque)
            .ThenByDescending(x => x.Data)
            .ToListAsync();
        Alunos = new SelectList(
            await db.Alunos.Where(x => x.Ativo).OrderBy(x => x.Nome).ToListAsync(),
            "Id",
            "Nome",
            Item.AlunoId
        );
    }
}
