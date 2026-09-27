using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaPedagogico.Core.Entities;
using SistemaPedagogico.Infrastructure.Data;

namespace SistemaPedagogico.Web.Pages.Sessoes;

public class IndexModel(SistemaPedagogicoDbContext db) : PageModel
{
    public List<SessaoReforco> Itens { get; set; } = [];
    public SelectList Planos { get; set; } = null!;
    public SelectList Sessoes { get; set; } = null!;
    public SelectList Exercicios { get; set; } = null!;

    [BindProperty]
    public SessaoReforco Item { get; set; } = new();

    [BindProperty]
    public SessaoExercicio Resultado { get; set; } = new();

    public async Task OnGetAsync(int? planoId)
    {
        Item.PlanoReforcoId = planoId ?? 0;
        await Carregar();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var plano = await db.PlanosReforco.FindAsync(Item.PlanoReforcoId);
        if (plano is null)
            ModelState.AddModelError("Item.PlanoReforcoId", "Selecione um plano.");
        if (!ModelState.IsValid)
        {
            await Carregar();
            return Page();
        }
        Item.AlunoId = plano!.AlunoId;
        db.SessoesReforco.Add(Item);
        if (plano.Status == StatusPlanoReforco.Planejado)
            plano.Status = StatusPlanoReforco.EmAndamento;
        await db.SaveChangesAsync();
        TempData["Mensagem"] = "Sessão registrada.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostResultadoAsync()
    {
        if (Resultado.Acertos + Resultado.Erros == 0)
            ModelState.AddModelError("Resultado.Acertos", "Informe pelo menos um acerto ou erro.");
        if (!ModelState.IsValid)
        {
            await Carregar();
            return Page();
        }
        db.SessoesExercicios.Add(Resultado);
        await db.SaveChangesAsync();
        TempData["Mensagem"] = "Resultado do exercício registrado.";
        return RedirectToPage();
    }

    private async Task Carregar()
    {
        Itens = await db
            .SessoesReforco.AsNoTracking()
            .Include(x => x.Aluno)
            .Include(x => x.PlanoReforco)
                .ThenInclude(x => x.Disciplina)
            .Include(x => x.Exercicios)
                .ThenInclude(x => x.Exercicio)
            .OrderByDescending(x => x.DataHora)
            .ToListAsync();
        var planos = await db
            .PlanosReforco.Include(x => x.Aluno)
            .Where(x =>
                x.Status != StatusPlanoReforco.Concluido && x.Status != StatusPlanoReforco.Suspenso
            )
            .ToListAsync();
        Planos = new SelectList(
            planos.Select(x => new { x.Id, Nome = $"{x.Aluno.Nome} · {x.Titulo}" }),
            "Id",
            "Nome",
            Item.PlanoReforcoId
        );
        Sessoes = new SelectList(
            Itens.Select(x => new
            {
                x.Id,
                Nome = $"{x.Aluno.Nome} · {x.DataHora:dd/MM/yyyy HH:mm}",
            }),
            "Id",
            "Nome"
        );
        var ex = await db.Exercicios.Include(x => x.Disciplina).Where(x => x.Ativo).ToListAsync();
        Exercicios = new SelectList(
            ex.Select(x => new { x.Id, Nome = $"{x.Disciplina.Nome} · {x.Conteudo}" }),
            "Id",
            "Nome"
        );
    }
}
