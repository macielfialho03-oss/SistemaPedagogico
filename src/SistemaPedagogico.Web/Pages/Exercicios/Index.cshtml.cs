using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaPedagogico.Core.Entities;
using SistemaPedagogico.Infrastructure.Data;

namespace SistemaPedagogico.Web.Pages.Exercicios;

public class IndexModel(SistemaPedagogicoDbContext db) : PageModel
{
    public List<Exercicio> Itens { get; set; } = [];
    public SelectList Disciplinas { get; set; } = null!;

    [BindProperty]
    public Exercicio Item { get; set; } = new();

    public async Task OnGetAsync() => await Carregar();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await Carregar();
            return Page();
        }
        db.Exercicios.Add(Item);
        await db.SaveChangesAsync();
        TempData["Mensagem"] = "Exercício adicionado ao banco.";
        return RedirectToPage();
    }

    private async Task Carregar()
    {
        Itens = await db
            .Exercicios.AsNoTracking()
            .Include(x => x.Disciplina)
            .OrderBy(x => x.Disciplina.Nome)
            .ThenBy(x => x.Conteudo)
            .ToListAsync();
        Disciplinas = new SelectList(
            await db.Disciplinas.Where(x => x.Ativa).OrderBy(x => x.Nome).ToListAsync(),
            "Id",
            "Nome"
        );
    }
}
