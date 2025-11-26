using System.ComponentModel.DataAnnotations;

namespace MusicalScales.Api.Models;

/// <summary>
/// Scale metadata containing names and other descriptive information
/// </summary>
public class ScaleMetadata
{
    /// <summary>
    /// Collection of names for the scale
    /// </summary>
    [Required]
    public IList<string> Names { get; set; } = [];
    
    /// <summary>
    /// Optional description of the scale
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Optional origin/culture information
    /// </summary>
    public string? Origin { get; set; }
}