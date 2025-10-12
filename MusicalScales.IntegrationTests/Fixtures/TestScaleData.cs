using MusicalScales.Api.Models;
using MusicalScales.Api.Models.Enums;

namespace MusicalScales.IntegrationTests.Fixtures;

/// <summary>
/// Test data fixtures for integration tests with various scale types
/// </summary>
public static class TestScaleData
{
    /// <summary>
    /// Simple Test Scale (C D E)
    /// Basic three-note scale for testing
    /// </summary>
    public static Scale SimpleTestScale => new()
    {
        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Metadata = new ScaleMetadata
        {
            Names = new List<string> { "Simple Test Scale" },
            Description = "Basic three-note scale for integration testing"
        },
        Intervals = new List<Interval>
        {
            new() { Name = IntervalSizeName.Unison, Quality = IntervalQualityName.Perfect, PitchOffset = 0, SemitoneOffset = 0 },
            new() { Name = IntervalSizeName.Second, Quality = IntervalQualityName.Major, PitchOffset = 1, SemitoneOffset = 2 },
            new() { Name = IntervalSizeName.Third, Quality = IntervalQualityName.Major, PitchOffset = 2, SemitoneOffset = 4 }
        }
    };

    /// <summary>
    /// Another Test Scale (C E G)
    /// Basic triad for testing
    /// </summary>
    public static Scale AnotherTestScale => new()
    {
        Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
        Metadata = new ScaleMetadata
        {
            Names = new List<string> { "Another Test Scale" },
            Description = "Basic triad for integration testing"
        },
        Intervals = new List<Interval>
        {
            new() { Name = IntervalSizeName.Unison, Quality = IntervalQualityName.Perfect, PitchOffset = 0, SemitoneOffset = 0 },
            new() { Name = IntervalSizeName.Third, Quality = IntervalQualityName.Major, PitchOffset = 2, SemitoneOffset = 4 },
            new() { Name = IntervalSizeName.Fifth, Quality = IntervalQualityName.Perfect, PitchOffset = 4, SemitoneOffset = 7 }
        }
    };

    /// <summary>
    /// Third Test Scale (C D F G)
    /// Four-note scale for testing
    /// </summary>
    public static Scale ThirdTestScale => new()
    {
        Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
        Metadata = new ScaleMetadata
        {
            Names = new List<string> { "Third Test Scale" },
            Description = "Four-note scale for integration testing"
        },
        Intervals = new List<Interval>
        {
            new() { Name = IntervalSizeName.Unison, Quality = IntervalQualityName.Perfect, PitchOffset = 0, SemitoneOffset = 0 },
            new() { Name = IntervalSizeName.Second, Quality = IntervalQualityName.Major, PitchOffset = 1, SemitoneOffset = 2 },
            new() { Name = IntervalSizeName.Fourth, Quality = IntervalQualityName.Perfect, PitchOffset = 3, SemitoneOffset = 5 },
            new() { Name = IntervalSizeName.Fifth, Quality = IntervalQualityName.Perfect, PitchOffset = 4, SemitoneOffset = 7 }
        }
    };

    /// <summary>
    /// Fourth Test Scale (C D E F G)
    /// Five-note scale for testing
    /// </summary>
    public static Scale FourthTestScale => new()
    {
        Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
        Metadata = new ScaleMetadata
        {
            Names = new List<string> { "Fourth Test Scale" },
            Description = "Five-note scale for integration testing"
        },
        Intervals = new List<Interval>
        {
            new() { Name = IntervalSizeName.Unison, Quality = IntervalQualityName.Perfect, PitchOffset = 0, SemitoneOffset = 0 },
            new() { Name = IntervalSizeName.Second, Quality = IntervalQualityName.Major, PitchOffset = 1, SemitoneOffset = 2 },
            new() { Name = IntervalSizeName.Third, Quality = IntervalQualityName.Major, PitchOffset = 2, SemitoneOffset = 4 },
            new() { Name = IntervalSizeName.Fourth, Quality = IntervalQualityName.Perfect, PitchOffset = 3, SemitoneOffset = 5 },
            new() { Name = IntervalSizeName.Fifth, Quality = IntervalQualityName.Perfect, PitchOffset = 4, SemitoneOffset = 7 }
        }
    };

    /// <summary>
    /// Get all test scales for bulk operations
    /// </summary>
    public static List<Scale> AllTestScales => new()
    {
        SimpleTestScale,
        AnotherTestScale,
        ThirdTestScale,
        FourthTestScale
    };

    /// <summary>
    /// Create a test pitch (C natural)
    /// </summary>
    public static Pitch TestRootPitch => new()
    {
        Name = DiatonicPitchName.C,
        Accidental = AccidentalName.Natural,
        PitchOffset = 0,
        SemitoneOffset = 0
    };
}