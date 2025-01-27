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

            // השדות הבסיסיים בדיוק כמו באקסס
            sb.Append("shovarhead.mspkod, shovarhead.manahovnum, shovarlines.mtfnum, shovarhead.shovarmsp");

            // שדות נוספים לפי סוג הפעולה
            if ((actionType == 1 && isYearly) || actionType == 3)
            {
                sb.Append(", IIF(Nz(shovarhead.shovarmsp, 0) = 0, shovarhead.hskod, '0') As miun");
            }
            if (actionType == 3)
            {
                sb.Append(", shovarhead.uniqnum");
            }
            if (actionType == 1 && isYearly)
            {
                sb.Append(", shovarhead.shnati");
            }

            // הוספת שדות דינמית לפי מבנה ה-mivnemtf
            foreach (var field in structure.Fields.OrderBy(f => f.RealSeder))
            {
                // דילוג על שדות מיוחדים
                if (field.InName.Contains("simanenu", StringComparison.OrdinalIgnoreCase) ||
                    field.InName.Contains("rek", StringComparison.OrdinalIgnoreCase))
                    continue;

                // טיפול מיוחד בשדות פירוט
                if (field.InName.Equals("sugtssum", StringComparison.OrdinalIgnoreCase))
                {
                    for (int i = 1; i <= structure.NumOfPerutLines; i++)
                    {
                        sb.AppendLine($", shovarlines.sm{i}, shovarlines.sugts{i}, shovarlines.sugtsname{i}, shovarlines.teur{i}");
                    }
                    continue;
                }

                // הוספת שדה רגיל
                var tablePrefix = GetTablePrefix(field.Recordset);
                sb.Append($", {tablePrefix}.{field.InName}");
            }

            // הוספת חלק ה-FROM בדיוק כמו באקסס
            sb.Append(@" FROM ((shovarhead 
            INNER JOIN shovarlines ON shovarhead.shovar = shovarlines.shovar) 
            LEFT JOIN shovarheadnx ON shovarhead.shovar = shovarheadnx.shovar)
            LEFT JOIN shovarheadDynamic ON shovarhead.shovar = shovarheadDynamic.shovar");

            // תנאי WHERE בדיוק כמו באקסס
            sb.Append(" WHERE shovarhead.mnt = @batchNumber")
              .Append(" AND (sndto < CASE WHEN ISNULL((SELECT PrintEmailMtf FROM param3), 0) = 0 THEN 3 ELSE 4 END OR shnati <> 0)");

            if (familyCode.HasValue)
                sb.Append(" AND shovarhead.mspkod = @familyCode");
            if (closureNumber.HasValue)
                sb.Append(" AND shovarhead.sgrnum = @closureNumber");
            if (voucherGroup.HasValue)
                sb.Append(" AND shovarhead.kvuzashovar = @voucherGroup");
            else
                sb.Append(" AND shovarhead.kvuzashovar = 0");

            // ORDER BY בדיוק כמו באקסס
            if (actionType == 1 && isYearly)
            {
                sb.Append(@" ORDER BY 
                shovarhead.nameinsvr, 
                shovarhead.mspkod, 
                IIF(Nz(shovarhead.shovarmsp, 0) = 0, shovarhead.hskod, '0'),
                shovarhead.kvuzashovar, 
                shovarlines.mtfnum, 
                IIF(shovarhead.shnati = True, 1, 0)");
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
                IIF(Nz(shovarhead.shovarmsp, 0) = 0, shovarhead.hskod, '0'), 
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
