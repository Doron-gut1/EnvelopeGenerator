using Dapper;
using Microsoft.Data.SqlClient;
using EnvelopeGenerator.Core.Models;
using System.Text;
using System.Runtime.CompilerServices;

namespace EnvelopeGenerator.Core.Services;

/// <summary>
/// Core service for generating envelope files
/// </summary>
public class EnvelopeGenerator
{
    private readonly string _connectionString;
    private readonly HebrewEncoder _hebrewEncoder;
    private static readonly Encoding FileEncoding = Encoding.GetEncoding(1255);

    public EnvelopeGenerator(string connectionString)
    {
        _connectionString = connectionString;
        _hebrewEncoder = new HebrewEncoder();
    }

    public async Task<bool> GenerateEnvelopes(
        int actionType,
        int envelopeType,
        int batchNumber,
        bool isYearly,
        long? familyCode,
        int? closureNumber,
        long? voucherGroup,
        string outputPath)
    {
        try
        {
            // 1. Get envelope structure
            var structure = await GetEnvelopeStructure(envelopeType);

            // 2. Build and execute query
            var query = BuildDynamicQuery(structure, actionType, batchNumber, 
                familyCode, closureNumber, voucherGroup);
            
            // 3. Process data and write files
            await using var connection = new SqlConnection(_connectionString);
            await using var reader = await connection.ExecuteReaderAsync(query);
            await ProcessAndWriteFiles(reader, structure, actionType, outputPath);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(TraceException(ex.Message));
            return false;
        }
    }

    private async Task<EnvelopeStructure> GetEnvelopeStructure(int envelopeType)
    {
        const string sql = @"
            SELECT m.inname as Name, m.fldtype as Type, m.length as Length, 
                   m.realseder as [Order], m.recordset as Source
            FROM mivnemtf m
            WHERE m.sugmtf = @envelopeType 
              AND ISNULL(m.show, 0) = 1 
              AND ISNULL(m.notInMtf, 0) = 0
            ORDER BY m.realseder;

            SELECT dosheb as DosHebrewEncoding,
                   numOfDigits as NumOfDigits,
                   positionOfShnati as PositionOfShnati,
                   numOfPerutLines as NumOfPerutLines
            FROM mivnemtfhead
            WHERE kodsugmtf = @envelopeType;

            SELECT revheb as ReverseHebrew
            FROM paramset;";

        await using var connection = new SqlConnection(_connectionString);
        using var multi = await connection.QueryMultipleAsync(sql, new { envelopeType });

        var fields = (await multi.ReadAsync<FieldDefinition>()).ToList();
        var header = await multi.ReadFirstAsync<EnvelopeStructure>();
        var param = await multi.ReadFirstAsync<dynamic>();

        header.Fields = fields;
        header.ReverseHebrew = param.ReverseHebrew;

        return header;
    }

    private string BuildDynamicQuery(EnvelopeStructure structure, 
        int actionType, int batchNumber, long? familyCode, 
        int? closureNumber, long? voucherGroup)
    {
        // TODO: Implement dynamic query building based on structure
        throw new NotImplementedException();
    }

    private async Task ProcessAndWriteFiles(SqlDataReader reader, 
        EnvelopeStructure structure, int actionType, string outputPath)
    {
        // TODO: Implement file processing and writing
        throw new NotImplementedException();
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