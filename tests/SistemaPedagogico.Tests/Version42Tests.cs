using System.IO.Compression;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SistemaPedagogico.Web.Portal;

namespace SistemaPedagogico.Tests;

public class Version42Tests
{
    [Theory]
    [InlineData("video.mp4", "video")]
    [InlineData("video.pdf", "\0\0\0\u0018ftypmp42")]
    [InlineData("video.txt", "\0\0\0\u0018ftypmp42")]
    [InlineData("video.png", "not an image")]
    [InlineData("video.docx", "not an office document")]
    public async Task Rejects_video_or_renamed_binary(string name, string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        using var stream = new MemoryStream(bytes);
        await Assert.ThrowsAsync<ArgumentException>(() => AttachmentRules.ReadAsync(new FormFile(stream, 0, bytes.Length, "attachment", name)));
    }

    [Fact]
    public async Task Allows_five_megabytes_but_rejects_larger_files()
    {
        var bytes = Enumerable.Repeat((byte)'a', AttachmentRules.MaxBytes + 1).ToArray();
        using var stream = new MemoryStream(bytes);
        await Assert.ThrowsAsync<ArgumentException>(() => AttachmentRules.ReadAsync(new FormFile(stream, 0, bytes.Length, "attachment", "texto.txt")));
        using var valid = new MemoryStream(bytes[..AttachmentRules.MaxBytes]);
        Assert.Equal(AttachmentRules.MaxBytes, (await AttachmentRules.ReadAsync(new FormFile(valid, 0, valid.Length, "attachment", "texto.txt"))).Length);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Office_document_accepts_text_but_rejects_embedded_video(bool video)
    {
        using var data = new MemoryStream();
        using (var zip = new ZipArchive(data, ZipArchiveMode.Create, true))
        {
            using (var writer = new StreamWriter(zip.CreateEntry("[Content_Types].xml").Open())) writer.Write("<Types />");
            using (var writer = new StreamWriter(zip.CreateEntry("word/document.xml").Open())) writer.Write("<document />");
            if (video) { using var writer = new StreamWriter(zip.CreateEntry("word/media/video.mp4").Open()); writer.Write("video"); }
        }
        data.Position = 0;
        var file = new FormFile(data, 0, data.Length, "attachment", "atividade.docx");
        if (video) await Assert.ThrowsAsync<ArgumentException>(() => AttachmentRules.ReadAsync(file));
        else Assert.NotEmpty(await AttachmentRules.ReadAsync(file));
    }

    [Fact]
    public async Task Adoption_is_atomic_idempotent_and_preserves_students_and_history()
    {
        using var connection = new SqliteConnection("Data Source=:memory:"); connection.Open();
        using var db = new EducaDb(new DbContextOptionsBuilder<EducaDb>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var owner = new PortalUser { Name = "Maria Silva", Email = "teacher@example.test", Role = "Professor", EntityId = 7 };
        var oldAdmin = new PortalUser { Name = "Admin Antigo", Email = "admin@example.test", Role = "Admin" };
        db.Users.AddRange(owner, oldAdmin);
        db.Teachers.Add(new Teacher { Id = 7, Days = "2,4", StartHour = 13, EndHour = 18 });
        db.Students.Add(new Student { Id = 5, Name = "Pedro Silva" });
        db.Grades.Add(new Grade { StudentId = 5, TeacherId = 20, Value = 8 });
        await db.SaveChangesAsync();
        Assert.True(await SingleTeacher.AdoptAsync(db, owner));
        Assert.True(await SingleTeacher.AdoptAsync(db, owner));
        Assert.False(await SingleTeacher.AdoptAsync(db, oldAdmin));
        Assert.Single(await db.Users.Where(x => x.Active).ToListAsync());
        Assert.Equal("Admin", owner.Role); Assert.Equal(7, owner.EntityId);
        Assert.Equal("2,4", (await db.Teachers.FindAsync(7))!.Days);
        Assert.Single(await db.Enrollments.ToListAsync());
        Assert.Equal(20, (await db.Grades.SingleAsync()).TeacherId);
        Assert.Single(await db.Students.ToListAsync());
    }
}
