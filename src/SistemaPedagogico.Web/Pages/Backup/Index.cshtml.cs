using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SistemaPedagogico.Infrastructure.Backup;
using SistemaPedagogico.Infrastructure.Storage;

namespace SistemaPedagogico.Web.Pages.Backup;

public class IndexModel(BackupService backup, StoragePaths paths) : PageModel
{
    public IReadOnlyList<FileInfo> Itens { get; set; } = [];
    public bool Integro { get; set; }
    public string Pasta => paths.BackupDirectory;

    public void OnGet()
    {
        Itens = backup.Listar();
        Integro = backup.VerificarIntegridade();
    }

    public IActionResult OnPostCriar()
    {
        var arquivo = backup.Criar();
        TempData["Mensagem"] = $"Backup criado: {Path.GetFileName(arquivo)}";
        return RedirectToPage();
    }

    public IActionResult OnPostRestaurar(string arquivo)
    {
        backup.Restaurar(arquivo);
        TempData["Mensagem"] =
            "Backup restaurado. Uma cópia de segurança do banco anterior foi criada.";
        return RedirectToPage();
    }
}
