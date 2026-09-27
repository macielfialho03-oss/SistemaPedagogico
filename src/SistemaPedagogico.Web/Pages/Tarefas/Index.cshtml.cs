using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaPedagogico.Core.Entities;
using SistemaPedagogico.Infrastructure.Data;

namespace SistemaPedagogico.Web.Pages.Tarefas;

public class IndexModel(SistemaPedagogicoDbContext db) : PageModel
{
    public List<TarefaEscolar> Itens { get; set; } = [];
    public SelectList Alunos { get; set; } = null!;
    public SelectList Disciplinas { get; set; } = null!;

    [BindProperty]
    public TarefaEscolar Item { get; set; } = new();

    public async Task OnGetAsync(int? alunoId)
    {
        Item.AlunoId = alunoId ?? 0;
        await Carregar();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await Carregar();
            return Page();
        }
        db.TarefasEscolares.Add(Item);
        await db.SaveChangesAsync();
        TempData["Mensagem"] = "Tarefa cadastrada.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostConcluirAsync(int id)
    {
        var x = await db.TarefasEscolares.FindAsync(id);
        if (x != null)
        {
            x.Status = StatusTarefa.Concluida;
            x.ConcluidaEm = DateTime.Now;
            await db.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    private async Task Carregar()
    {
        Itens = await db
            .TarefasEscolares.AsNoTracking()
            .Include(x => x.Aluno)
            .Include(x => x.Disciplina)
            .OrderBy(x => x.Status)
            .ThenBy(x => x.Prazo)
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
