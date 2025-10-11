using System.ComponentModel.DataAnnotations;
using MusicalScales.Api.Models.Enums;

namespace MusicalScales.Api.Models;

/// <summary>
/// A musical pitch with diatonic name, accidental, pitch offset and semitone offset
/// </summary>
public class Pitch
{
    /// <summary>
    /// The diatonic pitch name (C, D, E, F, G, A, B)
    /// </summary>
    [Required]
    public DiatonicPitchName Name { get; set; }
    
    /// <summary>
    /// The accidental applied to the pitch
    /// </summary>
    [Required]
    public AccidentalName Accidental { get; set; }
    
    /// <summary>
    /// Number of pitches offset added by the pitch
    /// </summary>
    public int PitchOffset { get; set; }
    
    /// <summary>
    /// Number of semitones offset added by the pitch
    /// </summary>
    public int SemitoneOffset { get; set; }
    
    /// <summary>
    /// Returns a string representation of the pitch
    /// </summary>
    public override string ToString()
    {
        var accidentalSymbol = Accidental switch
        {
            AccidentalName.DoubleFlat => "♭♭",
            AccidentalName.Flat => "♭",
            AccidentalName.Natural => "",
            AccidentalName.Sharp => "♯",
            AccidentalName.DoubleSharp => "♯♯",
            _ => ""
        };
        
        return $"{Name}{accidentalSymbol}";
    }
}