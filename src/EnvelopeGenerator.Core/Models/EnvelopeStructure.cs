namespace EnvelopeGenerator.Core.Models;

/// <summary>
/// Defines the structure of an envelope from mivnemtfhead
/// </summary>
public class EnvelopeStructure
{
    public List<EnvelopeField> Fields { get; set; } = new();
    public bool DosHebrewEncoding { get; set; }           // האם לקודד ל-DOS
    public bool ReverseHebrew { get; set; }               // האם להפוך עברית
    public int NumOfDigits { get; set; }                  // ספרות אחרי הנקודה במטבע
    public int PositionOfShnati { get; set; }             // מיקום שורות שנתיות (1=מתחת, 2=אחרי)
    public int NumOfPerutLines { get; set; }              // מספר שורות פירוט
    public int NumOfPerutFields { get; set; }             // מספר שדות בכל שורת פירוט
}