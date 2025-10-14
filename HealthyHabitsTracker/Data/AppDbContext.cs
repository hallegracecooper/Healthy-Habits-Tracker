using HealthyHabitsTracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HealthyHabitsTracker.Data
{
    // Use IdentityDbContext so ASP.NET Identity can create its tables
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Habit> Habits => Set<Habit>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Example index to speed up common queries
            modelBuilder.Entity<Habit>()
                .HasIndex(h => new { h.UserId, h.Title })
                .IsUnique(false);
        }
    }
}