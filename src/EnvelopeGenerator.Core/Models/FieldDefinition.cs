using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvelopeGenerator.Core.Models
{
    public class EnvelopeField
    {
        public string InName { get; init; } = string.Empty;     // שם השדה מ-mivnemtf
        public int FldType { get; init; }                       // סוג השדה (1=טקסט, 2=מספר, 3=מטבע)
        public int Length { get; init; }                        // אורך השדה
        public int RealSeder { get; init; }                    // סדר השדה במעטפית
        public int Recordset { get; init; }                    // מקור השדה (1=shovarhead, 2=shovarlines, 3=shovarheadnx וכו')
        public bool Show { get; init; }                        // האם להציג את השדה
        public bool NotInMtf { get; init; }                    // האם לא לכלול במעטפית
    }
}
