using FluentAssertions;
using MusicalScales.Api.Models;
using MusicalScales.Api.Models.Enums;
using MusicalScales.Api.Services;

namespace MusicalScales.Tests.Services;

public class IntervalServiceTests
{
    private readonly IntervalService _intervalService;

    public IntervalServiceTests()
    {
        _intervalService = new IntervalService();
    }

    [Theory]
    [InlineData(2, 1, IntervalSizeName.Second, IntervalQualityName.Major, 2, 1)] // Major second
    [InlineData(1, 1, IntervalSizeName.Second, IntervalQualityName.Minor, 1, 1)] // Minor second
    [InlineData(4, 2, IntervalSizeName.Third, IntervalQualityName.Major, 4, 2)] // Major third
    [InlineData(3, 2, IntervalSizeName.Third, IntervalQualityName.Minor, 3, 2)] // Minor third
    [InlineData(7, 4, IntervalSizeName.Fifth, IntervalQualityName.Perfect, 7, 4)] // Perfect fifth
    [InlineData(5, 3, IntervalSizeName.Fourth, IntervalQualityName.Perfect, 5, 3)] // Perfect fourth
    [InlineData(12, 7, IntervalSizeName.Unison, IntervalQualityName.Perfect, 12, 7)] // Octave - preserves original offsets
    public void CreateInterval_WithValidInputs_ReturnsCorrectInterval(
        int semitoneOffset, int pitchOffset,
        IntervalSizeName expectedSize, IntervalQualityName expectedQuality,
        int expectedSemitoneOffset, int expectedPitchOffset)
    {
        // Act
        var result = _intervalService.CreateInterval(semitoneOffset, pitchOffset);

        // Assert
        result.Name.Should().Be(expectedSize);
        result.Quality.Should().Be(expectedQuality);
        result.SemitoneOffset.Should().Be(expectedSemitoneOffset);
        result.PitchOffset.Should().Be(expectedPitchOffset);
    }

    [Theory]
    [InlineData(-2, -1, IntervalSizeName.Seventh, IntervalQualityName.Minor, -2, -1)] // Negative values preserve original but calculate based on normalized
    [InlineData(-1, -1, IntervalSizeName.Seventh, IntervalQualityName.Major, -1, -1)]
    [InlineData(-5, -3, IntervalSizeName.Fifth, IntervalQualityName.Perfect, -5, -3)]
    public void CreateInterval_WithNegativeInputs_PreservesOriginalValues(
        int semitoneOffset, int pitchOffset,
        IntervalSizeName expectedSize, IntervalQualityName expectedQuality, 
        int expectedSemitoneOffset, int expectedPitchOffset)
    {
        // Act
        var result = _intervalService.CreateInterval(semitoneOffset, pitchOffset);

        // Assert
        result.Name.Should().Be(expectedSize);
        result.Quality.Should().Be(expectedQuality);
        result.SemitoneOffset.Should().Be(expectedSemitoneOffset);
        result.PitchOffset.Should().Be(expectedPitchOffset);
    }

    [Theory]
    [InlineData(IntervalSizeName.Second, IntervalQualityName.Major, 1, 2, IntervalSizeName.Seventh, IntervalQualityName.Minor)]
    [InlineData(IntervalSizeName.Third, IntervalQualityName.Major, 2, 4, IntervalSizeName.Sixth, IntervalQualityName.Minor)]
    [InlineData(IntervalSizeName.Fourth, IntervalQualityName.Perfect, 3, 5, IntervalSizeName.Fifth, IntervalQualityName.Perfect)]
    [InlineData(IntervalSizeName.Fifth, IntervalQualityName.Perfect, 4, 7, IntervalSizeName.Fourth, IntervalQualityName.Perfect)]
    [InlineData(IntervalSizeName.Unison, IntervalQualityName.Perfect, 0, 0, IntervalSizeName.Unison, IntervalQualityName.Perfect)]
    public void GetInverse_WithValidInterval_ReturnsCorrectInverse(
        IntervalSizeName intervalSize, IntervalQualityName intervalQuality, int pitchOffset, int semitoneOffset,
        IntervalSizeName expectedSize, IntervalQualityName expectedQuality)
    {
        // Arrange
        var interval = new Interval
        {
            Name = intervalSize,
            Quality = intervalQuality,
            PitchOffset = pitchOffset,
            SemitoneOffset = semitoneOffset
        };

        // Act
        var result = _intervalService.GetInverse(interval);

        // Assert
        result.Name.Should().Be(expectedSize);
        result.Quality.Should().Be(expectedQuality);
    }

    [Fact]
    public void GetInverse_WithOctaveInterval_ReturnsUnison()
    {
        // Arrange
        var octaveInterval = new Interval
        {
            Name = IntervalSizeName.Octave,
            Quality = IntervalQualityName.Perfect,
            PitchOffset = 7,
            SemitoneOffset = 12
        };

        // Act
        var result = _intervalService.GetInverse(octaveInterval);

        // Assert
        result.Name.Should().Be(IntervalSizeName.Unison);
        result.Quality.Should().Be(IntervalQualityName.Perfect);
        result.PitchOffset.Should().Be(0);
        result.SemitoneOffset.Should().Be(0);
    }

    [Theory]
    [InlineData(
        IntervalSizeName.Second, IntervalQualityName.Major, 1, 2,
        IntervalSizeName.Third, IntervalQualityName.Major, 2, 4,
        IntervalSizeName.Fourth, IntervalQualityName.Augmented, 3, 6)] // Major second + major third = augmented fourth (6 semitones)
    [InlineData(
        IntervalSizeName.Third, IntervalQualityName.Minor, 2, 3,
        IntervalSizeName.Third, IntervalQualityName.Minor, 2, 3,
        IntervalSizeName.Fifth, IntervalQualityName.Diminished, 4, 6)] // Minor third + minor third = diminished fifth
    [InlineData(
        IntervalSizeName.Fourth, IntervalQualityName.Perfect, 3, 5,
        IntervalSizeName.Fourth, IntervalQualityName.Perfect, 3, 5,
        IntervalSizeName.Seventh, IntervalQualityName.Minor, 6, 10)] // Perfect fourth + perfect fourth = minor seventh (6 pitches = 7th)
    public void AddIntervals_WithValidIntervals_ReturnsCorrectSum(
        IntervalSizeName interval1Size, IntervalQualityName interval1Quality, int interval1PitchOffset, int interval1SemitoneOffset,
        IntervalSizeName interval2Size, IntervalQualityName interval2Quality, int interval2PitchOffset, int interval2SemitoneOffset,
        IntervalSizeName expectedSize, IntervalQualityName expectedQuality, int expectedPitchOffset, int expectedSemitoneOffset)
    {
        // Arrange
        var interval1 = new Interval
        {
            Name = interval1Size,
            Quality = interval1Quality,
            PitchOffset = interval1PitchOffset,
            SemitoneOffset = interval1SemitoneOffset
        };

        var interval2 = new Interval
        {
            Name = interval2Size,
            Quality = interval2Quality,
            PitchOffset = interval2PitchOffset,
            SemitoneOffset = interval2SemitoneOffset
        };

        // Act
        var result = _intervalService.AddIntervals(interval1, interval2);

        // Assert
        result.Name.Should().Be(expectedSize);
        result.Quality.Should().Be(expectedQuality);
        result.PitchOffset.Should().Be(expectedPitchOffset);
        result.SemitoneOffset.Should().Be(expectedSemitoneOffset);
    }

    [Theory]
    [InlineData(IntervalSizeName.Unison, 0, IntervalQualityName.Perfect)]
    [InlineData(IntervalSizeName.Unison, 1, IntervalQualityName.Augmented)]
    [InlineData(IntervalSizeName.Second, 1, IntervalQualityName.Minor)]
    [InlineData(IntervalSizeName.Second, 2, IntervalQualityName.Major)]
    [InlineData(IntervalSizeName.Second, 3, IntervalQualityName.Augmented)]
    [InlineData(IntervalSizeName.Third, 3, IntervalQualityName.Minor)]
    [InlineData(IntervalSizeName.Third, 4, IntervalQualityName.Major)]
    [InlineData(IntervalSizeName.Third, 5, IntervalQualityName.Augmented)]
    [InlineData(IntervalSizeName.Fourth, 5, IntervalQualityName.Perfect)]
    [InlineData(IntervalSizeName.Fourth, 6, IntervalQualityName.Augmented)]
    [InlineData(IntervalSizeName.Fifth, 7, IntervalQualityName.Perfect)]
    [InlineData(IntervalSizeName.Fifth, 8, IntervalQualityName.Augmented)]
    [InlineData(IntervalSizeName.Sixth, 8, IntervalQualityName.Minor)]
    [InlineData(IntervalSizeName.Sixth, 9, IntervalQualityName.Major)]
    [InlineData(IntervalSizeName.Sixth, 10, IntervalQualityName.Augmented)]
    [InlineData(IntervalSizeName.Seventh, 10, IntervalQualityName.Minor)]
    [InlineData(IntervalSizeName.Seventh, 11, IntervalQualityName.Major)]
    [InlineData(IntervalSizeName.Seventh, 0, IntervalQualityName.Augmented)] // Augmented seventh wraps to 0 semitones
    public void CreateInterval_CorrectlyDeterminesIntervalQuality(IntervalSizeName intervalSize, int semitones, IntervalQualityName expectedQuality)
    {
        // Arrange
        int pitchOffset = (int)intervalSize;

        // Act
        var result = _intervalService.CreateInterval(semitones, pitchOffset);

        // Assert
        result.Quality.Should().Be(expectedQuality);
        result.Name.Should().Be(intervalSize);
    }

    [Theory]
    [InlineData(13, 8, IntervalSizeName.Second, IntervalQualityName.Minor)] // 13 semitones = 1 semitone (normalized), 8 pitches = 1 pitch (normalized), but 1 semitone on a second = minor
    [InlineData(24, 14, IntervalSizeName.Unison, IntervalQualityName.Perfect)] // Two octaves
    [InlineData(15, 9, IntervalSizeName.Third, IntervalQualityName.Minor)] // 15 semitones = 3 semitones (normalized), 9 pitches = 2 pitches (normalized)
    public void CreateInterval_WithLargeOffsets_NormalizesWithinOctave(
        int semitoneOffset, int pitchOffset,
        IntervalSizeName expectedSize, IntervalQualityName expectedQuality)
    {
        // Act
        var result = _intervalService.CreateInterval(semitoneOffset, pitchOffset);

        // Assert
        result.Name.Should().Be(expectedSize);
        result.Quality.Should().Be(expectedQuality);
        result.SemitoneOffset.Should().Be(semitoneOffset); // Original values preserved
        result.PitchOffset.Should().Be(pitchOffset);
    }

    [Fact]
    public void AddIntervals_CreatesLargerInterval()
    {
        // Arrange - Adding a major second and a major third should give a perfect fourth
        var majorSecond = new Interval
        {
            Name = IntervalSizeName.Second,
            Quality = IntervalQualityName.Major,
            PitchOffset = 1,
            SemitoneOffset = 2
        };

        var majorThird = new Interval
        {
            Name = IntervalSizeName.Third,
            Quality = IntervalQualityName.Major,
            PitchOffset = 2,
            SemitoneOffset = 4
        };

        // Act
        var result = _intervalService.AddIntervals(majorSecond, majorThird);

        // Assert
        result.PitchOffset.Should().Be(3); // 1 + 2
        result.SemitoneOffset.Should().Be(6); // 2 + 4
        result.Name.Should().Be(IntervalSizeName.Fourth);
        result.Quality.Should().Be(IntervalQualityName.Augmented); // 6 semitones for a fourth is augmented
    }

    [Theory]
    [InlineData(IntervalSizeName.Unison, 11, IntervalQualityName.Diminished)] // Diminished unison
    [InlineData(IntervalSizeName.Second, 0, IntervalQualityName.Diminished)] // Diminished second
    [InlineData(IntervalSizeName.Fourth, 4, IntervalQualityName.Diminished)] // Diminished fourth
    [InlineData(IntervalSizeName.Fifth, 6, IntervalQualityName.Diminished)] // Diminished fifth
    public void CreateInterval_WithDiminishedQuality_ReturnsCorrectInterval(
        IntervalSizeName intervalSize, int semitones, IntervalQualityName expectedQuality)
    {
        // Arrange
        int pitchOffset = (int)intervalSize;

        // Act
        var result = _intervalService.CreateInterval(semitones, pitchOffset);

        // Assert
        result.Quality.Should().Be(expectedQuality);
        result.Name.Should().Be(intervalSize);
    }
}