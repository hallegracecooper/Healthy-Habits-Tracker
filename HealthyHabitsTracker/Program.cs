using HealthyHabitsTracker.Data;
using HealthyHabitsTracker.Components; // for App
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// === DbContext (SQLite) ===
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Data Source=app.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString)
);

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Map the root Razor component (Blazor Web App pattern)
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

// Ensure DB/migrations in dev
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();