using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MusicalScales.Api.Data;
using MusicalScales.Api.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MusicalScales.IntegrationTests.Fixtures;

/// <summary>
/// Custom WebApplicationFactory that configures the test application with isolated in-memory database
/// </summary>
public class MusicalScalesWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName;

    public MusicalScalesWebApplicationFactory()
    {
        // Generate unique database name per factory instance for test isolation
        _databaseName = $"TestDb_{Guid.NewGuid()}";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<MusicalScalesDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add InMemory database with unique name for isolation
            services.AddDbContext<MusicalScalesDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            // Ensure JSON serialization matches the API exactly
            services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });
        });

        builder.UseEnvironment("Testing");
    }

    protected override void ConfigureClient(HttpClient client)
    {
        base.ConfigureClient(client);
        
        // Configure JSON serialization options to match the API
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    /// <summary>
    /// Creates a properly configured HttpClient with JSON options matching the API
    /// </summary>
    public new HttpClient CreateClient()
    {
        var client = base.CreateClient();
        return client;
    }

    /// <summary>
    /// Gets JsonSerializerOptions that match the API configuration
    /// </summary>
    public JsonSerializerOptions GetJsonSerializerOptions()
    {
        return new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() },
            PropertyNamingPolicy = null
        };
    }
}