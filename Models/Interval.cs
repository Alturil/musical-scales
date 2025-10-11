using System.ComponentModel.DataAnnotations;
using MusicalScales.Api.Models.Enums;

namespace MusicalScales.Api.Models;

/// <summary>
/// An interval with size, quality, interval offset and semitone offset
/// </summary>
public class Interval
{
    /// <summary>
    /// The size of the interval (Unison, Second, Third, etc.)
    /// </summary>
    [Required]
    public IntervalSizeName Name { get; set; }
    
    /// <summary>
    /// The quality of the interval (Perfect, Major, Minor, etc.)
    /// </summary>
    [Required]
    public IntervalQualityName Quality { get; set; }
    
    /// <summary>
    /// Number of pitches offset added by the interval
    /// </summary>
    public int PitchOffset { get; set; }
    
    /// <summary>
    /// Number of semitones offset added by the interval
    /// </summary>
    public int SemitoneOffset { get; set; }
    
    /// <summary>
    /// Returns a string representation of the interval
    /// </summary>
    public override string ToString()
    {
        var qualityString = Quality switch
        {
            IntervalQualityName.Diminished => "dim",
            IntervalQualityName.Minor => "m",
            IntervalQualityName.Major => "M",
            IntervalQualityName.Perfect => "P",
            IntervalQualityName.Augmented => "aug",
            _ => Quality.ToString()
        };
        
        var sizeNumber = Name switch
        {
            IntervalSizeName.Unison => "1",
            IntervalSizeName.Second => "2",
            IntervalSizeName.Third => "3",
            IntervalSizeName.Fourth => "4",
            IntervalSizeName.Fifth => "5",
            IntervalSizeName.Sixth => "6",
            IntervalSizeName.Seventh => "7",
            IntervalSizeName.Octave => "8",
            _ => "?"
        };
        
        return $"{qualityString}{sizeNumber}";
    }
}