using System.ComponentModel.DataAnnotations;

namespace HealthyHabitsTracker.Models
{
    public class AppUser
    {
        [Key]
        public string UserId { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        // We'll store hashed passwords later when we add auth.
        public string PasswordHash { get; set; } = string.Empty;
    }
}
