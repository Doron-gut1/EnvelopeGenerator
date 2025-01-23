using System.Runtime.CompilerServices;

namespace EnvelopeGenerator.Core;

public class EnvelopeProcessManager
{
    private readonly string _odbcName;
    private readonly int _actionType;
    private readonly int _envelopeType;
    private readonly int _batchNumber;
    private readonly bool _isYearly;
    private readonly long? _familyCode;
    private readonly int? _closureNumber;
    private readonly long? _voucherGroup;
    
    public EnvelopeProcessManager(
        string odbcName,
        int actionType,
        int envelopeType,
        int batchNumber,
        bool isYearly = false,
        long? familyCode = null,
        int? closureNumber = null,
        long? voucherGroup = null)
    {
        _odbcName = odbcName;
        _actionType = actionType;
        _envelopeType = envelopeType;
        _batchNumber = batchNumber;
        _isYearly = isYearly;
        _familyCode = familyCode;
        _closureNumber = closureNumber;
        _voucherGroup = voucherGroup;
    }

    public bool GenerateEnvelopes(out string errorMessage)
    {
        errorMessage = string.Empty;
        try
        {
            // TODO: Implement envelope generation logic
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = TraceException(ex.Message);
            Console.WriteLine(errorMessage);  // Temporary logging to console
            return false;
        }
    }

    private static string TraceException(
        string exceptionMsg,
        [CallerMemberName] string memberName = null,
        [CallerFilePath] string sourcefilePath = null,
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        return $"{memberName} Exception - {exceptionMsg} ({new FileInfo(sourcefilePath).Name} line:{sourceLineNumber})";
    }
}