using System.Runtime.CompilerServices;

namespace EnvelopeGenerator.Core;

using Dapper;
using global::EnvelopeGenerator.Core.Models;
using System.Runtime.CompilerServices;
public record EnvelopeResult(bool Success, string ErrorMessage);

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
    private string _connectionString = string.Empty;
    private Services.EnvelopeGenerator? _generator;
    private EnvelopeStructure? _structure;

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

    private async Task InitializeServices()
    {
        // TODO: להחליף את זה כשיתווסף OdbcConverter
         var odbcConvert = new OdbcConverter.OdbcConverter();
         _connectionString = odbcConvert.GetSqlConnectionString(_odbcName, string.Empty, string.Empty);
        //_connectionString = "Server=localhost;Database=test;Trusted_Connection=True;"; // זמני

        using var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);

        // שינוי השאילתא כך שהשמות יתאימו למאפיינים במחלקה
        var sql = @"
        SELECT 
            m.inname as InName,           -- שינוי השם לפי המאפיין
            m.fldtype as FldType,         -- שינוי השם לפי המאפיין
            m.length as Length,           -- שינוי השם לפי המאפיין
            m.realseder as RealSeder,     -- שינוי השם לפי המאפיין
            m.recordset as Recordset,     -- שינוי השם לפי המאפיין
            ISNULL(m.show, 0) as Show,               -- הוספת ISNULL
            ISNULL(m.notInMtf, 0) as NotInMtf       -- הוספת ISNULL
        FROM mivnemtf m
        WHERE m.sugmtf = @envelopeType
            AND ISNULL(m.show, 0) = 1               -- הוספת תנאי סינון
            AND ISNULL(m.notInMtf, 0) = 0          -- הוספת תנאי סינון
        ORDER BY m.realseder";

        var fields = (await connection.QueryAsync<EnvelopeField>(sql, new { envelopeType = _envelopeType })).ToList();

        if (!fields.Any())
        {
            throw new InvalidOperationException($"No fields found for envelope type {_envelopeType}");
        }

        var structureSql = @"
        SELECT TOP 1
            ISNULL(mh.dosheb, 0) as DosHebrewEncoding,
            ISNULL(p.revheb, 0) as ReverseHebrew,
            ISNULL(mh.numOfDigits, 2) as NumOfDigits,
            ISNULL(mh.positionOfShnati, 2) as PositionOfShnati,
            ISNULL(mh.numOfPerutLines, 0) as NumOfPerutLines,
            ISNULL((SELECT CASE 
                WHEN EXISTS(SELECT 1 FROM mivnemtf 
                          WHERE sugmtf = @envelopeType 
                          AND inname = 'simanenu' 
                          AND ISNULL(show, 0) = 1) 
                THEN 5 ELSE 4 END), 4) as NumOfPerutFields
        FROM mivnemtfhead mh
        CROSS JOIN param p
        WHERE mh.kodsugmtf = @envelopeType";

        var structure = await connection.QuerySingleOrDefaultAsync<EnvelopeStructure>(
            structureSql,
            new { envelopeType = _envelopeType });

        if (structure == null)
        {
            throw new InvalidOperationException($"Structure not found for envelope type {_envelopeType}");
        }

        structure.Fields = fields;
        _structure = structure;
        _generator = new Services.EnvelopeGenerator(_connectionString, _structure);
    }

    private static async Task<T> GetSingleValue<T>(Microsoft.Data.SqlClient.SqlConnection connection, string sql, object? param = null)
    {
        return await connection.QueryFirstOrDefaultAsync<T>(sql, param);
    }

    public async Task<EnvelopeResult> GenerateEnvelopesAsync()
    {
        try
        {
            await InitializeServices();

            if (_generator == null || _structure == null)
            {
                return new EnvelopeResult(false, "Failed to initialize services");
            }

            // יצירת תיקיית פלט
            var outputPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                $"Mtf_{DateTime.Now:yyyyMMdd_HHmmss}");
            Directory.CreateDirectory(outputPath);

            // הפעלת תהליך היצירה
            var result = await _generator.GenerateFiles(
                _actionType,
                _batchNumber,
                _isYearly,
                _familyCode,
                _closureNumber,
                _voucherGroup,
                outputPath);

            return new EnvelopeResult(result.Success, result.ErrorMessage);
        }
        catch (Exception ex)
        {
            var error = TraceException(ex.Message);
            Console.WriteLine(error);
            return new EnvelopeResult(false, error);
        }
    }

    private static string TraceException(
        string exceptionMsg,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? sourcefilePath = null,
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        return $"{memberName} Exception - {exceptionMsg} ({Path.GetFileName(sourcefilePath ?? string.Empty)} line:{sourceLineNumber})";
    }
}