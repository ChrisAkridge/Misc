using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis
{
    public abstract class JobPhase
    {
        public AnalysisJob AnalysisJob { get; set; }
        public abstract AnalysisPhase AnalysisPhase { get; }
        public abstract void StartOrResume(AnalysisJob job);
    }
}
