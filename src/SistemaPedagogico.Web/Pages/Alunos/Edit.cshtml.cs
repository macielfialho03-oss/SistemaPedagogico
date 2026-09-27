using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaPedagogico.Core.Entities;
using SistemaPedagogico.Infrastructure.Data;

namespace SistemaPedagogico.Web.Pages.Alunos;

public class EditModel(SistemaPedagogicoDbContext db) : PageModel
{
    [BindProperty]
    public Aluno Item { get; set; } = new();
    public bool Edicao => Item.Id > 0;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
            return Page();
        var item = await db.Alunos.FindAsync(id.Value);
        if (item is null)
            return NotFound();
        Item = item;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();
        if (Item.Id == 0)
            db.Alunos.Add(Item);
        else
            db.Alunos.Update(Item);
        await db.SaveChangesAsync();
        TempData["Mensagem"] = "Aluno salvo com sucesso.";
        return RedirectToPage("Details", new { id = Item.Id });
    }
}
