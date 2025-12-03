using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MyProject.Data;
using MyProject.Models;
using MyProject.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add DbContext
builder.Services.AddDbContext<PalleOptimeringContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 4;
})
.AddEntityFrameworkStores<PalleOptimeringContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Register application services
builder.Services.AddScoped<IPalleService, PalleService>();
builder.Services.AddScoped<IElementService, ElementService>();
builder.Services.AddScoped<IPalleOptimeringSettingsService, PalleOptimeringSettingsService>();
builder.Services.AddScoped<IPalleOptimeringService, PalleOptimeringService>();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Palleoptimering API",
        Version = "v1",
        Description = "API til palleoptimering system for Acies"
    });
});

// Add CORS if needed
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Log database connection info
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var logger = app.Services.GetRequiredService<ILogger<Program>>();
if (connectionString?.Contains("bmm-server.database.windows.net") == true)
{
    logger.LogInformation("✓ Forbundet til Azure SQL Database: bmm-server.database.windows.net");
}
else if (connectionString?.Contains("localdb") == true)
{
    logger.LogInformation("⚠ Forbundet til LocalDB (ikke Azure)");
}
else
{
    logger.LogInformation("Database connection string: {ConnectionString}", connectionString?.Substring(0, Math.Min(50, connectionString.Length ?? 0)));
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Palleoptimering API V1");
    });
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed database med roller og brugere
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await MyProject.Data.DbInitializer.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "En fejl opstod ved seeding af databasen.");
    }
}

app.Run();
