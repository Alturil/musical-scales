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