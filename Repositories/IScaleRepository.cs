using MusicalScales.Api.Models;

namespace MusicalScales.Api.Repositories;

/// <summary>
/// Repository interface for Scale operations
/// </summary>
public interface IScaleRepository
{
    /// <summary>
    /// Gets all scales
    /// </summary>
    Task<IEnumerable<Scale>> GetAllScalesAsync();
    
    /// <summary>
    /// Gets a scale by its unique identifier
    /// </summary>
    Task<Scale?> GetScaleByIdAsync(Guid id);
    
    /// <summary>
    /// Gets scales by name (partial match)
    /// </summary>
    Task<IEnumerable<Scale>> GetScalesByNameAsync(string name);
    
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
    Task<Scale?> UpdateScaleAsync(Guid id, Scale scale);
    
    /// <summary>
    /// Deletes a scale by its identifier
    /// </summary>
    Task<bool> DeleteScaleAsync(Guid id);
    
    /// <summary>
    /// Checks if a scale exists by its identifier
    /// </summary>
    Task<bool> ScaleExistsAsync(Guid id);
}