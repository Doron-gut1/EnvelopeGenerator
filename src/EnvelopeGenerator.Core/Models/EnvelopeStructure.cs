namespace EnvelopeGenerator.Core.Models;

/// <summary>
/// Defines the structure of an envelope from mivnemtfhead
/// </summary>
public class EnvelopeStructure
{
    /// <summary>
    /// List of fields in the envelope
    /// </summary>
    public List<FieldDefinition> Fields { get; set; } = new();

    /// <summary>
    /// Whether to use DOS Hebrew encoding
    /// </summary>
    public bool DosHebrewEncoding { get; set; }

    /// <summary>
    /// Whether to reverse Hebrew text
    /// </summary>
    public bool ReverseHebrew { get; set; }

    /// <summary>
    /// Number of digits after decimal point for currency fields
    /// </summary>
    public int NumOfDigits { get; set; }

    /// <summary>
    /// Position of yearly rows (1: below periodic, 2: after periodic)
    /// </summary>
    public int PositionOfShnati { get; set; }

    /// <summary>
    /// Number of detail lines per envelope
    /// </summary>
    public int NumOfPerutLines { get; set; }
}