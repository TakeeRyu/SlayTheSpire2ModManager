using Microsoft.EntityFrameworkCore;

namespace SlayTheSpire2ModManager.Infrastructure.Configuration;

public class AppConfigDbContext(DbContextOptions<AppConfigDbContext> options) : DbContext(options)
{
    public DbSet<AppConfigEntry> AppConfigEntries => Set<AppConfigEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppConfigEntry>(entity =>
        {
            entity.ToTable("AppConfig");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Key).HasMaxLength(128).IsRequired();
            entity.HasIndex(x => x.Key).IsUnique();
        });
    }
}
