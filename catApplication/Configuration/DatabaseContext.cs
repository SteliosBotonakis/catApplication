using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace catApplication.Configuration;

public class DatabaseContext : DbContext
{
    public DbSet<CatEntity> Cats { get; set; }
    public DbSet<TagEntity> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CatEntity>()
            .HasMany(c => c.Tags)
            .WithMany(c => c.Cats)
            .UsingEntity(j => j.ToTable("CatTags"));
    }

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
}

public class CatEntity
{
    public int Id { get; set; }
    public string CatId { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Image { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;

    public List<TagEntity> Tags { get; set; } = new(); 
}


public class TagEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;

    public List<CatEntity> Cats { get; set; } = new();
}

