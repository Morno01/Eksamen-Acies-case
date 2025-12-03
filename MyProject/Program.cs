using Microsoft.EntityFrameworkCore;
using MyProject.Data;
using MyProject.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add DbContext
builder.Services.AddDbContext<PalleOptimeringContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register application services
builder.Services.AddScoped<IPalleService, PalleService>();
builder.Services.AddScoped<IElementService, ElementService>();
builder.Services.AddScoped<IPalleOptimeringSettingsService, PalleOptimeringSettingsService>();
builder.Services.AddScoped<IPalleOptimeringService, PalleOptimeringService>();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
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

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
