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
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<UserRating> UserRatings => Set<UserRating>();

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

        mb.Entity<Assignment>(e =>
{
    e.ToTable("assignments");
    e.HasKey(x => x.Id);

    e.Property(x => x.Title).IsRequired().HasMaxLength(200);
    e.Property(x => x.Description).HasMaxLength(4000);
    e.Property(x => x.Mode).IsRequired().HasMaxLength(20).HasDefaultValue("visual");
    e.Property(x => x.ReferenceLanguage).IsRequired().HasMaxLength(40).HasDefaultValue("python");
    e.Property(x => x.CompareTarget).IsRequired().HasMaxLength(200).HasDefaultValue("__return__");

    // Списки как JSONB (PostgreSQL native)
    e.Property(x => x.AllowedLanguages)
     .HasColumnType("jsonb")
     .HasConversion(
        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
        v => string.IsNullOrEmpty(v)
             ? new List<string>()
             : System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>());

    e.HasOne(x => x.Author)
     .WithMany(u => u.CreatedAssignments)
     .HasForeignKey(x => x.AuthorId)
     .OnDelete(DeleteBehavior.Restrict);

    e.HasIndex(x => x.AuthorId);
    e.HasIndex(x => x.IsPublished);
});

        mb.Entity<Submission>(e =>
        {
            e.ToTable("submissions");
            e.HasKey(x => x.Id);

            e.Property(x => x.Language).IsRequired().HasMaxLength(40);
            e.Property(x => x.Status).IsRequired().HasMaxLength(20).HasDefaultValue("pending");
            e.Property(x => x.Grade).HasMaxLength(20);

            e.HasOne(x => x.Assignment)
             .WithMany(a => a.Submissions)
             .HasForeignKey(x => x.AssignmentId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.User)
             .WithMany(u => u.Submissions)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.AssignmentId, x.UserId });
            e.HasIndex(x => x.UserId);
        });

        mb.Entity<Comment>(e =>
        {
            e.ToTable("comments");
            e.HasKey(x => x.Id);

            e.Property(x => x.Text).IsRequired().HasMaxLength(4000);

            e.HasOne(x => x.Submission)
             .WithMany(s => s.Comments)
             .HasForeignKey(x => x.SubmissionId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Author)
             .WithMany(u => u.Comments)
             .HasForeignKey(x => x.AuthorId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.SubmissionId);
        });

        mb.Entity<UserRating>(e =>
        {
            e.ToTable("user_ratings");
            e.HasKey(x => x.Id);

            e.HasOne(x => x.User)
             .WithOne(u => u.Rating)
             .HasForeignKey<UserRating>(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => x.UserId).IsUnique();
            e.HasIndex(x => x.PlayerXp).IsDescending();
            e.HasIndex(x => x.TeacherRating).IsDescending();
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
