using Microsoft.EntityFrameworkCore;

namespace TechRadar.Core.Data;

// Our handle on the database. Each DbSet is one table.
public class RadarDbContext(DbContextOptions<RadarDbContext> options) : DbContext(options)
{
    public DbSet<NewsItemEntity> NewsItems => Set<NewsItemEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NewsItemEntity>(item =>
        {
            item.Property(i => i.Title).HasMaxLength(500);
            item.Property(i => i.Url).HasMaxLength(2000);
            item.Property(i => i.Source).HasMaxLength(100);
        });
    }
}
