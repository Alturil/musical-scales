using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MusicalScales.Api.Data;
using MusicalScales.Api.Models;
using MusicalScales.Api.Services;

namespace MusicalScales.Tests.Services;

public class DatabaseSeederTests : IDisposable
{
    private readonly MusicalScalesDbContext _context;
    private readonly Mock<ILogger<DatabaseSeeder>> _mockLogger;
    private readonly DatabaseSeeder _databaseSeeder;
    private readonly DbContextOptions<MusicalScalesDbContext> _options;

    public DatabaseSeederTests()
    {
        // Create a unique database name for each test to avoid conflicts
        var databaseName = Guid.NewGuid().ToString();
        
        _options = new DbContextOptionsBuilder<MusicalScalesDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;
        
        _context = new MusicalScalesDbContext(_options);
        _mockLogger = new Mock<ILogger<DatabaseSeeder>>();
        _databaseSeeder = new DatabaseSeeder(_context, _mockLogger.Object);
    }

    [Fact]
    public async Task SeedAsync_WithEmptyDatabase_SeedsScales()
    {
        // Arrange
        // Ensure database is empty
        _context.Scales.Should().BeEmpty();

        // Act
        await _databaseSeeder.SeedAsync();

        // Assert
        var scales = await _context.Scales.ToListAsync();
        scales.Should().NotBeEmpty();
        scales.Should().HaveCountGreaterThan(0);

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Successfully seeded")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SeedAsync_WithExistingScales_SkipsSeeding()
    {
        // Arrange
        // Add a scale to make database non-empty
        var existingScale = new Scale
        {
            Id = Guid.NewGuid(),
            Metadata = new ScaleMetadata { Names = ["Existing Scale"] },
            Intervals = new List<Interval>()
        };
        
        _context.Scales.Add(existingScale);
        await _context.SaveChangesAsync();

        var initialCount = await _context.Scales.CountAsync();
        initialCount.Should().Be(1);

        // Act
        await _databaseSeeder.SeedAsync();

        // Assert
        var finalCount = await _context.Scales.CountAsync();
        finalCount.Should().Be(initialCount); // Should not have changed

        // Verify logging
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("already contains scales, skipping seed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SeedAsync_EnsuresDatabaseIsCreated()
    {
        // Arrange
        // Use a fresh context to test database creation
        using var newContext = new MusicalScalesDbContext(_options);
        var newSeeder = new DatabaseSeeder(newContext, _mockLogger.Object);

        // Act
        await newSeeder.SeedAsync();

        // Assert
        // If this completes without exception, the database was successfully created
        var scales = await newContext.Scales.ToListAsync();
        scales.Should().NotBeNull();
    }

    [Fact]
    public async Task SeedAsync_WithDatabaseException_LogsErrorAndRethrows()
    {
        // Arrange
        // Use a context that will have issues by disposing it first
        _context.Dispose();
        var seeder = new DatabaseSeeder(_context, _mockLogger.Object);

        // Act & Assert
        await seeder.Invoking(s => s.SeedAsync())
            .Should().ThrowAsync<ObjectDisposedException>();

        // Verify error logging occurred (the actual exception might be different but logging should happen)
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to seed database")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SeedAsync_SeededScalesHaveValidData()
    {
        // Arrange
        _context.Scales.Should().BeEmpty();

        // Act
        await _databaseSeeder.SeedAsync();

        // Assert
        var scales = await _context.Scales.ToListAsync();

        scales.Should().NotBeEmpty();

        foreach (var scale in scales)
        {
            // Each scale should have metadata
            scale.Metadata.Should().NotBeNull();
            
            // Each scale should have at least one name
            scale.Metadata!.Names.Should().NotBeNullOrEmpty();
            scale.Metadata.Names.Should().AllSatisfy(name => name.Should().NotBeNullOrWhiteSpace());
            
            // Each scale should have intervals (except possibly unison)
            scale.Intervals.Should().NotBeNull();
            
            // Each scale should have a valid ID
            scale.Id.Should().NotBe(Guid.Empty);
        }
    }

    [Fact]
    public async Task SeedAsync_CanBeCalledMultipleTimes()
    {
        // Arrange & Act
        await _databaseSeeder.SeedAsync(); // First call
        var firstCount = await _context.Scales.CountAsync();

        await _databaseSeeder.SeedAsync(); // Second call
        var secondCount = await _context.Scales.CountAsync();

        // Assert
        firstCount.Should().BeGreaterThan(0);
        secondCount.Should().Be(firstCount); // Should not change on second call

        // Verify that skip message was logged on second call
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("already contains scales, skipping seed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SeedAsync_SeededScalesArePersistedToDatabase()
    {
        // Arrange
        await _databaseSeeder.SeedAsync();
        
        // Act - Create a new context to verify data was actually saved
        using var newContext = new MusicalScalesDbContext(_options);
        var scales = await newContext.Scales.ToListAsync();

        // Assert
        scales.Should().NotBeEmpty();
        scales.Should().HaveCountGreaterThan(0);
        
        // Verify at least one common scale exists
        scales.Should().Contain(s => s.Metadata!.Names.Any(name => 
            name.Contains("Major", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("Minor", StringComparison.OrdinalIgnoreCase)));
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}