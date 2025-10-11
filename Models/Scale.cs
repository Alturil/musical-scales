using System.ComponentModel.DataAnnotations;

namespace MusicalScales.Api.Models;

/// <summary>
/// A musical scale containing metadata and interval structure
/// </summary>
public class Scale
{
    /// <summary>
    /// Unique identifier for the scale
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Metadata information about the scale
    /// </summary>
    [Required]
    public ScaleMetadata Metadata { get; set; } = new();
    
    /// <summary>
    /// The intervals that define the scale structure
    /// </summary>
    [Required]
    public IList<Interval> Intervals { get; set; } = new List<Interval>();
    
    /// <summary>
    /// When the scale was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the scale was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Returns the primary name of the scale
    /// </summary>
    public string PrimaryName => Metadata.Names.FirstOrDefault() ?? "Unnamed Scale";
    
    /// <summary>
    /// Returns a string representation of the scale
    /// </summary>
    public override string ToString()
    {
        return $"{PrimaryName} ({Intervals.Count} intervals)";
    }
}