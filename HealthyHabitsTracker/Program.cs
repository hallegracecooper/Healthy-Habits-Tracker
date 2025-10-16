using HealthyHabitsTracker.Data;
using HealthyHabitsTracker.Components; // App
using HealthyHabitsTracker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

/// <summary>
/// Main application entry point for Healthy Habits Tracker
/// Configures services, middleware, and API endpoints for the habit tracking application
/// </summary>
var builder = WebApplication.CreateBuilder(args);

// ===================== SERVICE CONFIGURATION =====================

// Razor + Blazor (Server interactivity) - Enables server-side Blazor components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// EF Core (SQLite) - Database configuration with SQLite for local development
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Data Source=app.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString)
);

// Authentication & Authorization (Cookies) - Secure user authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";           // Redirect to login page when not authenticated
        options.LogoutPath = "/logout";          // Logout endpoint
        options.AccessDeniedPath = "/login";     // Redirect to login on access denied
        options.Cookie.Name = "HHT.Auth";       // Custom cookie name for the application
        options.SlidingExpiration = true;       // Extend session on activity
    });

builder.Services.AddAuthorization();           // Enable authorization policies
builder.Services.AddCascadingAuthenticationState(); // Make auth state available to components

// HTTP services for external requests and context access
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// Register custom business logic services
builder.Services.AddScoped<HealthyHabitsTracker.Services.HabitProgressService>();

var app = builder.Build();

// ===================== MIDDLEWARE PIPELINE =====================

// Error handling configuration
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");  // Custom error page for production
    app.UseHsts();                      // HTTP Strict Transport Security
}

// Security and static file middleware
app.UseHttpsRedirection();             // Redirect HTTP to HTTPS
app.UseStaticFiles();                  // Serve static files (CSS, JS, images)

// Authentication and authorization middleware (order matters!)
app.UseAuthentication();               // Must come before UseAuthorization
app.UseAuthorization();                // Enable authorization policies

// IMPORTANT: Antiforgery must be enabled for Blazor endpoints.
// (Keep it ON globally; we'll selectively disable it on auth APIs.)
app.UseAntiforgery();

// Map the Blazor application with server-side interactivity
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

// ===================== API ENDPOINTS =====================

// --- Minimal APIs for Register/Login/Logout --- //

/// <summary>
/// Helper method to read authentication credentials from either JSON or form data
/// Supports both API and web form submissions for maximum flexibility
/// </summary>
/// <param name="req">The HTTP request containing credentials</param>
/// <returns>Tuple containing email and password, or null values if not found</returns>
static async Task<(string? Email, string? Password)> ReadCredsAsync(HttpRequest req)
{
    if (req.HasJsonContentType())
    {
        try
        {
            if (req.Path.StartsWithSegments("/auth/register"))
            {
                var dto = await req.ReadFromJsonAsync<RegisterDto>();
                return (dto?.Email, dto?.Password);
            }
            else
            {
                var dto = await req.ReadFromJsonAsync<LoginDto>();
                return (dto?.Email, dto?.Password);
            }
        }
        catch { /* fall through to form */ }
    }

    if (req.HasFormContentType)
    {
        var form = await req.ReadFormAsync();
        var email = form["Email"].ToString();
        var password = form["Password"].ToString();
        return (email, password);
    }

    return (null, null);
}

/// <summary>
/// User registration endpoint - Creates new user accounts with secure password hashing
/// Supports both JSON API calls and form submissions for maximum flexibility
/// </summary>
/// <param name="req">HTTP request containing user credentials</param>
/// <param name="db">Database context for user storage</param>
/// <param name="http">HTTP context for authentication</param>
/// <returns>Redirect to dashboard on success, error response on failure</returns>
app.MapPost("/auth/register", async (HttpRequest req, AppDbContext db, HttpContext http) =>
{
    // Extract credentials from request (JSON or form)
    var (rawEmail, rawPassword) = await ReadCredsAsync(req);
    var email = (rawEmail ?? "").Trim().ToLowerInvariant();
    var password = (rawPassword ?? "").Trim();

    // Validate required fields
    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        return Results.BadRequest("Email and password are required.");

    // Check for existing user to prevent duplicates
    if (db.Users.Any(u => u.Email == email))
        return Results.Conflict("An account with this email already exists.");

    // Create new user with unique ID
    var user = new HealthyHabitsTracker.Models.AppUser
    {
        UserId = Guid.NewGuid().ToString("N"),  // Generate unique user identifier
        Email = email
    };

    // Hash password securely using ASP.NET Identity PasswordHasher
    var hasher = new PasswordHasher<HealthyHabitsTracker.Models.AppUser>();
    user.PasswordHash = hasher.HashPassword(user, password);

    // Save user to database
    db.Users.Add(user);
    await db.SaveChangesAsync();

    // Automatically sign in the new user
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserId),  // Primary user identifier
        new Claim(ClaimTypes.Name, user.Email),             // Display name
        new Claim(ClaimTypes.Email, user.Email),            // Email for identification
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    // Handle different response types based on request content type
    if (req.HasFormContentType)
        return Results.Redirect("/dashboard");  // Web form submission - redirect to dashboard
    else
        return Results.Ok();                    // API call - return success status
}); // NOTE: do NOT disable antiforgery here now

// Login (accepts JSON or form)
app.MapPost("/auth/login", async (HttpRequest req, AppDbContext db, HttpContext http) =>
{
    var (rawEmail, rawPassword) = await ReadCredsAsync(req);
    var email = (rawEmail ?? "").Trim().ToLowerInvariant();
    var password = (rawPassword ?? "").Trim();

    var user = db.Users.SingleOrDefault(u => u.Email == email);
    if (user is null) return Results.Unauthorized();

    var hasher = new PasswordHasher<HealthyHabitsTracker.Models.AppUser>();
    var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
    if (result == PasswordVerificationResult.Failed)
        return Results.Unauthorized();

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserId),
        new Claim(ClaimTypes.Name, user.Email),
        new Claim(ClaimTypes.Email, user.Email),
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    if (req.HasFormContentType)
        return Results.Redirect("/dashboard");

    return Results.Ok();
});

// Logout
app.MapPost("/auth/logout", async (HttpContext http) =>
{
    await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

// ===================== HABIT ENDPOINTS ==========================

// Helpers
static string? GetUserId(HttpContext http) =>
    http.User.FindFirstValue(ClaimTypes.NameIdentifier);

// CREATE
app.MapPost("/habits/create", async (HttpContext http, AppDbContext db) =>
{
    var userId = GetUserId(http);
    if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

    if (!http.Request.HasFormContentType) return Results.BadRequest("Invalid form.");

    var form = await http.Request.ReadFormAsync();
    var title = (form["Title"].ToString() ?? "").Trim();
    var description = (form["Description"].ToString() ?? "").Trim();

    if (string.IsNullOrWhiteSpace(title))
        return Results.BadRequest("Title is required.");

    var habit = new Habit
    {
        UserId = userId,
        Title = title,
        Description = string.IsNullOrWhiteSpace(description) ? null : description,
        IsComplete = false,
        DateCreated = DateTime.UtcNow,
        LastCompletedDate = null
    };

    db.Habits.Add(habit);
    await db.SaveChangesAsync();
    return Results.Redirect("/dashboard");
});

// EDIT
app.MapPost("/habits/edit", async (HttpContext http, AppDbContext db) =>
{
    var userId = GetUserId(http);
    if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();
    if (!http.Request.HasFormContentType) return Results.BadRequest("Invalid form.");

    var form = await http.Request.ReadFormAsync();
    if (!int.TryParse(form["HabitId"], out var habitId)) return Results.BadRequest("HabitId required.");

    var title = (form["Title"].ToString() ?? "").Trim();
    var description = (form["Description"].ToString() ?? "").Trim();

    var habit = db.Habits.FirstOrDefault(h => h.HabitId == habitId && h.UserId == userId);
    if (habit is null) return Results.NotFound();

    if (string.IsNullOrWhiteSpace(title))
        return Results.BadRequest("Title is required.");

    habit.Title = title;
    habit.Description = string.IsNullOrWhiteSpace(description) ? null : description;

    await db.SaveChangesAsync();
    return Results.Redirect("/dashboard");
});

// DELETE
app.MapPost("/habits/delete", async (HttpContext http, AppDbContext db) =>
{
    var userId = GetUserId(http);
    if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();
    if (!http.Request.HasFormContentType) return Results.BadRequest("Invalid form.");

    var form = await http.Request.ReadFormAsync();
    if (!int.TryParse(form["HabitId"], out var habitId)) return Results.BadRequest("HabitId required.");

    var habit = db.Habits.FirstOrDefault(h => h.HabitId == habitId && h.UserId == userId);
    if (habit is null) return Results.NotFound();

    db.Habits.Remove(habit);
    await db.SaveChangesAsync();
    return Results.Redirect("/dashboard");
});

// TOGGLE COMPLETE (mark complete/uncomplete)
app.MapPost("/habits/toggle", async (HttpContext http, AppDbContext db) =>
{
    var userId = GetUserId(http);
    if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();
    if (!http.Request.HasFormContentType) return Results.BadRequest("Invalid form.");

    var form = await http.Request.ReadFormAsync();
    if (!int.TryParse(form["HabitId"], out var habitId)) return Results.BadRequest("HabitId required.");

    var habit = db.Habits.FirstOrDefault(h => h.HabitId == habitId && h.UserId == userId);
    if (habit is null) return Results.NotFound();

    // Get the progress service
    var progressService = http.RequestServices.GetRequiredService<HealthyHabitsTracker.Services.HabitProgressService>();

    // Toggle: if complete -> uncomplete; else complete now
    if (habit.IsComplete)
    {
        // Currently complete, so uncomplete
        habit.IsComplete = false;
        habit.LastCompletedDate = null;
        await progressService.RemoveCompletionAsync(habitId, userId);
    }
    else
    {
        // Currently incomplete, so complete
        habit.IsComplete = true;
        habit.LastCompletedDate = DateTime.UtcNow;
        await progressService.RecordCompletionAsync(habitId, userId);
    }

    await db.SaveChangesAsync();
    return Results.Redirect("/dashboard");
});

// Ensure DB/migrations in dev
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();

// DTOs
record RegisterDto(string Email, string Password);
record LoginDto(string Email, string Password);