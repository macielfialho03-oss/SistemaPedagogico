using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaPedagogico.Core.Entities;
using SistemaPedagogico.Infrastructure.Data;

namespace SistemaPedagogico.Web.Pages.Agenda;

public class IndexModel(SistemaPedagogicoDbContext db) : PageModel
{
    public List<EventoAgenda> Itens { get; set; } = [];
    public SelectList Alunos { get; set; } = null!;

    [BindProperty]
    public EventoAgenda Item { get; set; } = new();

    public async Task OnGetAsync() => await Carregar();

    public async Task<IActionResult> OnPostAsync()
    {
        if (Item.Termino.HasValue && Item.Termino < Item.Inicio)
            ModelState.AddModelError("Item.Termino", "O término deve ser posterior ao início.");
        if (!ModelState.IsValid)
        {
            await Carregar();
            return Page();
        }
        db.EventosAgenda.Add(Item);
        await db.SaveChangesAsync();
        TempData["Mensagem"] = "Compromisso agendado.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRealizarAsync(int id)
    {
        var x = await db.EventosAgenda.FindAsync(id);
        if (x != null)
        {
            x.Status = StatusAgenda.Realizado;
            await db.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    private async Task Carregar()
    {
        Itens = await db
            .EventosAgenda.AsNoTracking()
            .Include(x => x.Aluno)
            .OrderBy(x => x.Inicio < DateTime.Now)
            .ThenBy(x => x.Inicio)
            .ToListAsync();
        Alunos = new SelectList(
            await db.Alunos.Where(x => x.Ativo).OrderBy(x => x.Nome).ToListAsync(),
            "Id",
            "Nome"
        );
    }
}
