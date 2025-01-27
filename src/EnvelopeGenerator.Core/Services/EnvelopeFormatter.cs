using System.Text;
using System.Data.Common;
using EnvelopeGenerator.Core.Models;
using Microsoft.Data.SqlClient;

namespace EnvelopeGenerator.Core.Services;

/// <summary>
/// Formatter that exactly matches Access output format
/// </summary>
// Services/EnvelopeFormatter.cs
public class EnvelopeFormatter
{
    private readonly HebrewEncoder _hebrewEncoder;
    private readonly EnvelopeStructure _structure;

    public EnvelopeFormatter(EnvelopeStructure structure)
    {
        _structure = structure;
        _hebrewEncoder = new HebrewEncoder();
    }

      public string FormatLine(DbDataReader reader)  // שינוי מ-DbDataReader  ל-DbDataReader
    {
        StringBuilder line = new();
        foreach (var field in _structure.Fields.OrderBy(f => f.RealSeder))
        {
            string formattedField = FormatField(reader, field);
            line.Append(formattedField);
        }
        return line.ToString();
    }

    private string FormatField(DbDataReader reader, EnvelopeField field)
    {
        // בדיקת שדות מיוחדים simanenu ו-rek
        if (field.InName.Contains("simanenu", StringComparison.OrdinalIgnoreCase) ||
            field.InName.Contains("rek", StringComparison.OrdinalIgnoreCase))
        {
            return new string(field.FldType == 1 ? ' ' : '0', field.Length);
        }

        // טיפול במספרי שוברים שליליים
        if (field.InName.StartsWith("shovar", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var ordinal = reader.GetOrdinal(field.InName);
                if (!reader.IsDBNull(ordinal))
                {
                    var currValue = reader.GetValue(ordinal);
                    if (decimal.TryParse(currValue.ToString(), out decimal numValue) && numValue < 0)
                    {
                        return new string('0', field.Length);
                    }
                }
            }
            catch
            {
                // אם השדה לא קיים, נתעלם
            }
        }

        // קבלת ערך השדה
        string value = GetFieldValue(reader, field);
        if (string.IsNullOrEmpty(value))
        {
            return new string(field.FldType == 1 || field.FldType == 3 ? ' ' : '0', field.Length);
        }

        // פורמט לפי סוג השדה
        return field.FldType switch
        {
            1 => FormatTextField(value, field),
            2 => FormatNumericField(value, field),
            3 => FormatCurrencyField(value, field),
            _ => new string(' ', field.Length)
        };
    }

    private string FormatTextField(string value, EnvelopeField field)
    {
        // טיפול בתאריך הדפסה
        if (field.InName.Equals("hadpasadt", StringComparison.OrdinalIgnoreCase) &&
            DateTime.TryParse(value, out DateTime date))
        {
            value = date.ToString("dd/MM/yyyy");
        }

        // טיפול בקידוד עברית
        value = _hebrewEncoder.FormatHebrewField(
            value,
            _structure.DosHebrewEncoding,
            _structure.ReverseHebrew,
            field.InName.Equals("TikToshavLink", StringComparison.OrdinalIgnoreCase));

        // קיצור וריפוד בדיוק כמו באקסס
        value = Left(value, field.Length);
        return Right(new string(' ', field.Length) + value, field.Length);
    }

    private string FormatNumericField(string value, EnvelopeField field)
    {
        // טיפול בתאריכים
        if (DateTime.TryParse(value, out DateTime date))
        {
            return Right(new string('0', field.Length) + date.ToString("ddMMyy"), field.Length);
        }

        // טיפול במספרים רגילים
        return Right(new string('0', field.Length) + value, field.Length);
    }

    private string FormatCurrencyField(string value, EnvelopeField field)
    {
        if (!decimal.TryParse(value, out decimal amount))
        {
            return new string(' ', field.Length);
        }

        string format = _structure.NumOfDigits > 0
            ? "0." + new string('0', _structure.NumOfDigits)
            : "0";

        string formatted = amount.ToString(format);
        return Right(new string(' ', field.Length) + formatted, field.Length);
    }

    private static string GetFieldValue(DbDataReader  reader, EnvelopeField field)
    {
        try
        {
            var ordinal = reader.GetOrdinal(field.InName);
            if (reader.IsDBNull(ordinal))
                return string.Empty;

            return reader.GetValue(ordinal).ToString() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string Left(string value, int length)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Length <= length ? value : value[..length];
    }

    private static string Right(string value, int length)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Length <= length ? value.PadLeft(length) : value[^length..];
    }
}