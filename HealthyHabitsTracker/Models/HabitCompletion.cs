using System.ComponentModel.DataAnnotations;

namespace HealthyHabitsTracker.Models
{
    public class HabitCompletion
    {
        [Key]
        public int CompletionId { get; set; }

        [Required]
        public int HabitId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public DateTime CompletionDate { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Habit? Habit { get; set; }
    }
}
