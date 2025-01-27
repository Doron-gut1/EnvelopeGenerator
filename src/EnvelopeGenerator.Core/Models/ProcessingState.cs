using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvelopeGenerator.Core.Models
{
    public class ProcessingState
    {
        public string PreviousHskod { get; set; } = "0";
        public long PreviousMspkod { get; set; }
        public bool IsPreviousHov { get; set; } = true;
        public bool IsPreviousTkufati { get; set; }
        public int LineCounter { get; set; } = 1;
        public string LastTkufatiLine { get; set; } = string.Empty;
        public string LastHovLine { get; set; } = string.Empty;

        public void UpdateState(ProcessedLine data)
        {
            PreviousHskod = data.Miun;
            PreviousMspkod = data.Mspkod;
        }
    }
}
