using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HealthyHabitsTracker.Models
{
    // Inherit from IdentityUser to get secure password hashing, lockout, etc.
    public class AppUser : IdentityUser
    {
        // You can add profile fields here later (e.g., DisplayName)
        // Example:
        // [StringLength(100)]
        // public string? DisplayName { get; set; }
    }
}