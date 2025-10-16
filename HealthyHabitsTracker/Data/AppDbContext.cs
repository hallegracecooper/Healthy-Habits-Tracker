using HealthyHabitsTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthyHabitsTracker.Data
{
    /// <summary>
    /// Entity Framework database context for the Healthy Habits Tracker application
    /// Manages database operations for users, habits, and habit completions
    /// </summary>
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Initializes the database context with connection options
        /// </summary>
        /// <param name="options">Database connection and configuration options</param>
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // ===================== DATABASE ENTITIES =====================

        /// <summary>Database table for user habits - stores habit definitions and metadata</summary>
        public DbSet<Habit> Habits => Set<Habit>();

        /// <summary>Database table for user accounts - stores authentication information</summary>
        public DbSet<AppUser> Users => Set<AppUser>();

        /// <summary>Database table for habit completions - tracks daily completion records for streak calculation</summary>
        public DbSet<HabitCompletion> HabitCompletions => Set<HabitCompletion>();

        /// <summary>
        /// Configures database schema, relationships, and indexes for optimal performance
        /// Called during database migration and model creation
        /// </summary>
        /// <param name="modelBuilder">Entity Framework model builder for configuration</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Create composite index for habits to optimize user-specific habit queries
            // Allows fast lookups of habits by user and title combination
            modelBuilder.Entity<Habit>()
                .HasIndex(h => new { h.UserId, h.Title })
                .IsUnique(false);  // Allow duplicate titles for different users

            // Create index for habit completions by habit and date for streak calculations
            // Optimizes queries that need to find completions for specific habits on specific dates
            modelBuilder.Entity<HabitCompletion>()
                .HasIndex(hc => new { hc.HabitId, hc.CompletionDate })
                .IsUnique(false);

            // Create index for habit completions by user and date for weekly summaries
            // Optimizes queries that need to find all completions for a user within date ranges
            modelBuilder.Entity<HabitCompletion>()
                .HasIndex(hc => new { hc.UserId, hc.CompletionDate })
                .IsUnique(false);
        }
    }
}
