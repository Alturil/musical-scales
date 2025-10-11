using FluentAssertions;
using MusicalScales.Api.Models;
using MusicalScales.Api.Models.Enums;
using MusicalScales.Api.Services;

namespace MusicalScales.Tests.Services;

public class PitchServiceTests
{
    private readonly PitchService _pitchService;

    public PitchServiceTests()
    {
        _pitchService = new PitchService();
    }

    [Theory]
    [InlineData(DiatonicPitchName.C, AccidentalName.Natural, 0, 0, IntervalSizeName.Second, IntervalQualityName.Major, 1, 2, DiatonicPitchName.D, AccidentalName.Natural)]
    [InlineData(DiatonicPitchName.C, AccidentalName.Natural, 0, 0, IntervalSizeName.Third, IntervalQualityName.Major, 2, 4, DiatonicPitchName.E, AccidentalName.Natural)]
    [InlineData(DiatonicPitchName.C, AccidentalName.Natural, 0, 0, IntervalSizeName.Fifth, IntervalQualityName.Perfect, 4, 7, DiatonicPitchName.G, AccidentalName.Natural)]
    [InlineData(DiatonicPitchName.G, AccidentalName.Natural, 4, 7, IntervalSizeName.Second, IntervalQualityName.Major, 1, 2, DiatonicPitchName.A, AccidentalName.Natural)]
    [InlineData(DiatonicPitchName.F, AccidentalName.Natural, 3, 5, IntervalSizeName.Third, IntervalQualityName.Major, 2, 4, DiatonicPitchName.A, AccidentalName.Natural)]
    public void GetPitch_WithValidInputs_ReturnsCorrectPitch(
        DiatonicPitchName startPitchName, AccidentalName startAccidental, int startPitchOffset, int startSemitoneOffset,
        IntervalSizeName intervalSize, IntervalQualityName intervalQuality, int intervalPitchOffset, int intervalSemitoneOffset,
        DiatonicPitchName expectedPitchName, AccidentalName expectedAccidental)
    {
        // Arrange
        var startingPitch = new Pitch
        {
            Name = startPitchName,
            Accidental = startAccidental,
            PitchOffset = startPitchOffset,
            SemitoneOffset = startSemitoneOffset
        };

        var interval = new Interval
        {
            Name = intervalSize,
            Quality = intervalQuality,
            PitchOffset = intervalPitchOffset,
            SemitoneOffset = intervalSemitoneOffset
        };

        // Act
        var result = _pitchService.GetPitch(startingPitch, interval);

        // Assert
        result.Name.Should().Be(expectedPitchName);
        result.Accidental.Should().Be(expectedAccidental);
        result.PitchOffset.Should().Be(startPitchOffset + intervalPitchOffset);
        result.SemitoneOffset.Should().Be(startSemitoneOffset + intervalSemitoneOffset);
    }

    [Theory]
    [InlineData(DiatonicPitchName.C, AccidentalName.Natural, 0, 0, IntervalSizeName.Second, IntervalQualityName.Minor, 1, 1, DiatonicPitchName.D, AccidentalName.Flat)]
    [InlineData(DiatonicPitchName.C, AccidentalName.Natural, 0, 0, IntervalSizeName.Second, IntervalQualityName.Augmented, 1, 3, DiatonicPitchName.D, AccidentalName.Sharp)]
    [InlineData(DiatonicPitchName.F, AccidentalName.Natural, 3, 5, IntervalSizeName.Fourth, IntervalQualityName.Augmented, 3, 6, DiatonicPitchName.B, AccidentalName.Natural)]
    public void GetPitch_WithAccidentals_ReturnsCorrectPitchWithAccidental(
        DiatonicPitchName startPitchName, AccidentalName startAccidental, int startPitchOffset, int startSemitoneOffset,
        IntervalSizeName intervalSize, IntervalQualityName intervalQuality, int intervalPitchOffset, int intervalSemitoneOffset,
        DiatonicPitchName expectedPitchName, AccidentalName expectedAccidental)
    {
        // Arrange
        var startingPitch = new Pitch
        {
            Name = startPitchName,
            Accidental = startAccidental,
            PitchOffset = startPitchOffset,
            SemitoneOffset = startSemitoneOffset
        };

        var interval = new Interval
        {
            Name = intervalSize,
            Quality = intervalQuality,
            PitchOffset = intervalPitchOffset,
            SemitoneOffset = intervalSemitoneOffset
        };

        // Act
        var result = _pitchService.GetPitch(startingPitch, interval);

        // Assert
        result.Name.Should().Be(expectedPitchName);
        result.Accidental.Should().Be(expectedAccidental);
    }

    [Theory]
    [InlineData(DiatonicPitchName.C, AccidentalName.Natural, 0, 0, DiatonicPitchName.D, AccidentalName.Natural, 1, 2, IntervalSizeName.Second, IntervalQualityName.Major)]
    [InlineData(DiatonicPitchName.C, AccidentalName.Natural, 0, 0, DiatonicPitchName.E, AccidentalName.Natural, 2, 4, IntervalSizeName.Third, IntervalQualityName.Major)]
    [InlineData(DiatonicPitchName.C, AccidentalName.Natural, 0, 0, DiatonicPitchName.G, AccidentalName.Natural, 4, 7, IntervalSizeName.Fifth, IntervalQualityName.Perfect)]
    [InlineData(DiatonicPitchName.D, AccidentalName.Natural, 1, 2, DiatonicPitchName.F, AccidentalName.Natural, 3, 5, IntervalSizeName.Third, IntervalQualityName.Minor)]
    [InlineData(DiatonicPitchName.C, AccidentalName.Natural, 0, 0, DiatonicPitchName.D, AccidentalName.Flat, 1, 1, IntervalSizeName.Second, IntervalQualityName.Minor)]
    public void GetInterval_WithValidPitches_ReturnsCorrectInterval(
        DiatonicPitchName fromPitchName, AccidentalName fromAccidental, int fromPitchOffset, int fromSemitoneOffset,
        DiatonicPitchName toPitchName, AccidentalName toAccidental, int toPitchOffset, int toSemitoneOffset,
        IntervalSizeName expectedSize, IntervalQualityName expectedQuality)
    {
        // Arrange
        var fromPitch = new Pitch
        {
            Name = fromPitchName,
            Accidental = fromAccidental,
            PitchOffset = fromPitchOffset,
            SemitoneOffset = fromSemitoneOffset
        };

        var toPitch = new Pitch
        {
            Name = toPitchName,
            Accidental = toAccidental,
            PitchOffset = toPitchOffset,
            SemitoneOffset = toSemitoneOffset
        };

        // Act
        var result = _pitchService.GetInterval(fromPitch, toPitch);

        // Assert
        result.Name.Should().Be(expectedSize);
        result.Quality.Should().Be(expectedQuality);
        result.PitchOffset.Should().Be(toPitchOffset - fromPitchOffset);
        result.SemitoneOffset.Should().Be(toSemitoneOffset - fromSemitoneOffset);
    }

    [Fact]
    public void GetInterval_WithNegativeOffsets_NormalizesToPositive()
    {
        // Arrange - Going from D to C (descending)
        var fromPitch = new Pitch
        {
            Name = DiatonicPitchName.D,
            Accidental = AccidentalName.Natural,
            PitchOffset = 8, // D in second octave
            SemitoneOffset = 14 // D in second octave
        };

        var toPitch = new Pitch
        {
            Name = DiatonicPitchName.C,
            Accidental = AccidentalName.Natural,
            PitchOffset = 0,
            SemitoneOffset = 0
        };

        // Act
        var result = _pitchService.GetInterval(fromPitch, toPitch);

        // Assert - Should normalize to positive values representing the interval from C to D
        result.Name.Should().Be(IntervalSizeName.Seventh); // The inversion of a second is a seventh
        result.Quality.Should().Be(IntervalQualityName.Minor); // The inversion of a major second is a minor seventh
    }

    [Theory]
    [InlineData(DiatonicPitchName.C, AccidentalName.Natural, 0, 0, 2, DiatonicPitchName.C, AccidentalName.Natural, 0, 2)]
    [InlineData(DiatonicPitchName.G, AccidentalName.Sharp, 4, 8, -1, DiatonicPitchName.G, AccidentalName.Sharp, 4, 7)]
    [InlineData(DiatonicPitchName.F, AccidentalName.Flat, 3, 4, 12, DiatonicPitchName.F, AccidentalName.Flat, 3, 16)]
    public void TransposePitch_WithValidInputs_ReturnsTransposedPitch(
        DiatonicPitchName pitchName, AccidentalName accidental, int pitchOffset, int semitoneOffset,
        int transposeSemitones,
        DiatonicPitchName expectedPitchName, AccidentalName expectedAccidental, int expectedPitchOffset, int expectedSemitoneOffset)
    {
        // Arrange
        var pitch = new Pitch
        {
            Name = pitchName,
            Accidental = accidental,
            PitchOffset = pitchOffset,
            SemitoneOffset = semitoneOffset
        };

        // Act
        var result = _pitchService.TransposePitch(pitch, transposeSemitones);

        // Assert
        result.Name.Should().Be(expectedPitchName);
        result.Accidental.Should().Be(expectedAccidental);
        result.PitchOffset.Should().Be(expectedPitchOffset);
        result.SemitoneOffset.Should().Be(expectedSemitoneOffset);
    }

    [Theory]
    [InlineData(DiatonicPitchName.C)]
    [InlineData(DiatonicPitchName.D)]
    [InlineData(DiatonicPitchName.E)]
    [InlineData(DiatonicPitchName.F)]
    [InlineData(DiatonicPitchName.G)]
    [InlineData(DiatonicPitchName.A)]
    [InlineData(DiatonicPitchName.B)]
    public void GetPitch_WithOctaveInterval_WrapsAroundCorrectly(DiatonicPitchName startingPitchName)
    {
        // Arrange
        var startingPitch = new Pitch
        {
            Name = startingPitchName,
            Accidental = AccidentalName.Natural,
            PitchOffset = (int)startingPitchName,
            SemitoneOffset = 0
        };

        var octaveInterval = new Interval
        {
            Name = IntervalSizeName.Octave,
            Quality = IntervalQualityName.Perfect,
            PitchOffset = 7,
            SemitoneOffset = 12
        };

        // Act
        var result = _pitchService.GetPitch(startingPitch, octaveInterval);

        // Assert
        result.Name.Should().Be(startingPitchName); // Same pitch name, different octave
        result.PitchOffset.Should().Be((int)startingPitchName + 7);
        result.SemitoneOffset.Should().Be(12);
    }

    [Theory]
    [InlineData(IntervalSizeName.Unison, 0, IntervalQualityName.Perfect)]
    [InlineData(IntervalSizeName.Unison, 1, IntervalQualityName.Augmented)]
    [InlineData(IntervalSizeName.Second, 1, IntervalQualityName.Minor)]
    [InlineData(IntervalSizeName.Second, 2, IntervalQualityName.Major)]
    [InlineData(IntervalSizeName.Second, 3, IntervalQualityName.Augmented)]
    [InlineData(IntervalSizeName.Third, 3, IntervalQualityName.Minor)]
    [InlineData(IntervalSizeName.Third, 4, IntervalQualityName.Major)]
    [InlineData(IntervalSizeName.Fourth, 5, IntervalQualityName.Perfect)]
    [InlineData(IntervalSizeName.Fourth, 6, IntervalQualityName.Augmented)]
    [InlineData(IntervalSizeName.Fifth, 7, IntervalQualityName.Perfect)]
    [InlineData(IntervalSizeName.Sixth, 8, IntervalQualityName.Minor)]
    [InlineData(IntervalSizeName.Sixth, 9, IntervalQualityName.Major)]
    [InlineData(IntervalSizeName.Seventh, 10, IntervalQualityName.Minor)]
    [InlineData(IntervalSizeName.Seventh, 11, IntervalQualityName.Major)]
    [InlineData(IntervalSizeName.Octave, 0, IntervalQualityName.Perfect)] // Octave with 0 semitones (modulo 12)
    public void GetInterval_CorrectlyDeterminesIntervalQuality(IntervalSizeName intervalSize, int semitones, IntervalQualityName expectedQuality)
    {
        // Arrange - Create two pitches that form the specified interval
        var fromPitch = new Pitch
        {
            Name = DiatonicPitchName.C,
            Accidental = AccidentalName.Natural,
            PitchOffset = 0,
            SemitoneOffset = 0
        };

        var toPitch = new Pitch
        {
            Name = DiatonicPitchName.C, // This will be adjusted based on the interval
            Accidental = AccidentalName.Natural,
            PitchOffset = (int)intervalSize,
            SemitoneOffset = semitones
        };

        // Act
        var result = _pitchService.GetInterval(fromPitch, toPitch);

        // Assert
        result.Quality.Should().Be(expectedQuality);
    }
}