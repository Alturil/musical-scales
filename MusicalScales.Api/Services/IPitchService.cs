using MusicalScales.Api.Models;

namespace MusicalScales.Api.Services;

/// <summary>
/// Service interface for pitch-related operations
/// </summary>
public interface IPitchService
{
    /// <summary>
    /// Gets a pitch by applying an interval to a starting pitch
    /// </summary>
    Pitch GetPitch(Pitch startingPitch, Interval interval);

    /// <summary>
    /// Gets the interval between two pitches
    /// </summary>
    Interval GetInterval(Pitch fromPitch, Pitch toPitch);

    /// <summary>
    /// Transposes a pitch by a given number of semitones
    /// </summary>
    Pitch TransposePitch(Pitch pitch, int semitones);
}