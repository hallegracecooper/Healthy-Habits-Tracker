using HealthyHabitsTracker.Data;
using HealthyHabitsTracker.Components; // App
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Razor + Blazor (Server interactivity)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// EF Core (SQLite)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Data Source=app.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString)
);

// Authentication & Authorization (Cookies)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/login";
        options.Cookie.Name = "HHT.Auth";
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// IMPORTANT: Antiforgery must be enabled for Blazor endpoints.
// (Keep it ON globally; we'll selectively disable it on auth APIs.)
app.UseAntiforgery();

// Map the Blazor app
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

// --- Minimal APIs for Register/Login/Logout --- //

// Helper to read either JSON or form
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

// Register (accepts JSON or form)
app.MapPost("/auth/register", async (HttpRequest req, AppDbContext db, HttpContext http) =>
{
    var (rawEmail, rawPassword) = await ReadCredsAsync(req);
    var email = (rawEmail ?? "").Trim().ToLowerInvariant();
    var password = (rawPassword ?? "").Trim();

    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        return Results.BadRequest("Email and password are required.");

    if (db.Users.Any(u => u.Email == email))
        return Results.Conflict("An account with this email already exists.");

    var user = new HealthyHabitsTracker.Models.AppUser
    {
        UserId = Guid.NewGuid().ToString("N"),
        Email = email
    };

    var hasher = new PasswordHasher<HealthyHabitsTracker.Models.AppUser>();
    user.PasswordHash = hasher.HashPassword(user, password);

    db.Users.Add(user);
    await db.SaveChangesAsync();

    // Sign in
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserId),
        new Claim(ClaimTypes.Name, user.Email),
        new Claim(ClaimTypes.Email, user.Email),
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    // For form posts, redirect to dashboard; for JSON, Ok is fine
    if (req.HasFormContentType)
        return Results.Redirect("/dashboard");

    return Results.Ok();
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