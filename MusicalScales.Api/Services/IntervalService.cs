using MusicalScales.Api.Models;
using MusicalScales.Api.Models.Enums;

namespace MusicalScales.Api.Services;

/// <summary>
/// Service implementation for interval-related operations
/// </summary>
public class IntervalService : IIntervalService
{
    /// <inheritdoc />
    public Interval CreateInterval(int semitoneOffset, int pitchOffset)
    {
        // Normalize offsets to be within an octave
        var normalizedSemitones = semitoneOffset % 12;
        var normalizedPitches = pitchOffset % 7;

        if (normalizedSemitones < 0) normalizedSemitones += 12;
        if (normalizedPitches < 0) normalizedPitches += 7;

        var intervalSize = (IntervalSizeName)normalizedPitches;
        var quality = DetermineIntervalQuality(intervalSize, normalizedSemitones);

        return new Interval
        {
            Name = intervalSize,
            Quality = quality,
            PitchOffset = pitchOffset,
            SemitoneOffset = semitoneOffset
        };
    }

    /// <inheritdoc />
    public Interval GetInverse(Interval interval)
    {
        var inversePitchOffset = 7 - interval.PitchOffset;
        var inverseSemitoneOffset = 12 - interval.SemitoneOffset;

        // Handle octave intervals
        if (interval.PitchOffset == 7 || interval.SemitoneOffset == 12)
        {
            inversePitchOffset = 0;
            inverseSemitoneOffset = 0;
        }

        return CreateInterval(inverseSemitoneOffset, inversePitchOffset);
    }

    /// <inheritdoc />
    public Interval AddIntervals(Interval interval1, Interval interval2)
    {
        var combinedPitchOffset = interval1.PitchOffset + interval2.PitchOffset;
        var combinedSemitoneOffset = interval1.SemitoneOffset + interval2.SemitoneOffset;

        return CreateInterval(combinedSemitoneOffset, combinedPitchOffset);
    }

    /// <inheritdoc />
    public int GetSemitoneOffset(IntervalSizeName name, IntervalQualityName quality)
    {
        return name switch
        {
            IntervalSizeName.Unison => quality switch
            {
                IntervalQualityName.Perfect => 0,
                IntervalQualityName.Augmented => 1,
                IntervalQualityName.Diminished => -1,
                _ => throw new ArgumentException($"Invalid quality '{quality}' for Unison interval")
            },
            IntervalSizeName.Second => quality switch
            {
                IntervalQualityName.Minor => 1,
                IntervalQualityName.Major => 2,
                IntervalQualityName.Augmented => 3,
                IntervalQualityName.Diminished => 0,
                _ => throw new ArgumentException($"Invalid quality '{quality}' for Second interval")
            },
            IntervalSizeName.Third => quality switch
            {
                IntervalQualityName.Minor => 3,
                IntervalQualityName.Major => 4,
                IntervalQualityName.Augmented => 5,
                IntervalQualityName.Diminished => 2,
                _ => throw new ArgumentException($"Invalid quality '{quality}' for Third interval")
            },
            IntervalSizeName.Fourth => quality switch
            {
                IntervalQualityName.Perfect => 5,
                IntervalQualityName.Augmented => 6,
                IntervalQualityName.Diminished => 4,
                _ => throw new ArgumentException($"Invalid quality '{quality}' for Fourth interval")
            },
            IntervalSizeName.Fifth => quality switch
            {
                IntervalQualityName.Perfect => 7,
                IntervalQualityName.Augmented => 8,
                IntervalQualityName.Diminished => 6,
                _ => throw new ArgumentException($"Invalid quality '{quality}' for Fifth interval")
            },
            IntervalSizeName.Sixth => quality switch
            {
                IntervalQualityName.Minor => 8,
                IntervalQualityName.Major => 9,
                IntervalQualityName.Augmented => 10,
                IntervalQualityName.Diminished => 7,
                _ => throw new ArgumentException($"Invalid quality '{quality}' for Sixth interval")
            },
            IntervalSizeName.Seventh => quality switch
            {
                IntervalQualityName.Minor => 10,
                IntervalQualityName.Major => 11,
                IntervalQualityName.Augmented => 12,
                IntervalQualityName.Diminished => 9,
                _ => throw new ArgumentException($"Invalid quality '{quality}' for Seventh interval")
            },
            IntervalSizeName.Octave => quality switch
            {
                IntervalQualityName.Perfect => 12,
                IntervalQualityName.Augmented => 13,
                IntervalQualityName.Diminished => 11,
                _ => throw new ArgumentException($"Invalid quality '{quality}' for Octave interval")
            },
            _ => throw new ArgumentException($"Invalid interval size '{name}'")
        };
    }

    /// <inheritdoc />
    public int GetPitchOffset(IntervalSizeName name)
    {
        return name switch
        {
            IntervalSizeName.Unison => 0,
            IntervalSizeName.Second => 1,
            IntervalSizeName.Third => 2,
            IntervalSizeName.Fourth => 3,
            IntervalSizeName.Fifth => 4,
            IntervalSizeName.Sixth => 5,
            IntervalSizeName.Seventh => 6,
            IntervalSizeName.Octave => 7,
            _ => throw new ArgumentException($"Invalid interval size '{name}'")
        };
    }

    /// <inheritdoc />
    public void PopulateIntervalOffsets(Interval interval)
    {
        interval.PitchOffset = GetPitchOffset(interval.Name);
        interval.SemitoneOffset = GetSemitoneOffset(interval.Name, interval.Quality);
    }

    private static IntervalQualityName DetermineIntervalQuality(IntervalSizeName size, int semitones)
    {
        return size switch
        {
            IntervalSizeName.Unison => semitones switch
            {
                0 => IntervalQualityName.Perfect,
                1 => IntervalQualityName.Augmented,
                _ => IntervalQualityName.Diminished
            },
            IntervalSizeName.Second => semitones switch
            {
                1 => IntervalQualityName.Minor,
                2 => IntervalQualityName.Major,
                3 => IntervalQualityName.Augmented,
                _ => IntervalQualityName.Diminished
            },
            IntervalSizeName.Third => semitones switch
            {
                3 => IntervalQualityName.Minor,
                4 => IntervalQualityName.Major,
                5 => IntervalQualityName.Augmented,
                _ => IntervalQualityName.Diminished
            },
            IntervalSizeName.Fourth => semitones switch
            {
                5 => IntervalQualityName.Perfect,
                6 => IntervalQualityName.Augmented,
                _ => IntervalQualityName.Diminished
            },
            IntervalSizeName.Fifth => semitones switch
            {
                7 => IntervalQualityName.Perfect,
                8 => IntervalQualityName.Augmented,
                _ => IntervalQualityName.Diminished
            },
            IntervalSizeName.Sixth => semitones switch
            {
                8 => IntervalQualityName.Minor,
                9 => IntervalQualityName.Major,
                10 => IntervalQualityName.Augmented,
                _ => IntervalQualityName.Diminished
            },
            IntervalSizeName.Seventh => semitones switch
            {
                10 => IntervalQualityName.Minor,
                11 => IntervalQualityName.Major,
                0 => IntervalQualityName.Augmented,
                _ => IntervalQualityName.Diminished
            },
            IntervalSizeName.Octave => semitones switch
            {
                0 => IntervalQualityName.Perfect,
                1 => IntervalQualityName.Augmented,
                _ => IntervalQualityName.Diminished
            },
            _ => IntervalQualityName.Perfect
        };
    }
}