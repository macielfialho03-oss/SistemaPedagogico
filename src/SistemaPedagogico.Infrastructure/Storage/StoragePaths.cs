namespace SistemaPedagogico.Infrastructure.Storage;

public sealed class StoragePaths
{
    public required string DataDirectory { get; init; }
    public string DatabasePath => Path.Combine(DataDirectory, "sistema-pedagogico.db");
    public string BackupDirectory => Path.Combine(DataDirectory, "Backups");

    public static StoragePaths Resolve(string? configuredPath, string contentRoot)
    {
        var path =
            string.IsNullOrWhiteSpace(configuredPath)
                ? Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SistemaPedagogico",
                    "Dados"
                )
            : Path.IsPathRooted(configuredPath) ? configuredPath
            : Path.GetFullPath(Path.Combine(contentRoot, configuredPath));

        Directory.CreateDirectory(path);
        Directory.CreateDirectory(Path.Combine(path, "Backups"));
        return new StoragePaths { DataDirectory = path };
    }
}
