using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaPedagogico.Core.Entities;
using SistemaPedagogico.Infrastructure.Data;

namespace SistemaPedagogico.Web.Pages.Responsaveis;

public class IndexModel(SistemaPedagogicoDbContext db) : PageModel
{
    public List<Responsavel> Itens { get; set; } = [];
    public SelectList Alunos { get; set; } = null!;

    [BindProperty]
    public Responsavel Item { get; set; } = new();

    [BindProperty]
    public int? AlunoId { get; set; }

    [BindProperty]
    public string Parentesco { get; set; } = "Responsável";

    [BindProperty]
    public bool ContatoPrincipal { get; set; }

    public async Task OnGetAsync(int? alunoId)
    {
        AlunoId = alunoId;
        await Carregar();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await Carregar();
            return Page();
        }
        db.Responsaveis.Add(Item);
        if (AlunoId.HasValue)
            db.AlunosResponsaveis.Add(
                new AlunoResponsavel
                {
                    AlunoId = AlunoId.Value,
                    Responsavel = Item,
                    Parentesco = Parentesco,
                    ContatoPrincipal = ContatoPrincipal,
                }
            );
        await db.SaveChangesAsync();
        TempData["Mensagem"] = "Responsável cadastrado.";
        return RedirectToPage(new { alunoId = AlunoId });
    }

    private async Task Carregar()
    {
        Itens = await db
            .Responsaveis.AsNoTracking()
            .Include(x => x.Alunos)
                .ThenInclude(x => x.Aluno)
            .OrderBy(x => x.Nome)
            .ToListAsync();
        Alunos = new SelectList(
            await db.Alunos.Where(x => x.Ativo).OrderBy(x => x.Nome).ToListAsync(),
            "Id",
            "Nome",
            AlunoId
        );
    }
}
