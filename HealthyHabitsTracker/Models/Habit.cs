using System;
using System.ComponentModel.DataAnnotations;

namespace HealthyHabitsTracker.Models
{
    public class Habit
    {
        [Key]
        public int HabitId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        // Optional, we'll compute streaks from records later, but this lets us have a quick flag for the day.
        public bool IsComplete { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        // A convenience field you can use/ignore later
        public DateTime? LastCompletedDate { get; set; }
    }
}
