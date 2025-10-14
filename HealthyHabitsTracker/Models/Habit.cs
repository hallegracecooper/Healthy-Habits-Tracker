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

        public bool IsComplete { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public DateTime? LastCompletedDate { get; set; }
    }
}
