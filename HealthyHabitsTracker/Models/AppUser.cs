using System.ComponentModel.DataAnnotations;

namespace HealthyHabitsTracker.Models
{
    public class AppUser
    {
        [Key]
        public string UserId { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;
    }
}
