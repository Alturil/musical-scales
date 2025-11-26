using MusicalScales.Api.Models;
using MusicalScales.Api.Models.Enums;
using MusicalScales.Api.Repositories;

namespace MusicalScales.Api.Services;

/// <summary>
/// Service for seeding initial data into DynamoDB
/// </summary>
public class DynamoDbSeeder
{
    private readonly IScaleRepository _repository;
    private readonly ILogger<DynamoDbSeeder> _logger;

    public DynamoDbSeeder(IScaleRepository repository, ILogger<DynamoDbSeeder> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Checking if DynamoDB needs seeding...");

            var existingScales = await _repository.GetAllScalesAsync();
            if (existingScales.Any())
            {
                _logger.LogInformation("DynamoDB already has {Count} scales, skipping seed", existingScales.Count());
                return;
            }

            _logger.LogInformation("Seeding DynamoDB with standard scales...");

            var scales = GetSeedScales();
            foreach (var scale in scales)
            {
                await _repository.CreateScaleAsync(scale);
                _logger.LogInformation("Seeded: {ScaleName}", scale.PrimaryName);
            }

            _logger.LogInformation("Successfully seeded {Count} scales to DynamoDB", scales.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed DynamoDB");
            throw;
        }
    }

    private static List<Scale> GetSeedScales()
    {
        return
        [
            new()
            {
                Metadata = new ScaleMetadata
                {
                    Names = ["Major", "Ionian"],
                    Description = "The most common scale in Western music, known for its bright and happy sound."
                },
                Intervals =
                [
                    new() { Name = IntervalSizeName.Unison, Quality = IntervalQualityName.Perfect, PitchOffset = 0, SemitoneOffset = 0 },
                    new() { Name = IntervalSizeName.Second, Quality = IntervalQualityName.Major, PitchOffset = 1, SemitoneOffset = 2 },
                    new() { Name = IntervalSizeName.Third, Quality = IntervalQualityName.Major, PitchOffset = 2, SemitoneOffset = 4 },
                    new() { Name = IntervalSizeName.Fourth, Quality = IntervalQualityName.Perfect, PitchOffset = 3, SemitoneOffset = 5 },
                    new() { Name = IntervalSizeName.Fifth, Quality = IntervalQualityName.Perfect, PitchOffset = 4, SemitoneOffset = 7 },
                    new() { Name = IntervalSizeName.Sixth, Quality = IntervalQualityName.Major, PitchOffset = 5, SemitoneOffset = 9 },
                    new() { Name = IntervalSizeName.Seventh, Quality = IntervalQualityName.Major, PitchOffset = 6, SemitoneOffset = 11 }
                ]
            },
            new()
            {
                Metadata = new ScaleMetadata
                {
                    Names = ["Natural Minor", "Aeolian"],
                    Description = "The relative minor scale, known for its melancholic and somber character."
                },
                Intervals =
                [
                    new() { Name = IntervalSizeName.Unison, Quality = IntervalQualityName.Perfect, PitchOffset = 0, SemitoneOffset = 0 },
                    new() { Name = IntervalSizeName.Second, Quality = IntervalQualityName.Major, PitchOffset = 1, SemitoneOffset = 2 },
                    new() { Name = IntervalSizeName.Third, Quality = IntervalQualityName.Minor, PitchOffset = 2, SemitoneOffset = 3 },
                    new() { Name = IntervalSizeName.Fourth, Quality = IntervalQualityName.Perfect, PitchOffset = 3, SemitoneOffset = 5 },
                    new() { Name = IntervalSizeName.Fifth, Quality = IntervalQualityName.Perfect, PitchOffset = 4, SemitoneOffset = 7 },
                    new() { Name = IntervalSizeName.Sixth, Quality = IntervalQualityName.Minor, PitchOffset = 5, SemitoneOffset = 8 },
                    new() { Name = IntervalSizeName.Seventh, Quality = IntervalQualityName.Minor, PitchOffset = 6, SemitoneOffset = 10 }
                ]
            },
            new()
            {
                Metadata = new ScaleMetadata
                {
                    Names = ["Harmonic Minor"],
                    Description = "A minor scale with a raised 7th degree, creating a distinctive exotic sound."
                },
                Intervals =
                [
                    new() { Name = IntervalSizeName.Unison, Quality = IntervalQualityName.Perfect, PitchOffset = 0, SemitoneOffset = 0 },
                    new() { Name = IntervalSizeName.Second, Quality = IntervalQualityName.Major, PitchOffset = 1, SemitoneOffset = 2 },
                    new() { Name = IntervalSizeName.Third, Quality = IntervalQualityName.Minor, PitchOffset = 2, SemitoneOffset = 3 },
                    new() { Name = IntervalSizeName.Fourth, Quality = IntervalQualityName.Perfect, PitchOffset = 3, SemitoneOffset = 5 },
                    new() { Name = IntervalSizeName.Fifth, Quality = IntervalQualityName.Perfect, PitchOffset = 4, SemitoneOffset = 7 },
                    new() { Name = IntervalSizeName.Sixth, Quality = IntervalQualityName.Minor, PitchOffset = 5, SemitoneOffset = 8 },
                    new() { Name = IntervalSizeName.Seventh, Quality = IntervalQualityName.Major, PitchOffset = 6, SemitoneOffset = 11 }
                ]
            },
            new()
            {
                Metadata = new ScaleMetadata
                {
                    Names = ["Melodic Minor", "Jazz Minor"],
                    Description = "A minor scale with raised 6th and 7th degrees ascending."
                },
                Intervals =
                [
                    new() { Name = IntervalSizeName.Unison, Quality = IntervalQualityName.Perfect, PitchOffset = 0, SemitoneOffset = 0 },
                    new() { Name = IntervalSizeName.Second, Quality = IntervalQualityName.Major, PitchOffset = 1, SemitoneOffset = 2 },
                    new() { Name = IntervalSizeName.Third, Quality = IntervalQualityName.Minor, PitchOffset = 2, SemitoneOffset = 3 },
                    new() { Name = IntervalSizeName.Fourth, Quality = IntervalQualityName.Perfect, PitchOffset = 3, SemitoneOffset = 5 },
                    new() { Name = IntervalSizeName.Fifth, Quality = IntervalQualityName.Perfect, PitchOffset = 4, SemitoneOffset = 7 },
                    new() { Name = IntervalSizeName.Sixth, Quality = IntervalQualityName.Major, PitchOffset = 5, SemitoneOffset = 9 },
                    new() { Name = IntervalSizeName.Seventh, Quality = IntervalQualityName.Major, PitchOffset = 6, SemitoneOffset = 11 }
                ]
            },
            new()
            {
                Metadata = new ScaleMetadata
                {
                    Names = ["Phrygian"],
                    Description = "The third mode of the major scale, with a distinctive Spanish or Middle Eastern flavor."
                },
                Intervals =
                [
                    new() { Name = IntervalSizeName.Unison, Quality = IntervalQualityName.Perfect, PitchOffset = 0, SemitoneOffset = 0 },
                    new() { Name = IntervalSizeName.Second, Quality = IntervalQualityName.Minor, PitchOffset = 1, SemitoneOffset = 1 },
                    new() { Name = IntervalSizeName.Third, Quality = IntervalQualityName.Minor, PitchOffset = 2, SemitoneOffset = 3 },
                    new() { Name = IntervalSizeName.Fourth, Quality = IntervalQualityName.Perfect, PitchOffset = 3, SemitoneOffset = 5 },
                    new() { Name = IntervalSizeName.Fifth, Quality = IntervalQualityName.Perfect, PitchOffset = 4, SemitoneOffset = 7 },
                    new() { Name = IntervalSizeName.Sixth, Quality = IntervalQualityName.Minor, PitchOffset = 5, SemitoneOffset = 8 },
                    new() { Name = IntervalSizeName.Seventh, Quality = IntervalQualityName.Minor, PitchOffset = 6, SemitoneOffset = 10 }
                ]
            },
            new()
            {
                Metadata = new ScaleMetadata
                {
                    Names = ["Lydian"],
                    Description = "The fourth mode of the major scale, known for its bright and dreamy quality."
                },
                Intervals =
                [
                    new() { Name = IntervalSizeName.Unison, Quality = IntervalQualityName.Perfect, PitchOffset = 0, SemitoneOffset = 0 },
                    new() { Name = IntervalSizeName.Second, Quality = IntervalQualityName.Major, PitchOffset = 1, SemitoneOffset = 2 },
                    new() { Name = IntervalSizeName.Third, Quality = IntervalQualityName.Major, PitchOffset = 2, SemitoneOffset = 4 },
                    new() { Name = IntervalSizeName.Fourth, Quality = IntervalQualityName.Augmented, PitchOffset = 3, SemitoneOffset = 6 },
                    new() { Name = IntervalSizeName.Fifth, Quality = IntervalQualityName.Perfect, PitchOffset = 4, SemitoneOffset = 7 },
                    new() { Name = IntervalSizeName.Sixth, Quality = IntervalQualityName.Major, PitchOffset = 5, SemitoneOffset = 9 },
                    new() { Name = IntervalSizeName.Seventh, Quality = IntervalQualityName.Major, PitchOffset = 6, SemitoneOffset = 11 }
                ]
            },
            new()
            {
                Metadata = new ScaleMetadata
                {
                    Names = ["Mixolydian"],
                    Description = "The fifth mode of the major scale, commonly used in blues and rock music."
                },
                Intervals =
                [
                    new() { Name = IntervalSizeName.Unison, Quality = IntervalQualityName.Perfect, PitchOffset = 0, SemitoneOffset = 0 },
                    new() { Name = IntervalSizeName.Second, Quality = IntervalQualityName.Major, PitchOffset = 1, SemitoneOffset = 2 },
                    new() { Name = IntervalSizeName.Third, Quality = IntervalQualityName.Major, PitchOffset = 2, SemitoneOffset = 4 },
                    new() { Name = IntervalSizeName.Fourth, Quality = IntervalQualityName.Perfect, PitchOffset = 3, SemitoneOffset = 5 },
                    new() { Name = IntervalSizeName.Fifth, Quality = IntervalQualityName.Perfect, PitchOffset = 4, SemitoneOffset = 7 },
                    new() { Name = IntervalSizeName.Sixth, Quality = IntervalQualityName.Major, PitchOffset = 5, SemitoneOffset = 9 },
                    new() { Name = IntervalSizeName.Seventh, Quality = IntervalQualityName.Minor, PitchOffset = 6, SemitoneOffset = 10 }
                ]
            },
            new()
            {
                Metadata = new ScaleMetadata
                {
                    Names = ["Locrian"],
                    Description = "The seventh mode of the major scale, with a dark and unstable character."
                },
                Intervals =
                [
                    new() { Name = IntervalSizeName.Unison, Quality = IntervalQualityName.Perfect, PitchOffset = 0, SemitoneOffset = 0 },
                    new() { Name = IntervalSizeName.Second, Quality = IntervalQualityName.Minor, PitchOffset = 1, SemitoneOffset = 1 },
                    new() { Name = IntervalSizeName.Third, Quality = IntervalQualityName.Minor, PitchOffset = 2, SemitoneOffset = 3 },
                    new() { Name = IntervalSizeName.Fourth, Quality = IntervalQualityName.Perfect, PitchOffset = 3, SemitoneOffset = 5 },
                    new() { Name = IntervalSizeName.Fifth, Quality = IntervalQualityName.Diminished, PitchOffset = 4, SemitoneOffset = 6 },
                    new() { Name = IntervalSizeName.Sixth, Quality = IntervalQualityName.Minor, PitchOffset = 5, SemitoneOffset = 8 },
                    new() { Name = IntervalSizeName.Seventh, Quality = IntervalQualityName.Minor, PitchOffset = 6, SemitoneOffset = 10 }
                ]
            },
            new()
            {
                Metadata = new ScaleMetadata
                {
                    Names = ["Dorian"],
                    Description = "The second mode of the major scale, with a minor quality but raised 6th degree."
                },
                Intervals =
                [
                    new() { Name = IntervalSizeName.Unison, Quality = IntervalQualityName.Perfect, PitchOffset = 0, SemitoneOffset = 0 },
                    new() { Name = IntervalSizeName.Second, Quality = IntervalQualityName.Major, PitchOffset = 1, SemitoneOffset = 2 },
                    new() { Name = IntervalSizeName.Third, Quality = IntervalQualityName.Minor, PitchOffset = 2, SemitoneOffset = 3 },
                    new() { Name = IntervalSizeName.Fourth, Quality = IntervalQualityName.Perfect, PitchOffset = 3, SemitoneOffset = 5 },
                    new() { Name = IntervalSizeName.Fifth, Quality = IntervalQualityName.Perfect, PitchOffset = 4, SemitoneOffset = 7 },
                    new() { Name = IntervalSizeName.Sixth, Quality = IntervalQualityName.Major, PitchOffset = 5, SemitoneOffset = 9 },
                    new() { Name = IntervalSizeName.Seventh, Quality = IntervalQualityName.Minor, PitchOffset = 6, SemitoneOffset = 10 }
                ]
            },
            new()
            {
                Metadata = new ScaleMetadata
                {
                    Names = ["Pentatonic Major"],
                    Description = "A five-note scale derived from the major scale, widely used in folk and popular music."
                },
                Intervals =
                [
                    new() { Name = IntervalSizeName.Unison, Quality = IntervalQualityName.Perfect, PitchOffset = 0, SemitoneOffset = 0 },
                    new() { Name = IntervalSizeName.Second, Quality = IntervalQualityName.Major, PitchOffset = 1, SemitoneOffset = 2 },
                    new() { Name = IntervalSizeName.Third, Quality = IntervalQualityName.Major, PitchOffset = 2, SemitoneOffset = 4 },
                    new() { Name = IntervalSizeName.Fifth, Quality = IntervalQualityName.Perfect, PitchOffset = 4, SemitoneOffset = 7 },
                    new() { Name = IntervalSizeName.Sixth, Quality = IntervalQualityName.Major, PitchOffset = 5, SemitoneOffset = 9 }
                ]
            },
            new()
            {
                Metadata = new ScaleMetadata
                {
                    Names = ["Pentatonic Minor", "Blues Scale"],
                    Description = "A five-note scale derived from the natural minor scale, fundamental to blues and rock."
                },
                Intervals =
                [
                    new() { Name = IntervalSizeName.Unison, Quality = IntervalQualityName.Perfect, PitchOffset = 0, SemitoneOffset = 0 },
                    new() { Name = IntervalSizeName.Third, Quality = IntervalQualityName.Minor, PitchOffset = 2, SemitoneOffset = 3 },
                    new() { Name = IntervalSizeName.Fourth, Quality = IntervalQualityName.Perfect, PitchOffset = 3, SemitoneOffset = 5 },
                    new() { Name = IntervalSizeName.Fifth, Quality = IntervalQualityName.Perfect, PitchOffset = 4, SemitoneOffset = 7 },
                    new() { Name = IntervalSizeName.Seventh, Quality = IntervalQualityName.Minor, PitchOffset = 6, SemitoneOffset = 10 }
                ]
            }
        ];
    }
}
