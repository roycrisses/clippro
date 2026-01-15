using ClipboardPro.Models;
using Microsoft.EntityFrameworkCore;

namespace ClipboardPro.Data;

/// <summary>
/// Entity Framework Core database context for ClipboardPro
/// </summary>
public class AppDbContext : DbContext
{
    private readonly string _dbPath;

    public DbSet<Clipping> Clippings { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<ClippingTag> ClippingTags { get; set; } = null!;

    public AppDbContext(string dbPath)
    {
        _dbPath = dbPath;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={_dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Clipping configuration
        modelBuilder.Entity<Clipping>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.ContentHash).IsRequired();
            entity.HasIndex(e => e.ContentHash);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.SourceApp);
            
            entity.HasOne(e => e.Project)
                  .WithMany(p => p.Clippings)
                  .HasForeignKey(e => e.ProjectId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Project configuration
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Tag configuration
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // ClippingTag join table
        modelBuilder.Entity<ClippingTag>(entity =>
        {
            entity.HasKey(e => new { e.ClippingId, e.TagId });
            
            entity.HasOne(e => e.Clipping)
                  .WithMany(c => c.ClippingTags)
                  .HasForeignKey(e => e.ClippingId);
                  
            entity.HasOne(e => e.Tag)
                  .WithMany(t => t.ClippingTags)
                  .HasForeignKey(e => e.TagId);
        });
    }
}
