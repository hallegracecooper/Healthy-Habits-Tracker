using HealthyHabitsTracker.Data;
using HealthyHabitsTracker.Components;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services for Blazor Web App (Server interactivity)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// === EF Core DbContext (SQLite for dev) ===
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Data Source=app.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString)
);

var app = builder.Build();

// Error handling & security
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// Map the root Razor component (App.razor)
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Apply migrations on startup (dev convenience)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();