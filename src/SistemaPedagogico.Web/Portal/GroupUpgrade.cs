using Microsoft.EntityFrameworkCore;

namespace SistemaPedagogico.Web.Portal;

public static class GroupUpgrade
{
    public static async Task ApplyAsync(EducaDb db)
    {
        if (await db.Settings.AnyAsync(x => x.Key == "groups-v4.3")) return;
        await using var tx = await db.Database.BeginTransactionAsync();
        foreach (var lesson in await db.Lessons.Where(x => x.GroupKey == "").ToListAsync())
            lesson.GroupKey = Guid.NewGuid().ToString("N");

        // Old releases did not save a publication ID. Group identical assignments to
        // distinct students, preserving every response, grade, ID and attachment.
        // Repeated assignments to the SAME student remain separate publications.
        var old = await db.Activities.Where(x => x.GroupKey == "").OrderBy(x => x.Id).ToListAsync();
        foreach (var same in old.GroupBy(x => new { x.TeacherId, x.SubjectId, x.Title, x.Description, x.Due, x.Origin, x.AssignmentKey, x.AssignmentName }))
        {
            var batches = new List<(string key, HashSet<int> students)>();
            foreach (var row in same)
            {
                var index = batches.FindIndex(x => !x.students.Contains(row.StudentId));
                if (index < 0) { batches.Add((Guid.NewGuid().ToString("N"), new HashSet<int>())); index = batches.Count - 1; }
                row.GroupKey = batches[index].key;
                batches[index].students.Add(row.StudentId);
            }
        }
        db.Settings.Add(new PortalSetting { Key = "groups-v4.3", Value = DateTime.UtcNow.ToString("O") });
        await db.SaveChangesAsync();
        await db.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_Lessons_GroupKey ON Lessons(GroupKey)");
        await db.Database.ExecuteSqlRawAsync("CREATE INDEX IF NOT EXISTS IX_Activities_GroupKey ON Activities(GroupKey)");
        await tx.CommitAsync();
    }
}
