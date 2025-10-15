namespace HealthyHabitsTracker.Models;

public record RegisterDto(string Email, string Password);
public record LoginDto(string Email, string Password);

