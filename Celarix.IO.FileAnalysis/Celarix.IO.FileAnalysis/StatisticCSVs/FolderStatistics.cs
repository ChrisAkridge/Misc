using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.StatisticCSVs
{
    public sealed class FolderStatistics
    {
        public string AbsolutePath { get; set; }
        public int PathLength => AbsolutePath.Length;
        public int ChildFolderCount { get; set; }
        public int ChildFileCount { get; set; }
        public long TotalSize { get; set; }
        public double AverageFileSize => (double)TotalSize / ChildFileCount;
    }
}
