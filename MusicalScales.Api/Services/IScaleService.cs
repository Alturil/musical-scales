using MusicalScales.Api.Models;

namespace MusicalScales.Api.Services;

/// <summary>
/// Service interface for scale-related operations
/// </summary>
public interface IScaleService
{
    /// <summary>
    /// Gets all pitches in a scale starting from a given root pitch
    /// </summary>
    Task<IList<Pitch>> GetScalePitchesAsync(Pitch rootPitch, IList<Interval> scaleIntervals);

    /// <summary>
    /// Gets all scales
    /// </summary>
    Task<IEnumerable<Scale>> GetAllScalesAsync();

    /// <summary>
    /// Gets a scale by its unique identifier
    /// </summary>
    Task<Scale?> GetScaleByIdAsync(Guid scaleId);

    /// <summary>
    /// Gets scales by name (partial match)
    /// </summary>
    Task<IEnumerable<Scale>> GetScalesByNameAsync(string scaleName);

    /// <summary>
    /// Gets a scale by its interval structure
    /// </summary>
    Task<Scale?> GetScaleByIntervalsAsync(IList<Interval> intervals);

    /// <summary>
    /// Creates a new scale
    /// </summary>
    Task<Scale> CreateScaleAsync(Scale scale);

    /// <summary>
    /// Updates an existing scale
    /// </summary>
    Task<Scale?> UpdateScaleAsync(Guid scaleId, Scale scale);

    /// <summary>
    /// Deletes a scale by its identifier
    /// </summary>
    Task<bool> DeleteScaleAsync(Guid scaleId);
}