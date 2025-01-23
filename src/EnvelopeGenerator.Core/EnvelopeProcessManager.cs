using System.Runtime.CompilerServices;
using EnvelopeGenerator.Core.Services;

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
    private string _connectionString;
    private Services.EnvelopeGenerator? _generator;
    
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

    private void InitializeServices()
    {
        // TODO: להחליף את זה כשיתווסף OdbcConverter
        // var odbcConvert = new OdbcConverter.OdbcConverter();
        // _connectionString = odbcConvert.GetSqlConnectionString(_odbcName, "", "");
        _connectionString = "Server=localhost;Database=test;Trusted_Connection=True;"; // זמני
        _generator = new Services.EnvelopeGenerator(_connectionString);
    }

    public bool GenerateEnvelopes(out string errorMessage)
    {
        errorMessage = string.Empty;
        try
        {
            // יצירת תיקיית פלט
            var outputPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, 
                $"Mtf_{DateTime.Now:yyyyMMdd_HHmmss}");
            Directory.CreateDirectory(outputPath);

            // אתחול שירותים
            InitializeServices();

            if (_generator == null)
            {
                throw new InvalidOperationException("Failed to initialize services");
            }

            // הפעלת תהליך היצירה
            var result = _generator.GenerateEnvelopes(
                _actionType,
                _envelopeType,
                _batchNumber,
                _isYearly,
                _familyCode,
                _closureNumber,
                _voucherGroup,
                outputPath).GetAwaiter().GetResult();

            return result;
        }
        catch (Exception ex)
        {
            errorMessage = TraceException(ex.Message);
            Console.WriteLine(errorMessage);
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