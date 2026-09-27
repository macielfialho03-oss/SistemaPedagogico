using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaPedagogico.Core.Entities;
using SistemaPedagogico.Infrastructure.Data;

namespace SistemaPedagogico.Web.Pages;

public class IndexModel(SistemaPedagogicoDbContext db) : PageModel
{
    public int AlunosAtivos { get; set; }
    public int DificuldadesAbertas { get; set; }
    public int ReforcosAtivos { get; set; }
    public int TarefasPendentes { get; set; }
    public List<EventoAgenda> ProximosEventos { get; set; } = [];
    public List<Dificuldade> DificuldadesRecentes { get; set; } = [];

    public async Task OnGetAsync()
    {
        AlunosAtivos = await db.Alunos.CountAsync(x => x.Ativo);
        DificuldadesAbertas = await db.Dificuldades.CountAsync(x =>
            x.Status != StatusDificuldade.Superada && x.Status != StatusDificuldade.Arquivada
        );
        ReforcosAtivos = await db.PlanosReforco.CountAsync(x =>
            x.Status == StatusPlanoReforco.Planejado || x.Status == StatusPlanoReforco.EmAndamento
        );
        TarefasPendentes = await db.TarefasEscolares.CountAsync(x =>
            x.Status == StatusTarefa.Pendente || x.Status == StatusTarefa.EmAndamento
        );
        ProximosEventos = await db
            .EventosAgenda.Include(x => x.Aluno)
            .Where(x => x.Inicio >= DateTime.Today && x.Status == StatusAgenda.Agendado)
            .OrderBy(x => x.Inicio)
            .Take(6)
            .ToListAsync();
        DificuldadesRecentes = await db
            .Dificuldades.Include(x => x.Aluno)
            .Include(x => x.Disciplina)
            .Where(x =>
                x.Status != StatusDificuldade.Superada && x.Status != StatusDificuldade.Arquivada
            )
            .OrderByDescending(x => x.IdentificadaEm)
            .Take(6)
            .ToListAsync();
    }
}
