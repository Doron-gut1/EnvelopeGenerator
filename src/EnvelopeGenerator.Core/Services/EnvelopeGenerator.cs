using System.Text;
using Microsoft.Data.SqlClient;
using Dapper;
using EnvelopeGenerator.Core.Models;
using System.Runtime.CompilerServices;
using System.Data.Common;

namespace EnvelopeGenerator.Core.Services;

public class EnvelopeGenerator
{
    private readonly string _connectionString;
    private readonly QueryBuilder _queryBuilder;
    private readonly EnvelopeFormatter _formatter;
    private readonly EnvelopeStructure _structure;
    private const string DATE_FORMAT = "yyyyMMdd_HHmmss";

    public EnvelopeGenerator(string connectionString, EnvelopeStructure structure)
    {
        _connectionString = connectionString;
        _structure = structure;
        _queryBuilder = new QueryBuilder();
        _formatter = new EnvelopeFormatter(structure);
    }

    public async Task<(bool Success, string ErrorMessage)> GenerateFiles(
        int actionType,
        int batchNumber,
        bool isYearly,
        long? familyCode,
        int? closureNumber,
        long? voucherGroup,
        string outputPath)
    {
        try
        {
            // הכנת שאילתא
            var query = _queryBuilder.BuildDynamicQuery(
                _structure, actionType, batchNumber, familyCode,
                closureNumber, voucherGroup, isYearly);

            // עדכון uniqnum למעטפיות משולבות
            if (actionType == 3)
            {
                await UpdateUniqNumForCombined(batchNumber, familyCode,
                    closureNumber, voucherGroup);
            }

            // פתיחת קבצי פלט
            var files = OpenOutputFiles(outputPath, actionType, batchNumber);

            try
            {
                // עיבוד הנתונים
                await using var connection = new SqlConnection(_connectionString);
                await using var reader = await connection.ExecuteReaderAsync(query);

                await ProcessData(reader, files, actionType, isYearly);

                return (true, string.Empty);
            }
            finally
            {
                foreach (var file in files.Values)
                {
                    await file.DisposeAsync();
                }
            }
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    private async Task UpdateUniqNumForCombined(
        int batchNumber,
        long? familyCode,
        int? closureNumber,
        long? voucherGroup)
    {
        const string sql = @"
            -- עדכון ברמת נכס
            UPDATE shovarhead SET uniqnum = -100
            FROM shovarhead 
            INNER JOIN shovarhead AS sh1 
                ON shovarhead.hskod = sh1.hskod 
                AND shovarhead.mspkod = sh1.mspkod 
                AND shovarhead.mnt = sh1.mnt
            WHERE shovarhead.mnt = @batchNumber 
                AND ISNULL(shovarhead.shnati, 0) = 0 
                AND ISNULL(shovarhead.shovarmsp, 0) = 0
                AND ISNULL(sh1.shnati, 0) = 0 
                AND ISNULL(sh1.shovarmsp, 0) = 0
                AND ISNULL(shovarhead.manahovnum, 0) = 0 
                AND ISNULL(sh1.manahovnum, 0) <> 0
                AND (@familyCode IS NULL OR shovarhead.mspkod = @familyCode)
                AND (@closureNumber IS NULL OR shovarhead.sgrnum = @closureNumber)
                AND (@voucherGroup IS NULL OR shovarhead.kvuzashovar = @voucherGroup);

            -- עדכון ברמת משפחה
            UPDATE shovarhead SET uniqnum = -100
            FROM shovarhead 
            INNER JOIN shovarhead AS sh1 
                ON shovarhead.mspkod = sh1.mspkod 
                AND shovarhead.mnt = sh1.mnt
            WHERE shovarhead.mnt = @batchNumber 
                AND ISNULL(shovarhead.shnati, 0) = 0 
                AND ISNULL(shovarhead.shovarmsp, 0) = 1
                AND ISNULL(sh1.shnati, 0) = 0 
                AND ISNULL(sh1.shovarmsp, 0) = 1
                AND ISNULL(shovarhead.manahovnum, 0) = 0 
                AND ISNULL(sh1.manahovnum, 0) <> 0
                AND (@familyCode IS NULL OR shovarhead.mspkod = @familyCode)
                AND (@closureNumber IS NULL OR shovarhead.sgrnum = @closureNumber)
                AND (@voucherGroup IS NULL OR shovarhead.kvuzashovar = @voucherGroup);";

        await using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, new { batchNumber, familyCode, closureNumber, voucherGroup });
    }

    private Dictionary<string, StreamWriter> OpenOutputFiles(
        string outputPath,
        int actionType,
        int batchNumber)
    {
        var files = new Dictionary<string, StreamWriter>();
        var monthName = batchNumber.ToString().Replace("/", "");

        if (actionType == 1 || actionType == 2)
        {
            var filename = actionType == 1 ? "SvrShotef" : "SvrHov";
            files[filename] = new StreamWriter(
                Path.Combine(outputPath, $"{filename}_{monthName}.txt"),
                false, Encoding.GetEncoding(1255));
        }
        else
        {
            files["SvrShotef"] = new StreamWriter(
                Path.Combine(outputPath, $"SvrShotef_{monthName}.txt"),
                false, Encoding.GetEncoding(1255));
            files["SvrHov"] = new StreamWriter(
                Path.Combine(outputPath, $"SvrHov_{monthName}.txt"),
                false, Encoding.GetEncoding(1255));
            files["SvrMeshulavShotefHov"] = new StreamWriter(
                Path.Combine(outputPath, $"SvrMeshulavShotefHov_{monthName}.txt"),
                false, Encoding.GetEncoding(1255));
        }

        return files;
    }

    private async Task ProcessData(
        DbDataReader reader,
        Dictionary<string, StreamWriter> files,
        int actionType,
        bool isYearly)
    {
        var state = new ProcessingState();
        var emptyLine = new string(' ', CalculateLineLength());

        while (await reader.ReadAsync())
        {
            var currentLine = _formatter.FormatLine(reader);
            var currentData = new ProcessedLine(reader);

            if (actionType == 3 && currentData.UniqNum == -100)
            {
                await ProcessCombinedLine(files["SvrMeshulavShotefHov"],
                    currentLine, currentData, state, emptyLine);
            }
            else if (actionType == 1 && isYearly)
            {
                await ProcessYearlyLine(files["SvrShotef"],
                    currentLine, currentData, state, emptyLine);
            }
            else
            {
                var file = GetAppropriateFile(files, actionType, currentData.ManaHovNum);
                await file.WriteLineAsync(currentLine);
                state.IsPreviousTkufati = true;
            }

            state.UpdateState(currentData);
        }
    }

    private int CalculateLineLength()
    {
        return _structure.Fields.Sum(f => f.Length);
    }

    private StreamWriter GetAppropriateFile(
        Dictionary<string, StreamWriter> files,
        int actionType,
        long? manaHovNum)
    {
        var key = actionType switch
        {
            1 => "SvrShotef",
            2 => "SvrHov",
            _ => manaHovNum == null ? "SvrShotef" : "SvrHov"
        };
        return files[key];
    }

    private async Task ProcessCombinedLine(
        StreamWriter file,
        string currentLine,
        ProcessedLine currentData,
        ProcessingState state,
        string emptyLine)
    {
        if (state.PreviousMspkod != currentData.Mspkod ||
            state.PreviousHskod != currentData.Miun)
        {
            state.LineCounter = 1;
        }

        if (state.LineCounter <= 2)
        {
            await file.WriteLineAsync(currentLine);
            if (state.LineCounter == 1)
            {
                state.LastTkufatiLine = currentLine;
            }
            else
            {
                state.LastHovLine = currentLine;
            }
            state.LineCounter++;
            state.IsPreviousHov = true;
        }
        else
        {
            if (state.IsPreviousHov && currentData.ManaHovNum.HasValue)
            {
                await file.WriteLineAsync(state.LastTkufatiLine);
                await file.WriteLineAsync(currentLine);
            }
            else if (!state.IsPreviousHov && !currentData.ManaHovNum.HasValue)
            {
                await file.WriteLineAsync(state.LastHovLine);
                await file.WriteLineAsync(currentLine);
            }
            else
            {
                await file.WriteLineAsync(currentLine);
                if (state.IsPreviousHov)
                {
                    state.LastTkufatiLine = currentLine;
                }
                else
                {
                    state.LastHovLine = currentLine;
                }
                state.IsPreviousHov = !state.IsPreviousHov;
            }
        }
    }
    private async Task ProcessYearlyLine(
        StreamWriter file,
        string currentLine,
        ProcessedLine currentData,
        ProcessingState state,
        string emptyLine)
    {
        if (_structure.PositionOfShnati == 1)  // שורות שנתיות מתחת לתקופתיות
        {
            if (currentData.Shnati)
            {
                if (state.IsPreviousTkufati)
                {
                    if (state.PreviousMspkod != currentData.Mspkod ||
                        state.PreviousHskod != currentData.Miun)
                    {
                        await file.WriteLineAsync(emptyLine);
                        await file.WriteLineAsync(currentLine);
                    }
                    else
                    {
                        await file.WriteLineAsync(currentLine);
                    }
                }
                else
                {
                    await file.WriteLineAsync(emptyLine);
                    await file.WriteLineAsync(currentLine);
                }
                state.IsPreviousTkufati = false;
            }
            else
            {
                if (state.IsPreviousTkufati)
                {
                    await file.WriteLineAsync(emptyLine);
                }
                await file.WriteLineAsync(currentLine);
                state.IsPreviousTkufati = true;
            }
        }
        else  // positionOfShnati == 2 - שורות שנתיות אחרי תקופתיות
        {
            if (currentData.Shnati)
            {
                if (state.IsPreviousTkufati &&
                    (state.PreviousMspkod != currentData.Mspkod ||
                     state.PreviousHskod != currentData.Miun))
                {
                    // דחיפת שתי שורות ריקות
                    await file.WriteLineAsync(emptyLine);
                    await file.WriteLineAsync(emptyLine);
                }
                else if (!state.IsPreviousTkufati)
                {
                    // דחיפת שורה ריקה אחת
                    await file.WriteLineAsync(emptyLine);
                }
                await file.WriteLineAsync(currentLine);
                state.IsPreviousTkufati = false;
            }
            else
            {
                await file.WriteLineAsync(currentLine);
                state.IsPreviousTkufati = true;

                // בדיקה אם זו השורה האחרונה
                if (state.PreviousMspkod != currentData.Mspkod ||
                    state.PreviousHskod != currentData.Miun)
                {
                    await file.WriteLineAsync(emptyLine);
                }
            }
        }
    }
}



