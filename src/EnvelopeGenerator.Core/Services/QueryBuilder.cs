using EnvelopeGenerator.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvelopeGenerator.Core.Services
{
    public class QueryBuilder
    {
        public string BuildDynamicQuery(
      EnvelopeStructure structure,
      int actionType,
      int batchNumber,
      long? familyCode,
      int? closureNumber,
      long? voucherGroup,
      bool isYearly)
        {
            var sb = new StringBuilder();
            sb.Append("SELECT ");

            // השדות הבסיסיים - בדיוק כמו באקסס
            sb.Append("shovarhead.mspkod, shovarhead.manahovnum, shovarlines.mtfnum, shovarhead.shovarmsp");

            // שדות נוספים בהתאם לסוג הפעולה
            if ((actionType == 1 && isYearly) || actionType == 3)
            {
                sb.Append(", IIF(ISNULL(shovarhead.shovarmsp, 0) = 0, shovarhead.hskod, '0') AS miun");
            }
            if (actionType == 3)
            {
                sb.Append(", shovarhead.uniqnum");
            }
            if (actionType == 1 && isYearly)
            {
                sb.Append(", shovarhead.shnati");
            }

            // מעבר על כל השדות מטבלת mivnemtf
            bool hasActTeur = structure.Fields.Any(f => f.InName.Equals("actteur", StringComparison.OrdinalIgnoreCase));
            int boundOfArrays = hasActTeur ?
                structure.Fields.Max(f => f.RealSeder) + (structure.NumOfPerutFields * (structure.NumOfPerutLines - 1)) :
                structure.Fields.Max(f => f.RealSeder);

            var processedFields = new HashSet<string>(); // למנוע כפילויות

            foreach (var field in structure.Fields.OrderBy(f => f.RealSeder))
            {
                if (processedFields.Contains(field.InName))
                    continue;

                // דילוג על שדות מיוחדים
                if (field.InName.Contains("simanenu", StringComparison.OrdinalIgnoreCase) ||
                    field.InName.Contains("rek", StringComparison.OrdinalIgnoreCase))
                    continue;

                // טיפול בשדות פירוט
                if (field.InName.Equals("sugtssum", StringComparison.OrdinalIgnoreCase))
                {
                    for (int j = 1; j <= structure.NumOfPerutLines; j++)
                    {
                        sb.AppendLine($", shovarlines.sm{j}");
                        sb.AppendLine($", shovarlines.sugts{j}");
                        sb.AppendLine($", shovarlines.sugtsname{j}");
                        if (structure.NumOfPerutFields == 5)
                        {
                            sb.AppendLine($", shovarlines.simanenu{j}");
                        }
                        sb.AppendLine($", shovarlines.teur{j}");
                    }
                    continue;

                }

                // הוספת שדה רגיל
                var tablePrefix = GetTablePrefix(field.Recordset);
                if (field.InName.Equals("ktoveths", StringComparison.OrdinalIgnoreCase) &&
                    !processedFields.Contains("ktoveths2"))
                {
                    sb.Append($", {tablePrefix}.{field.InName} AS ktoveths2");
                    processedFields.Add("ktoveths2");
                    continue;
                }
                processedFields.Add(field.InName);
            }

            // חלק ה-FROM זהה לאקסס
            sb.Append(@" FROM ((shovarhead 
        INNER JOIN shovarlines ON shovarhead.shovar = shovarlines.shovar) 
        LEFT JOIN shovarheadnx ON shovarhead.shovar = shovarheadnx.shovar) 
        LEFT JOIN shovarheadDynamic ON shovarhead.shovar = shovarheadDynamic.shovar");

            // תנאי WHERE עם טיפול ב-NULL
            sb.Append(" WHERE shovarhead.mnt = @batchNumber")
              .Append(" AND (sndto < ")
              .Append("CASE WHEN ISNULL((SELECT PrintEmailMtf FROM param3), 0) = 0 THEN 3 ELSE 4 END")
              .Append(" OR ISNULL(shnati, 0) <> 0)");

            // תנאים נוספים עם טיפול בפרמטרים אופציונליים
            if (familyCode.HasValue)
                sb.Append(" AND shovarhead.mspkod = @familyCode");
            if (closureNumber.HasValue)
                sb.Append(" AND ISNULL(shovarhead.sgrnum, 0) = @closureNumber");
            if (voucherGroup.HasValue)
                sb.Append(" AND ISNULL(shovarhead.kvuzashovar, 0) = @voucherGroup");
            else
                sb.Append(" AND ISNULL(shovarhead.kvuzashovar, 0) = 0");

            // ORDER BY - מותאם לפי סוג הפעולה
            if (actionType == 1 && isYearly)
            {
                sb.Append(@" ORDER BY 
            shovarhead.nameinsvr, 
            shovarhead.mspkod, 
            IIF(ISNULL(shovarhead.shovarmsp, 0) = 0, shovarhead.hskod, '0'),
            shovarhead.kvuzashovar, 
            shovarlines.mtfnum, 
            IIF(ISNULL(shovarhead.shnati, 0) = 1, 1, 0)");
            }
            else if (actionType == 1 || actionType == 2)
            {
                sb.Append(" ORDER BY shovarhead.nameinsvr, shovarhead.mspkod, shovarhead.shovar, shovarlines.mtfnum");
            }
            else
            {
                sb.Append(@" ORDER BY 
            shovarhead.nameinsvr, 
            shovarhead.mspkod, 
            IIF(ISNULL(shovarhead.shovarmsp, 0) = 0, shovarhead.hskod, '0'), 
            shovarlines.mtfnum, 
            shovarhead.manahovnum");
            }

            return sb.ToString();
        }

        private static string GetTablePrefix(int source) => source switch
        {
            1 => "shovarhead",
            2 => "shovarlines",
            3 => "shovarheadnx",
            4 => "",
            5 => "shovarheadDynamic",
            _ => throw new ArgumentException($"Unknown source: {source}")
        };

    }
}
