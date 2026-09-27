using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaPedagogico.Core.Entities;
using SistemaPedagogico.Infrastructure.Data;

namespace SistemaPedagogico.Web.Pages.Notas;

public class IndexModel(SistemaPedagogicoDbContext db) : PageModel
{
    public List<Nota> Itens { get; set; } = [];
    public SelectList Alunos { get; set; } = null!;
    public SelectList Disciplinas { get; set; } = null!;

    [BindProperty]
    public Nota Item { get; set; } = new();

    public async Task OnGetAsync(int? alunoId)
    {
        Item.AlunoId = alunoId ?? 0;
        await Carregar(alunoId);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Item.Valor > Item.ValorMaximo)
            ModelState.AddModelError("Item.Valor", "A nota não pode superar o valor máximo.");
        if (!ModelState.IsValid)
        {
            await Carregar(Item.AlunoId);
            return Page();
        }
        db.Notas.Add(Item);
        await db.SaveChangesAsync();
        TempData["Mensagem"] = "Nota registrada.";
        return RedirectToPage(new { alunoId = Item.AlunoId });
    }

    private async Task Carregar(int? alunoId)
    {
        var q = db
            .Notas.AsNoTracking()
            .Include(x => x.Aluno)
            .Include(x => x.Disciplina)
            .AsQueryable();
        if (alunoId.HasValue)
            q = q.Where(x => x.AlunoId == alunoId);
        Itens = await q.OrderByDescending(x => x.Data).ToListAsync();
        Alunos = new SelectList(
            await db.Alunos.Where(x => x.Ativo).OrderBy(x => x.Nome).ToListAsync(),
            "Id",
            "Nome",
            alunoId
        );
        Disciplinas = new SelectList(
            await db.Disciplinas.Where(x => x.Ativa).OrderBy(x => x.Nome).ToListAsync(),
            "Id",
            "Nome"
        );
    }
}
