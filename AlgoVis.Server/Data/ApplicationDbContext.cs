using AlgoVis.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoVis.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<VisualizationSession> Sessions { get; set; }
        public DbSet<VisualizationStep> Steps { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); 

            modelBuilder.Entity<VisualizationSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ClientConnectionId);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.Status);

                entity.HasMany(e => e.Steps)
                      .WithOne(e => e.Session)
                      .HasForeignKey(e => e.SessionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<VisualizationStep>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SessionId);
                entity.HasIndex(e => e.StepNumber);

                entity.HasIndex(e => new { e.SessionId, e.StepNumber })
                      .IsUnique();
            });
        }
    }
}