using Microsoft.EntityFrameworkCore;
using MusicalScales.Api.Data;
using MusicalScales.Api.Models;
using MusicalScales.Api.Models.Enums;

namespace MusicalScales.Api.Services;

/// <summary>
/// Service for seeding initial data into the database
/// </summary>
public class DatabaseSeeder
{
    private readonly MusicalScalesDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(MusicalScalesDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            await _context.Database.EnsureCreatedAsync();

            if (await _context.Scales.AnyAsync())
            {
                _logger.LogInformation("Database already contains scales, skipping seed");
                return;
            }

            var scales = GetSeedScales();
            _context.Scales.AddRange(scales);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully seeded {Count} scales", scales.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed database");
            throw;
        }
    }

    private static List<Scale> GetSeedScales()
    {
        return
        [
            new()
            {
                Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440001"),
                Metadata = new ScaleMetadata
                {
                    Names = ["Major Scale", "Ionian Mode"],
                    Description = "The most common scale in Western music"
                },
                Intervals =
                [
                    new() { Name = IntervalSizeName.Second, Quality = IntervalQualityName.Major, PitchOffset = 1, SemitoneOffset = 2 },
                    new() { Name = IntervalSizeName.Third, Quality = IntervalQualityName.Major, PitchOffset = 2, SemitoneOffset = 4 },
                    new() { Name = IntervalSizeName.Fourth, Quality = IntervalQualityName.Perfect, PitchOffset = 3, SemitoneOffset = 5 },
                    new() { Name = IntervalSizeName.Fifth, Quality = IntervalQualityName.Perfect, PitchOffset = 4, SemitoneOffset = 7 },
                    new() { Name = IntervalSizeName.Sixth, Quality = IntervalQualityName.Major, PitchOffset = 5, SemitoneOffset = 9 },
                    new() { Name = IntervalSizeName.Seventh, Quality = IntervalQualityName.Major, PitchOffset = 6, SemitoneOffset = 11 },
                    new() { Name = IntervalSizeName.Octave, Quality = IntervalQualityName.Perfect, PitchOffset = 7, SemitoneOffset = 12 }
                ]
            },
            new()
            {
                Id = Guid.Parse("550e8400-e29b-41d4-a716-446655440002"),
                Metadata = new ScaleMetadata
                {
                    Names = ["Natural Minor Scale", "Aeolian Mode"],
                    Description = "The natural minor scale"
                },
                Intervals =
                [
                    new() { Name = IntervalSizeName.Second, Quality = IntervalQualityName.Major, PitchOffset = 1, SemitoneOffset = 2 },
                    new() { Name = IntervalSizeName.Third, Quality = IntervalQualityName.Minor, PitchOffset = 2, SemitoneOffset = 3 },
                    new() { Name = IntervalSizeName.Fourth, Quality = IntervalQualityName.Perfect, PitchOffset = 3, SemitoneOffset = 5 },
                    new() { Name = IntervalSizeName.Fifth, Quality = IntervalQualityName.Perfect, PitchOffset = 4, SemitoneOffset = 7 },
                    new() { Name = IntervalSizeName.Sixth, Quality = IntervalQualityName.Minor, PitchOffset = 5, SemitoneOffset = 8 },
                    new() { Name = IntervalSizeName.Seventh, Quality = IntervalQualityName.Minor, PitchOffset = 6, SemitoneOffset = 10 },
                    new() { Name = IntervalSizeName.Octave, Quality = IntervalQualityName.Perfect, PitchOffset = 7, SemitoneOffset = 12 }
                ]
            }
        ];
    }
}