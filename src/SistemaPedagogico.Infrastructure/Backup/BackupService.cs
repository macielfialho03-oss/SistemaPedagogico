using Microsoft.Data.Sqlite;
using SistemaPedagogico.Infrastructure.Storage;

namespace SistemaPedagogico.Infrastructure.Backup;

public sealed class BackupService(StoragePaths paths)
{
    public IReadOnlyList<FileInfo> Listar() =>
        new DirectoryInfo(paths.BackupDirectory)
            .GetFiles("*.db")
            .OrderByDescending(x => x.CreationTime)
            .ToList();

    public string Criar(string motivo = "manual")
    {
        Directory.CreateDirectory(paths.BackupDirectory);
        var nome = $"SistemaPedagogico_{DateTime.Now:yyyy-MM-dd_HHmmss}_{Sanitizar(motivo)}.db";
        var destino = Path.Combine(paths.BackupDirectory, nome);
        using var origem = new SqliteConnection($"Data Source={paths.DatabasePath}");
        using var backup = new SqliteConnection($"Data Source={destino}");
        origem.Open();
        backup.Open();
        origem.BackupDatabase(backup);
        return destino;
    }

    public void Restaurar(string nomeArquivo)
    {
        var origemPath = Path.GetFullPath(Path.Combine(paths.BackupDirectory, nomeArquivo));
        var backupRoot = Path.GetFullPath(paths.BackupDirectory) + Path.DirectorySeparatorChar;
        if (
            !origemPath.StartsWith(backupRoot, StringComparison.OrdinalIgnoreCase)
            || !File.Exists(origemPath)
        )
            throw new InvalidOperationException("Backup inválido.");

        Criar("antes-restauracao");
        using var origem = new SqliteConnection($"Data Source={origemPath};Mode=ReadOnly");
        using var destino = new SqliteConnection($"Data Source={paths.DatabasePath}");
        origem.Open();
        destino.Open();
        origem.BackupDatabase(destino);
    }

    public bool VerificarIntegridade()
    {
        using var connection = new SqliteConnection(
            $"Data Source={paths.DatabasePath};Mode=ReadOnly"
        );
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA integrity_check;";
        return string.Equals(
            command.ExecuteScalar()?.ToString(),
            "ok",
            StringComparison.OrdinalIgnoreCase
        );
    }

    private static string Sanitizar(string valor) =>
        string.Concat(valor.ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-'))
            .Trim('-');
}
