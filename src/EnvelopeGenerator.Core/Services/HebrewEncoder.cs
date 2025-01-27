using System.Text;

namespace EnvelopeGenerator.Core.Services;

// Services/HebrewEncoder.cs
public class HebrewEncoder
{
    private static readonly Encoding Windows1255 = Encoding.GetEncoding(1255);
    private static readonly Encoding Dos862 = Encoding.GetEncoding(862);

    public string ConvertToDos(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        byte[] windows1255Bytes = Windows1255.GetBytes(text);
        byte[] dos862Bytes = Encoding.Convert(Windows1255, Dos862, windows1255Bytes);
        return Dos862.GetString(dos862Bytes);
    }

    public string ReverseHebrew(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var chars = text.ToCharArray();
        int left = 0;
        int right = chars.Length - 1;

        // ���� �� ����� ������, ���� ��� ������ ������ ������� ������ �����
        while (left < right)
        {
            while (left < right && !IsHebrew(chars[left]))
                left++;

            while (left < right && !IsHebrew(chars[right]))
                right--;

            if (IsHebrew(chars[left]) && IsHebrew(chars[right]))
            {
                var temp = chars[left];
                chars[left] = chars[right];
                chars[right] = temp;
            }

            left++;
            right--;
        }

        return new string(chars);
    }

    private static bool IsHebrew(char c)
    {
        // ���� ������ ������
        return (c >= '�' && c <= '�') || c == '�' || c == '�' || c == '�' || c == '�' || c == '�';
    }

    public string FormatHebrewField(string value, bool dosHebrew, bool reverseHebrew, bool isTikToshavLink)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        // ����� ������ DOS ����� ��� �����
        if (dosHebrew && !isTikToshavLink)
        {
            value = ConvertToDos(value);
            if (reverseHebrew)
            {
                value = ReverseHebrew(value);
            }
        }

        return value;
    }
}