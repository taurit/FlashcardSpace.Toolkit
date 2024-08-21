using Microsoft.EntityFrameworkCore;

namespace BookToAnki.NotePropertiesDatabase;

public class NoteContext : DbContext
{
    public DbSet<NotePropertiesEntity> Notes { get; set; }
    public string DbPath { get; private set; }

    public NoteContext(string dbPath, bool ensureCreated = false)
    {
        DbPath = dbPath;
        if (ensureCreated)
            Database.EnsureCreated(); //only use when I need it, e.g. unit tests. Huge performance hit.
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DbPath}");
        //optionsBuilder.LogTo(Console.WriteLine);
    }
}
