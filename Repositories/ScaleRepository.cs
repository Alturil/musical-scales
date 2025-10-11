using Microsoft.EntityFrameworkCore;
using MusicalScales.Api.Data;
using MusicalScales.Api.Models;
using System.Text.Json;

namespace MusicalScales.Api.Repositories;

/// <summary>
/// Repository implementation for Scale operations using Entity Framework Core
/// </summary>
public class ScaleRepository : IScaleRepository
{
    private readonly MusicalScalesDbContext _context;
    
    public ScaleRepository(MusicalScalesDbContext context)
    {
        _context = context;
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<Scale>> GetAllScalesAsync()
    {
        return await _context.Scales
            .OrderBy(s => s.Metadata.Names.FirstOrDefault())
            .ToListAsync();
    }
    
    /// <inheritdoc />
    public async Task<Scale?> GetScaleByIdAsync(Guid id)
    {
        return await _context.Scales
            .FirstOrDefaultAsync(s => s.Id == id);
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<Scale>> GetScalesByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Enumerable.Empty<Scale>();
            
        return await _context.Scales
            .Where(s => s.Metadata.Names.Any(n => n.ToLower().Contains(name.ToLower())))
            .OrderBy(s => s.Metadata.Names.FirstOrDefault())
            .ToListAsync();
    }
    
    /// <inheritdoc />
    public async Task<Scale?> GetScaleByIntervalsAsync(IList<Interval> intervals)
    {
        if (intervals == null || !intervals.Any())
            return null;
            
        var intervalsJson = JsonSerializer.Serialize(intervals);
        
        // This is a simplified comparison - in a real implementation,
        // you might want to compare interval structures more intelligently
        return await _context.Scales
            .Where(s => EF.Property<string>(s, "Intervals") == intervalsJson)
            .FirstOrDefaultAsync();
    }
    
    /// <inheritdoc />
    public async Task<Scale> CreateScaleAsync(Scale scale)
    {
        scale.Id = Guid.NewGuid();
        scale.CreatedAt = DateTime.UtcNow;
        scale.UpdatedAt = DateTime.UtcNow;
        
        _context.Scales.Add(scale);
        await _context.SaveChangesAsync();
        
        return scale;
    }
    
    /// <inheritdoc />
    public async Task<Scale?> UpdateScaleAsync(Guid id, Scale scale)
    {
        var existingScale = await _context.Scales.FindAsync(id);
        
        if (existingScale == null)
            return null;
            
        existingScale.Metadata = scale.Metadata;
        existingScale.Intervals = scale.Intervals;
        existingScale.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return existingScale;
    }
    
    /// <inheritdoc />
    public async Task<bool> DeleteScaleAsync(Guid id)
    {
        var scale = await _context.Scales.FindAsync(id);
        
        if (scale == null)
            return false;
            
        _context.Scales.Remove(scale);
        await _context.SaveChangesAsync();
        
        return true;
    }
    
    /// <inheritdoc />
    public async Task<bool> ScaleExistsAsync(Guid id)
    {
        return await _context.Scales.AnyAsync(s => s.Id == id);
    }
}