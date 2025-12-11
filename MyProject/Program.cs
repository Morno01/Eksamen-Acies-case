using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MyProject.Data;
using MyProject.Models;
using MyProject.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


builder.Services.AddDbContext<PalleOptimeringContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


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


builder.Services.AddScoped<IPalleService, PalleService>();
builder.Services.AddScoped<IElementService, ElementService>();
builder.Services.AddScoped<IPalleOptimeringSettingsService, PalleOptimeringSettingsService>();
builder.Services.AddScoped<IPalleOptimeringService, PalleOptimeringService>();


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


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<PalleOptimeringContext>();

    logger.LogInformation("🔄 Kører database migrations...");
    db.Database.Migrate();
    logger.LogInformation("✓ Database migrations kørt succesfuldt");


    var elementCount = db.Elementer.Count();
    logger.LogInformation($"📊 Antal elementer i database: {elementCount}");

    if (elementCount == 0)
    {
        logger.LogWarning("⚠️ Ingen elementer fundet i database - der kan være et problem med seed data");
    }
}



var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var appLogger = app.Services.GetRequiredService<ILogger<Program>>();
if (connectionString?.Contains("bmm-server.database.windows.net") == true)
{
    appLogger.LogInformation("✓ Forbundet til Azure SQL Database: bmm-server.database.windows.net");
}
else if (connectionString?.Contains("localdb") == true)
{
    appLogger.LogInformation("⚠ Forbundet til LocalDB (ikke Azure)");
}
else
{
    var displayLength = connectionString != null ? Math.Min(50, connectionString.Length) : 0;
    appLogger.LogInformation("Database connection string: {ConnectionString}", connectionString?.Substring(0, displayLength));
}


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
