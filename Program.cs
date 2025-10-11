using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MusicalScales.Api.Data;
using MusicalScales.Api.Repositories;
using MusicalScales.Api.Services;
using Serilog;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure URLs
builder.WebHost.UseUrls("http://localhost:5000", "https://localhost:5001");

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON serialization
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Keep original casing
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Configure Entity Framework
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=musicscales.db";

builder.Services.AddDbContext<MusicalScalesDbContext>(options =>
    options.UseSqlite(connectionString));

// Register repositories
builder.Services.AddScoped<IScaleRepository, ScaleRepository>();

// Register services
builder.Services.AddScoped<IScaleService, ScaleService>();
builder.Services.AddScoped<IPitchService, PitchService>();
builder.Services.AddScoped<IIntervalService, IntervalService>();

// Configure API Explorer for Swagger
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Musical Scales API",
        Description = "A modern object-oriented API for musical scales and intervals",
        Contact = new OpenApiContact
        {
            Name = "Musical Scales API",
            Email = "contact@example.com",
            Url = new Uri("https://github.com/yourusername/musical-scales")
        }
    });

    // Include XML comments
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Add enum descriptions
    options.SchemaFilter<EnumSchemaFilter>();
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<MusicalScalesDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Musical Scales API v1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at root
        options.DocumentTitle = "Musical Scales API";
    });
    
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    await next();
});

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

// Ensure database is created
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<MusicalScalesDbContext>();
    await context.Database.EnsureCreatedAsync();
    
    // Seed basic scales if database is empty
    if (!await context.Scales.AnyAsync())
    {
        await SeedBasicScales(context);
    }
    Log.Information("Database initialized successfully");
}
catch (Exception ex)
{
    Log.Error(ex, "Failed to initialize database");
    // Continue without seeding - don't crash the app
}

try
{
    Log.Information("Starting Musical Scales API");
    Log.Information("Swagger UI is available at the root path (/) when running in Development mode.");
    
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Helper method to seed basic scales
static async Task SeedBasicScales(MusicalScalesDbContext context)
{
    var majorScale = new MusicalScales.Api.Models.Scale
    {
        Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
        Metadata = new MusicalScales.Api.Models.ScaleMetadata 
        { 
            Names = new List<string> { "Major Scale", "Ionian Mode" },
            Description = "The most common scale in Western music",
            Origin = "Western"
        },
        Intervals = new List<MusicalScales.Api.Models.Interval>
        {
            new() { Name = MusicalScales.Api.Models.Enums.IntervalSizeName.Second, Quality = MusicalScales.Api.Models.Enums.IntervalQualityName.Major, PitchOffset = 1, SemitoneOffset = 2 },
            new() { Name = MusicalScales.Api.Models.Enums.IntervalSizeName.Third, Quality = MusicalScales.Api.Models.Enums.IntervalQualityName.Major, PitchOffset = 2, SemitoneOffset = 4 },
            new() { Name = MusicalScales.Api.Models.Enums.IntervalSizeName.Fourth, Quality = MusicalScales.Api.Models.Enums.IntervalQualityName.Perfect, PitchOffset = 3, SemitoneOffset = 5 },
            new() { Name = MusicalScales.Api.Models.Enums.IntervalSizeName.Fifth, Quality = MusicalScales.Api.Models.Enums.IntervalQualityName.Perfect, PitchOffset = 4, SemitoneOffset = 7 },
            new() { Name = MusicalScales.Api.Models.Enums.IntervalSizeName.Sixth, Quality = MusicalScales.Api.Models.Enums.IntervalQualityName.Major, PitchOffset = 5, SemitoneOffset = 9 },
            new() { Name = MusicalScales.Api.Models.Enums.IntervalSizeName.Seventh, Quality = MusicalScales.Api.Models.Enums.IntervalQualityName.Major, PitchOffset = 6, SemitoneOffset = 11 },
            new() { Name = MusicalScales.Api.Models.Enums.IntervalSizeName.Octave, Quality = MusicalScales.Api.Models.Enums.IntervalQualityName.Perfect, PitchOffset = 7, SemitoneOffset = 12 }
        }
    };
    
    var minorScale = new MusicalScales.Api.Models.Scale
    {
        Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"),
        Metadata = new MusicalScales.Api.Models.ScaleMetadata 
        { 
            Names = new List<string> { "Natural Minor Scale", "Aeolian Mode" },
            Description = "The natural minor scale",
            Origin = "Western"
        },
        Intervals = new List<MusicalScales.Api.Models.Interval>
        {
            new() { Name = MusicalScales.Api.Models.Enums.IntervalSizeName.Second, Quality = MusicalScales.Api.Models.Enums.IntervalQualityName.Major, PitchOffset = 1, SemitoneOffset = 2 },
            new() { Name = MusicalScales.Api.Models.Enums.IntervalSizeName.Third, Quality = MusicalScales.Api.Models.Enums.IntervalQualityName.Minor, PitchOffset = 2, SemitoneOffset = 3 },
            new() { Name = MusicalScales.Api.Models.Enums.IntervalSizeName.Fourth, Quality = MusicalScales.Api.Models.Enums.IntervalQualityName.Perfect, PitchOffset = 3, SemitoneOffset = 5 },
            new() { Name = MusicalScales.Api.Models.Enums.IntervalSizeName.Fifth, Quality = MusicalScales.Api.Models.Enums.IntervalQualityName.Perfect, PitchOffset = 4, SemitoneOffset = 7 },
            new() { Name = MusicalScales.Api.Models.Enums.IntervalSizeName.Sixth, Quality = MusicalScales.Api.Models.Enums.IntervalQualityName.Minor, PitchOffset = 5, SemitoneOffset = 8 },
            new() { Name = MusicalScales.Api.Models.Enums.IntervalSizeName.Seventh, Quality = MusicalScales.Api.Models.Enums.IntervalQualityName.Minor, PitchOffset = 6, SemitoneOffset = 10 },
            new() { Name = MusicalScales.Api.Models.Enums.IntervalSizeName.Octave, Quality = MusicalScales.Api.Models.Enums.IntervalQualityName.Perfect, PitchOffset = 7, SemitoneOffset = 12 }
        }
    };
    
    context.Scales.AddRange(majorScale, minorScale);
    await context.SaveChangesAsync();
}

/// <summary>
/// Schema filter to add enum descriptions to Swagger
/// </summary>
public class EnumSchemaFilter : Swashbuckle.AspNetCore.SwaggerGen.ISchemaFilter
{
    public void Apply(Microsoft.OpenApi.Models.OpenApiSchema schema, Swashbuckle.AspNetCore.SwaggerGen.SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum?.Clear();
            foreach (var enumName in Enum.GetNames(context.Type))
            {
                schema.Enum?.Add(new Microsoft.OpenApi.Any.OpenApiString(enumName));
            }
        }
    }
}