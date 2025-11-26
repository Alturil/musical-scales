using MusicalScales.Api.Models;

namespace MusicalScales.Api.Services;

/// <summary>
/// Service interface for interval-related operations
/// </summary>
public interface IIntervalService
{
    /// <summary>
    /// Creates an interval from semitone and pitch offsets
    /// </summary>
    Interval CreateInterval(int semitoneOffset, int pitchOffset);
    
    /// <summary>
    /// Gets the inverse of an interval
    /// </summary>
    Interval GetInverse(Interval interval);
    
    /// <summary>
    /// Adds two intervals together
    /// </summary>
    Interval AddIntervals(Interval interval1, Interval interval2);

    /// <summary>
    /// Calculates the semitone offset for a given interval name and quality
    /// </summary>
    int GetSemitoneOffset(Models.Enums.IntervalSizeName name, Models.Enums.IntervalQualityName quality);

    /// <summary>
    /// Calculates the pitch offset for a given interval name
    /// </summary>
    int GetPitchOffset(Models.Enums.IntervalSizeName name);

    /// <summary>
    /// Populates the offsets for an interval based on its name and quality
    /// </summary>
    void PopulateIntervalOffsets(Interval interval);
}