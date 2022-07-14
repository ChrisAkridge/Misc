using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis
{
    public enum AnalysisPhase
    {
        Created,
        WritingStatisticCSVs,
        GeneratingFileLists,
        GeneratingFileHashes,
        CopyingFiles,
        DeduplicatingFiles,
        GeneratingPathFiles,
        AnalyzingFiles,
        Complete
    }

    public enum FileLocation
    {
        Input,
        Output
    }

    public enum TextMapKind
    {
        Default,
        AssemblyFile,
        CSharpSourceFile
    }
}
