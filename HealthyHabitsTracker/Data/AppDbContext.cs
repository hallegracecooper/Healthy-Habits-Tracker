using HealthyHabitsTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthyHabitsTracker.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Habit> Habits => Set<Habit>();
        public DbSet<AppUser> Users => Set<AppUser>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Example: index on UserId + Title to avoid duplicate habit titles per user (optional right now)
            modelBuilder.Entity<Habit>()
                .HasIndex(h => new { h.UserId, h.Title })
                .IsUnique(false);
        }
    }
}
