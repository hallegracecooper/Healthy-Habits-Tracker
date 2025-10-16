using System;
using System.ComponentModel.DataAnnotations;

namespace HealthyHabitsTracker.Models
{
    /// <summary>
    /// Represents a user's habit in the Healthy Habits Tracker application
    /// Contains habit metadata, completion status, and tracking information
    /// </summary>
    public class Habit
    {
        /// <summary>Primary key identifier for the habit</summary>
        [Key]
        public int HabitId { get; set; }

        /// <summary>Foreign key linking the habit to its owner (AppUser.UserId)</summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        /// <summary>Display name/title of the habit (e.g., "Drink 8 glasses of water")</summary>
        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty;

        /// <summary>Optional detailed description explaining the habit's importance or motivation</summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>Current completion status for today - true if habit is marked complete</summary>
        public bool IsComplete { get; set; }

        /// <summary>Timestamp when the habit was first created</summary>
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        /// <summary>Timestamp of the most recent completion (null if never completed)</summary>
        public DateTime? LastCompletedDate { get; set; }
    }
}
