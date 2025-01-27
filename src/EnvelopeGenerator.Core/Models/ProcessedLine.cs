using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace EnvelopeGenerator.Core.Models
{
    public record ProcessedLine
    {
        public long Mspkod { get; init; }
        public long? ManaHovNum { get; init; }
        public string Miun { get; init; }
        public int? UniqNum { get; init; }
        public bool Shnati { get; init; }

        public ProcessedLine(DbDataReader reader)
        {
            Mspkod = reader.GetInt64(reader.GetOrdinal("mspkod"));
            var manahovNumOrdinal = reader.GetOrdinal("manahovnum");
            ManaHovNum = !reader.IsDBNull(manahovNumOrdinal) ? reader.GetInt64(manahovNumOrdinal) : null;
            Miun = reader.GetString(reader.GetOrdinal("miun"));
            var uniqNumOrdinal = reader.GetOrdinal("uniqnum");
            UniqNum = !reader.IsDBNull(uniqNumOrdinal) ? reader.GetInt32(uniqNumOrdinal) : null;
            Shnati = reader.GetBoolean(reader.GetOrdinal("shnati"));
        }
    }
}
