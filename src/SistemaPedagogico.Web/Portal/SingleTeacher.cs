using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace SistemaPedagogico.Web.Portal;

public static class SingleTeacher
{
    public const string OwnerKey = "single-teacher-owner";
    public const string TeacherKey = "single-teacher-profile";

    // Called only after checking the password and explicit migration confirmation.
    public static async Task<bool> AdoptAsync(EducaDb db, PortalUser user)
    {
        if (!user.Active || user.Role is not ("Admin" or "Professor")) return false;
        var owner = await db.Settings.SingleOrDefaultAsync(x => x.Key == OwnerKey);
        if (owner != null) return owner.Value == user.Id.ToString();

        var connection = (SqliteConnection)db.Database.GetDbConnection();
        if (File.Exists(connection.DataSource))
        {
            var dir = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(connection.DataSource))!, "Backups");
            Directory.CreateDirectory(dir);
            await db.Database.OpenConnectionAsync();
            using var copy = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = Path.Combine(dir, "tutoring-antes-v4.2-" + Guid.NewGuid().ToString("N") + ".db") }.ToString());
            copy.Open(); connection.BackupDatabase(copy);
        }

        await using var tx = await db.Database.BeginTransactionAsync();
        // A unique settings key also protects against concurrent adoption.
        if (await db.Settings.AnyAsync(x => x.Key == OwnerKey)) return false;
        var teacher = user.Role == "Professor" && user.EntityId.HasValue
            ? await db.Teachers.FindAsync(user.EntityId.Value) : null;
        teacher ??= await db.Teachers.FirstOrDefaultAsync(x => x.Email == user.Email && x.Active);
        if (teacher == null) { teacher = new Teacher(); db.Teachers.Add(teacher); }
        teacher.Name = user.Name; teacher.Email = user.Email; teacher.Active = true;
        teacher.SubjectIds = string.Join(",", await db.Subjects.Where(x => x.Active).Select(x => x.Id).ToListAsync());
        await db.SaveChangesAsync();
        foreach (var other in await db.Users.Where(x => x.Id != user.Id && (x.Role == "Admin" || x.Role == "Professor")).ToListAsync())
        { other.Active = false; other.Stamp = Guid.NewGuid().ToString(); }
        user.Role = "Admin"; user.EntityId = teacher.Id; user.Stamp = Guid.NewGuid().ToString();
        var linked = await db.Enrollments.Where(x => x.TeacherId == teacher.Id).Select(x => x.StudentId).ToListAsync();
        foreach (var id in await db.Students.Where(x => !linked.Contains(x.Id)).Select(x => x.Id).ToListAsync())
            db.Enrollments.Add(new Enrollment { StudentId = id, TeacherId = teacher.Id });
        db.Settings.AddRange(new PortalSetting { Key = OwnerKey, Value = user.Id.ToString() }, new PortalSetting { Key = TeacherKey, Value = teacher.Id.ToString() });
        await db.SaveChangesAsync(); await tx.CommitAsync();
        return true;
    }

    public static async Task<int> ProfileIdAsync(EducaDb db)
    {
        var value = await db.Settings.Where(x => x.Key == TeacherKey).Select(x => x.Value).SingleOrDefaultAsync();
        return int.TryParse(value, out var id) ? id : throw new ArgumentException("Entre novamente com a conta da professora para concluir a atualização.");
    }
}
