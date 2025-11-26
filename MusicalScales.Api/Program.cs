using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MusicalScales.Api.Data;
using MusicalScales.Api.Repositories;
using MusicalScales.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// Configure Entity Framework
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=musicscales.db";
builder.Services.AddDbContext<MusicalScalesDbContext>(options => options.UseSqlite(connectionString));

// Register repositories
builder.Services.AddScoped<IScaleRepository, ScaleRepository>();

// Register services
builder.Services.AddScoped<IScaleService, ScaleService>();
builder.Services.AddScoped<IPitchService, PitchService>();
builder.Services.AddScoped<IIntervalService, IntervalService>();
builder.Services.AddScoped<DatabaseSeeder>();

// Register the Swagger generator, defining 1 or more Swagger documents
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Musical Scales API",
        Description = "An object oriented solution for musical scales",
        Contact = new OpenApiContact
        {
            Name = "Musical Scales API",
            Email = "contact@example.com"
        }
    });
    
    // Set the comments path for the Swagger JSON and UI
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add health checks
builder.Services.AddHealthChecks().AddDbContextCheck<MusicalScalesDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();

// Enable middleware to serve generated Swagger as a JSON endpoint.
app.UseSwagger();

// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
// specifying the Swagger JSON endpoint.
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Musical Scales API");
    c.RoutePrefix = string.Empty;
});

app.MapControllers();
app.MapHealthChecks("/health");

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

Console.WriteLine("🎵 Musical Scales API");
Console.WriteLine("📍 Application URLs:");
Console.WriteLine("   • http://localhost:5000 (Swagger UI)");
Console.WriteLine("   • https://localhost:5001 (Swagger UI)");
Console.WriteLine("   • /health (Health check)");
Console.WriteLine("🚀 Application started successfully!");

app.Run();

// Make the Program class accessible for integration testing
public partial class Program { }