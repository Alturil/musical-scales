using MusicalScales.Api.Models;
using MusicalScales.Api.Models.Enums;

namespace MusicalScales.Api.Services;

/// <summary>
/// Service implementation for pitch-related operations
/// </summary>
public class PitchService : IPitchService
{
    private static readonly Dictionary<DiatonicPitchName, int> PitchSemitones = new()
    {
        { DiatonicPitchName.C, 0 },
        { DiatonicPitchName.D, 2 },
        { DiatonicPitchName.E, 4 },
        { DiatonicPitchName.F, 5 },
        { DiatonicPitchName.G, 7 },
        { DiatonicPitchName.A, 9 },
        { DiatonicPitchName.B, 11 }
    };
    
    private static readonly Dictionary<AccidentalName, int> AccidentalOffsets = new()
    {
        { AccidentalName.DoubleFlat, -2 },
        { AccidentalName.Flat, -1 },
        { AccidentalName.Natural, 0 },
        { AccidentalName.Sharp, 1 },
        { AccidentalName.DoubleSharp, 2 }
    };
    
    /// <inheritdoc />
    public Pitch GetPitch(Pitch startingPitch, Interval interval)
    {
        var newPitchName = GetDiatonicPitchName(startingPitch.Name, interval.PitchOffset);
        var expectedSemitones = (GetPitchSemitones(startingPitch) + interval.SemitoneOffset) % 12;
        var actualSemitones = PitchSemitones[newPitchName];
        var accidentalOffset = expectedSemitones - actualSemitones;
        
        // Handle wrap-around for negative values
        // TODO: this might be defensive coding, but not a real life scenario
        if (accidentalOffset < -6)
            accidentalOffset += 12;
        else if (accidentalOffset > 6)
            accidentalOffset -= 12;
            
        var accidental = GetAccidentalFromOffset(accidentalOffset);
        
        return new Pitch
        {
            Name = newPitchName,
            Accidental = accidental,
            PitchOffset = startingPitch.PitchOffset + interval.PitchOffset,
            SemitoneOffset = startingPitch.SemitoneOffset + interval.SemitoneOffset
        };
    }
    
    /// <inheritdoc />
    public Interval GetInterval(Pitch fromPitch, Pitch toPitch)
    {
        var pitchOffset = toPitch.PitchOffset - fromPitch.PitchOffset;
        var semitoneOffset = toPitch.SemitoneOffset - fromPitch.SemitoneOffset;
        
        // Normalize to positive values
        while (pitchOffset < 0) pitchOffset += 7;
        while (semitoneOffset < 0) semitoneOffset += 12;
        
        var intervalSize = (IntervalSizeName)(pitchOffset % 7);
        var quality = DetermineIntervalQuality(intervalSize, semitoneOffset % 12);
        
        return new Interval
        {
            Name = intervalSize,
            Quality = quality,
            PitchOffset = pitchOffset,
            SemitoneOffset = semitoneOffset
        };
    }
    
    /// <inheritdoc />
    public Pitch TransposePitch(Pitch pitch, int semitones)
    {
        var newSemitoneOffset = pitch.SemitoneOffset + semitones;
        
        return new Pitch
        {
            Name = pitch.Name,
            Accidental = pitch.Accidental,
            PitchOffset = pitch.PitchOffset,
            SemitoneOffset = newSemitoneOffset
        };
    }
    
    private static DiatonicPitchName GetDiatonicPitchName(DiatonicPitchName startingPitch, int offset)
    {
        var pitchIndex = (int)startingPitch;
        var newPitchIndex = (pitchIndex + offset) % 7;
        
        if (newPitchIndex < 0)
            newPitchIndex += 7;
            
        return (DiatonicPitchName)newPitchIndex;
    }
    
    private static int GetPitchSemitones(Pitch pitch)
    {
        return PitchSemitones[pitch.Name] + AccidentalOffsets[pitch.Accidental];
    }
    
    private static AccidentalName GetAccidentalFromOffset(int offset)
    {
        return offset switch
        {
            -2 => AccidentalName.DoubleFlat,
            -1 => AccidentalName.Flat,
            0 => AccidentalName.Natural,
            1 => AccidentalName.Sharp,
            2 => AccidentalName.DoubleSharp,
            _ => AccidentalName.Natural
        };
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