using System.IO.Compression;
using System.Text;

namespace SistemaPedagogico.Web.Portal;

public static class AttachmentRules
{
    public const int MaxBytes = 4 * 1024 * 1024;
    public const string Accept = ".pdf,.docx,.xlsx,.pptx,.txt,.png,.jpg,.jpeg";
    public const string Help = "PDF, DOCX, XLSX, PPTX, TXT, JPG ou PNG. Até 4 MB por arquivo. Vídeos não são aceitos.";

    public static async Task<byte[]> ReadAsync(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!Accept.Split(',').Contains(ext) || file.Length < 1 || file.Length > MaxBytes)
            throw new ArgumentException(Help);
        await using var input = file.OpenReadStream();
        using var output = new MemoryStream();
        var buffer = new byte[81920];
        int read;
        while ((read = await input.ReadAsync(buffer)) > 0)
        {
            if (output.Length + read > MaxBytes) throw new ArgumentException(Help);
            output.Write(buffer, 0, read);
        }
        var bytes = output.ToArray();
        bool valid;
        try
        {
            valid = ext switch
            {
                ".pdf" => bytes.AsSpan().StartsWith("%PDF-"u8) && !Encoding.Latin1.GetString(bytes).Contains("/RichMedia", StringComparison.OrdinalIgnoreCase),
                ".png" => bytes.AsSpan().StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
                ".jpg" or ".jpeg" => bytes.AsSpan().StartsWith(new byte[] { 255, 216, 255 }),
                ".txt" => new UTF8Encoding(false, true).GetString(bytes).All(c => !char.IsControl(c) || c is '\r' or '\n' or '\t'),
                _ => ValidOffice(bytes, ext)
            };
        }
        catch (Exception e) when (e is InvalidDataException or DecoderFallbackException) { valid = false; }
        if (!valid) throw new ArgumentException("O conteúdo não corresponde ao formato permitido ou contém mídia não aceita. " + Help);
        return bytes;
    }

    private static bool ValidOffice(byte[] bytes, string ext)
    {
        using var zip = new ZipArchive(new MemoryStream(bytes));
        if (zip.Entries.Count > 2000 || zip.Entries.Sum(x => x.Length) > 50L * 1024 * 1024) return false;
        var main = ext switch { ".docx" => "word/document.xml", ".xlsx" => "xl/workbook.xml", ".pptx" => "ppt/presentation.xml", _ => "" };
        var types = zip.GetEntry("[Content_Types].xml");
        if (main == "" || zip.GetEntry(main) == null || types == null || types.Length > 1024 * 1024) return false;
        using var reader = new StreamReader(types.Open());
        var contentTypes = reader.ReadToEnd();
        if (contentTypes.Contains("video/", StringComparison.OrdinalIgnoreCase) || contentTypes.Contains("audio/", StringComparison.OrdinalIgnoreCase)) return false;
        // Reject embedded packages and non-image media rather than accepting renamed videos.
        return zip.Entries.All(x => !x.FullName.Contains("/embeddings/", StringComparison.OrdinalIgnoreCase)
            && (!x.FullName.Contains("/media/", StringComparison.OrdinalIgnoreCase)
                || new[] { ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".svg", ".emf", ".wmf", ".tif", ".tiff" }.Contains(Path.GetExtension(x.FullName).ToLowerInvariant())));
    }
}
