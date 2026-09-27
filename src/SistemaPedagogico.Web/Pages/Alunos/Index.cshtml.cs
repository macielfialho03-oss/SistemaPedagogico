using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SistemaPedagogico.Core.Entities;
using SistemaPedagogico.Infrastructure.Data;

namespace SistemaPedagogico.Web.Pages.Alunos;

public class IndexModel(SistemaPedagogicoDbContext db) : PageModel
{
    public List<Aluno> Itens { get; set; } = [];

    public async Task OnGetAsync(string? busca)
    {
        var query = db.Alunos.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(x => x.Nome.Contains(busca) || (x.Escola ?? "").Contains(busca));
        Itens = await query.OrderByDescending(x => x.Ativo).ThenBy(x => x.Nome).ToListAsync();
    }
}
