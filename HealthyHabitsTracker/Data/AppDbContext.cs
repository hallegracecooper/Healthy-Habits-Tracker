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

            modelBuilder.Entity<Habit>()
                .HasIndex(h => new { h.UserId, h.Title })
                .IsUnique(false);
        }
    }
}
