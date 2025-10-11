using MusicalScales.Api.Models;
using MusicalScales.Api.Repositories;

namespace MusicalScales.Api.Services;

/// <summary>
/// Service implementation for scale-related operations
/// </summary>
public class ScaleService : IScaleService
{
    private readonly IScaleRepository _scaleRepository;
    private readonly IPitchService _pitchService;
    
    public ScaleService(IScaleRepository scaleRepository, IPitchService pitchService)
    {
        _scaleRepository = scaleRepository;
        _pitchService = pitchService;
    }
    
    /// <inheritdoc />
    public Task<IList<Pitch>> GetScalePitchesAsync(Pitch rootPitch, IList<Interval> scaleIntervals)
    {
        var scalePitches = new List<Pitch> { rootPitch };
        
        var currentPitch = rootPitch;
        foreach (var interval in scaleIntervals)
        {
            currentPitch = _pitchService.GetPitch(rootPitch, interval);
            scalePitches.Add(currentPitch);
        }
        
        return Task.FromResult<IList<Pitch>>(scalePitches);
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<Scale>> GetAllScalesAsync()
    {
        return await _scaleRepository.GetAllScalesAsync();
    }
    
    /// <inheritdoc />
    public async Task<Scale?> GetScaleByIdAsync(Guid scaleId)
    {
        return await _scaleRepository.GetScaleByIdAsync(scaleId);
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<Scale>> GetScalesByNameAsync(string scaleName)
    {
        return await _scaleRepository.GetScalesByNameAsync(scaleName);
    }
    
    /// <inheritdoc />
    public async Task<Scale?> GetScaleByIntervalsAsync(IList<Interval> intervals)
    {
        return await _scaleRepository.GetScaleByIntervalsAsync(intervals);
    }
    
    /// <inheritdoc />
    public async Task<Scale> CreateScaleAsync(Scale scale)
    {
        ValidateScale(scale);
        return await _scaleRepository.CreateScaleAsync(scale);
    }
    
    /// <inheritdoc />
    public async Task<Scale?> UpdateScaleAsync(Guid scaleId, Scale scale)
    {
        ValidateScale(scale);
        return await _scaleRepository.UpdateScaleAsync(scaleId, scale);
    }
    
    /// <inheritdoc />
    public async Task<bool> DeleteScaleAsync(Guid scaleId)
    {
        return await _scaleRepository.DeleteScaleAsync(scaleId);
    }
    
    private static void ValidateScale(Scale scale)
    {
        if (scale.Metadata?.Names == null || !scale.Metadata.Names.Any())
        {
            throw new ArgumentException("Scale must have at least one name");
        }
        
        if (scale.Intervals == null || !scale.Intervals.Any())
        {
            throw new ArgumentException("Scale must have at least one interval");
        }
        
        // Ensure names are not empty or whitespace
        if (scale.Metadata.Names.Any(name => string.IsNullOrWhiteSpace(name)))
        {
            throw new ArgumentException("Scale names cannot be empty or whitespace");
        }
    }
}