using HealthyHabitsTracker.Data;
using Microsoft.EntityFrameworkCore;
using HealthyHabitsTracker.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// === DbContext registration (SQLite) ===
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                      ?? "Data Source=app.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString)
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Ensure database exists / apply migrations at startup (ok for dev)
// Comment out later if you prefer CLI migrations only.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();