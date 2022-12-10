using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.PostProcessing
{
    internal sealed class TextMapColumn
    {
        public int Width { get; set; }
        public long X { get; set; }
        public StorageBackedBitArrayList.StorageBackedBitArrayBatch LineMaps { get; set; }
        public double StartHue { get; set; }
        public double EndHue { get; set; }
        public int LineMapCount { get; set; }

        public double GetHueForLine(int lineIndex)
        {
            var hueStep = (EndHue - StartHue) / LineMapCount;
            return StartHue + (lineIndex * hueStep);
        }
    }
}
