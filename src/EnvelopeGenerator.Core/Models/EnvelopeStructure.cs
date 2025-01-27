namespace EnvelopeGenerator.Core.Models;

/// <summary>
/// Defines the structure of an envelope from mivnemtfhead
/// </summary>
public class EnvelopeStructure
{
    public List<EnvelopeField> Fields { get; set; } = new();
    public bool DosHebrewEncoding { get; set; }           // ��� ����� �-DOS
    public bool ReverseHebrew { get; set; }               // ��� ����� �����
    public int NumOfDigits { get; set; }                  // ����� ���� ������ �����
    public int PositionOfShnati { get; set; }             // ����� ����� ������ (1=����, 2=����)
    public int NumOfPerutLines { get; set; }              // ���� ����� �����
    public int NumOfPerutFields { get; set; }             // ���� ���� ��� ���� �����
}