using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaPedagogico.Core.Entities;
using SistemaPedagogico.Infrastructure.Data;

namespace SistemaPedagogico.Web.Pages.Disciplinas;

public class IndexModel(SistemaPedagogicoDbContext db) : PageModel
{
    public List<Disciplina> Itens { get; set; } = [];

    [BindProperty]
    public Disciplina Item { get; set; } = new();

    public async Task OnGetAsync() =>
        Itens = await db.Disciplinas.AsNoTracking().OrderBy(x => x.Nome).ToListAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }
        db.Disciplinas.Add(Item);
        await db.SaveChangesAsync();
        TempData["Mensagem"] = "Disciplina cadastrada.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAlternarAsync(int id)
    {
        var item = await db.Disciplinas.FindAsync(id);
        if (item is not null)
        {
            item.Ativa = !item.Ativa;
            await db.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}
