using System.Text;

namespace EnvelopeGenerator.Core.Services;

/// <summary>
/// Handles Hebrew text encoding and transformations
/// </summary>
public class HebrewEncoder
{
    private static readonly Encoding Windows1255 = Encoding.GetEncoding(1255);
    private static readonly Encoding Dos862 = Encoding.GetEncoding(862);

    /// <summary>
    /// Converts text to DOS Hebrew encoding
    /// </summary>
    public string ConvertToDos(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        byte[] windows1255Bytes = Windows1255.GetBytes(text);
        byte[] dos862Bytes = Encoding.Convert(Windows1255, Dos862, windows1255Bytes);
        return Dos862.GetString(dos862Bytes);
    }

    /// <summary>
    /// Reverses Hebrew text for proper display
    /// </summary>
    public string ReverseHebrew(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        char[] chars = text.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }
}