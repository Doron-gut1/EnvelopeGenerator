using System.Text;
using Microsoft.Data.SqlClient;

namespace EnvelopeGenerator.Core.Services;

/// <summary>
/// Handles exact formatting of envelope lines to match Access output
/// </summary>
public class EnvelopeFormatter
{
    private readonly HebrewEncoder _hebrewEncoder;
    private readonly EnvelopeStructure _structure;
    
    public EnvelopeFormatter(EnvelopeStructure structure)
    {
        _structure = structure;
        _hebrewEncoder = new HebrewEncoder();
    }

    public string FormatLine(SqlDataReader reader)
    {
        var line = new StringBuilder();
        foreach (var field in _structure.Fields.OrderBy(f => f.Order))
        {
            var formattedField = FormatField(reader, field);
            line.Append(formattedField);
        }
        return line.ToString();
    }

    private string FormatField(SqlDataReader reader, FieldDefinition field)
    {
        // Handle simanenu and rek fields exactly as Access does
        if (InStr(field.Name, "simanenu") > 0 || InStr(field.Name, "rek") > 0)
        {
            return GetPaddedEmptyValue(field);
        }

        // Handle negative voucher numbers exactly as Access does
        if (field.Name.StartsWith("shovar", StringComparison.OrdinalIgnoreCase))
        {
            var value = GetValue(reader, field.Name);
            if (value != null && decimal.TryParse(value.ToString(), out decimal numValue) && numValue < 0)
            {
                return String(field.Length, '0');
            }
        }

        var rawValue = GetValue(reader, field.Name);
        if (rawValue == null || rawValue == DBNull.Value)
        {
            return GetPaddedEmptyValue(field);
        }

        string formattedValue = rawValue.ToString() ?? string.Empty;

        // Format based on field type with exact Access compatibility
        return field.Type switch
        {
            1 => FormatTextField(formattedValue, field),
            2 => FormatNumericField(rawValue, field),
            3 => FormatCurrencyField(rawValue, field),
            _ => GetPaddedEmptyValue(field)
        };
    }

    private string FormatTextField(string value, FieldDefinition field)
    {
        if (string.IsNullOrEmpty(value))
            return String(field.Length, ' ');

        // Special handling for hadpasadt exactly as Access does
        if (field.Name.Equals("hadpasadt", StringComparison.OrdinalIgnoreCase))
        {
            if (DateTime.TryParse(value, out DateTime date))
            {
                value = date.ToString("dd/MM/yyyy");
            }
        }
        
        // DOS Hebrew encoding exactly as Access does
        if (_structure.DosHebrewEncoding && !field.Name.Equals("TikToshavLink", StringComparison.OrdinalIgnoreCase))
        {
            value = _hebrewEncoder.ConvertToDos(value);
            if (_structure.ReverseHebrew)
            {
                value = _hebrewEncoder.ReverseHebrew(value);
            }
        }

        // Trim and pad exactly as Access does using Right function
        value = Left(value, field.Length);
        return Right(String(field.Length, ' ') + value, field.Length);
    }

    private string FormatNumericField(object value, FieldDefinition field)
    {
        if (value is DateTime date)
        {
            // Format dates exactly as Access does
            return Right(String(field.Length, '0') + date.ToString("ddMMyy"), field.Length);
        }

        var strValue = value.ToString() ?? string.Empty;
        return Right(String(field.Length, '0') + strValue, field.Length);
    }

    private string FormatCurrencyField(object value, FieldDefinition field)
    {
        if (!decimal.TryParse(value.ToString(), out decimal amount))
        {
            return String(field.Length, ' ');
        }

        // Format with exact decimal digits as specified in structure
        string format = _structure.NumOfDigits > 0 
            ? "0." + String(_structure.NumOfDigits, '0')
            : "0";
            
        var formatted = amount.ToString(format);
        return Right(String(field.Length, ' ') + formatted, field.Length);
    }

    private string GetPaddedEmptyValue(FieldDefinition field)
    {
        return field.Type == 1 || field.Type == 3 
            ? String(field.Length, ' ') 
            : String(field.Length, '0');
    }

    // Access-compatible string functions
    private static string String(int length, char character) => new(character, length);
    
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

    private static int InStr(string value, string search)
    {
        return value.IndexOf(search, StringComparison.OrdinalIgnoreCase);
    }

    private static object? GetValue(SqlDataReader reader, string fieldName)
    {
        try
        {
            return reader[fieldName];
        }
        catch
        {
            return null;
        }
    }
}