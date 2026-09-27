using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaPedagogico.Core.Entities;
using SistemaPedagogico.Infrastructure.Data;

namespace SistemaPedagogico.Web.Pages.Dificuldades;

public class IndexModel(SistemaPedagogicoDbContext db) : PageModel
{
    public List<Dificuldade> Itens { get; set; } = [];
    public SelectList Alunos { get; set; } = null!;
    public SelectList Disciplinas { get; set; } = null!;

    [BindProperty]
    public Dificuldade Item { get; set; } = new();

    public async Task OnGetAsync(int? alunoId, int? acompanhamentoId)
    {
        Item.AlunoId = alunoId ?? 0;
        Item.AcompanhamentoOrigemId = acompanhamentoId;
        await Carregar(alunoId);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await Carregar(Item.AlunoId);
            return Page();
        }
        db.Dificuldades.Add(Item);
        await db.SaveChangesAsync();
        TempData["Mensagem"] = "Dificuldade identificada.";
        return RedirectToPage(new { alunoId = Item.AlunoId });
    }

    public async Task<IActionResult> OnPostSuperarAsync(int id)
    {
        var x = await db.Dificuldades.FindAsync(id);
        if (x != null)
        {
            x.Status = StatusDificuldade.Superada;
            x.SuperadaEm = DateTime.Now;
            await db.SaveChangesAsync();
            TempData["Mensagem"] = "Dificuldade marcada como superada.";
        }
        return RedirectToPage();
    }

    private async Task Carregar(int? alunoId)
    {
        var q = db
            .Dificuldades.AsNoTracking()
            .Include(x => x.Aluno)
            .Include(x => x.Disciplina)
            .AsQueryable();
        if (alunoId.HasValue)
            q = q.Where(x => x.AlunoId == alunoId);
        Itens = await q.OrderBy(x => x.Status == StatusDificuldade.Superada)
            .ThenByDescending(x => x.IdentificadaEm)
            .ToListAsync();
        Alunos = new SelectList(
            await db.Alunos.Where(x => x.Ativo).OrderBy(x => x.Nome).ToListAsync(),
            "Id",
            "Nome",
            Item.AlunoId
        );
        Disciplinas = new SelectList(
            await db.Disciplinas.Where(x => x.Ativa).OrderBy(x => x.Nome).ToListAsync(),
            "Id",
            "Nome"
        );
    }
}
