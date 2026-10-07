using AlgoVis.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlgoVis.Data;

public sealed class AlgoVisDbContext : DbContext
{
    public AlgoVisDbContext(DbContextOptions<AlgoVisDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Project> Projects => Set<Project>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);

            e.Property(x => x.Email).IsRequired().HasMaxLength(200);
            e.HasIndex(x => x.Email).IsUnique();

            e.Property(x => x.Username).IsRequired().HasMaxLength(100);
            e.Property(x => x.PasswordHash).IsRequired().HasMaxLength(200);
            e.Property(x => x.Role).IsRequired().HasMaxLength(20).HasDefaultValue("student");
            e.Property(x => x.DefaultLanguage).IsRequired().HasMaxLength(40).HasDefaultValue("python");

            e.HasMany(x => x.RefreshTokens)
             .WithOne(x => x.User)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.Projects)
             .WithOne(x => x.User)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<RefreshToken>(e =>
        {
            e.ToTable("refresh_tokens");
            e.HasKey(x => x.Id);

            e.Property(x => x.Token).IsRequired().HasMaxLength(200);
            e.HasIndex(x => x.Token).IsUnique();

            e.Property(x => x.UserAgent).HasMaxLength(500);
            e.Property(x => x.IpAddress).HasMaxLength(50);

            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.ExpiresAt);
        });

        mb.Entity<Project>(e =>
        {
            e.ToTable("projects");
            e.HasKey(x => x.Id);

            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Description).HasMaxLength(2000);
            e.Property(x => x.PublicSlug).HasMaxLength(64);

            e.HasIndex(x => x.UserId);
            e.HasIndex(x => x.PublicSlug).IsUnique().HasFilter("\"PublicSlug\" IS NOT NULL");

            e.Property(x => x.IsPublic).HasDefaultValue(false);
        });
    }

    public override int SaveChanges()
    {
        TouchUpdated();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        TouchUpdated();
        return base.SaveChangesAsync(ct);
    }

    private void TouchUpdated()
    {
        var entries = ChangeTracker.Entries<Project>()
            .Where(e => e.State == EntityState.Modified);
        foreach (var e in entries)
            e.Entity.UpdatedAt = DateTime.UtcNow;
    }
}
