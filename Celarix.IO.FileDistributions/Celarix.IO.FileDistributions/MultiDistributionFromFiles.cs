using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.IO.FileDistributions.Distributions;

namespace Celarix.IO.FileDistributions
{
    internal sealed class MultiDistributionFromFiles
    {
        private IEnumerator<string> filePathEnumerator;
        private BinaryReader currentStream;

        private BooleanDistribution booleanDistribution = new();
        private TwoBitDistribution twoBitDistribution = new();
        private FourBitDistribution fourBitDistribution = new();
        private ByteDistribution byteDistribution = new();
        private SByteDistribution sByteDistribution = new();
        private Int16Distribution int16Distribution = new();
        private UInt16Distribution uint16Distribution = new();
        private Int32Distribution int32Distribution = new();
        private UInt32Distribution uint32Distribution = new();
        private Int64Distribution int64Distribution = new();
        private UInt64Distribution uInt64Distribution = new();
        private SingleDistribution singleDistribution = new();
        private DoubleDistribution doubleDistribution = new();
        private CharDistribution charDistribution = new();

        public MultiDistributionFromFiles(IEnumerable<string> filePaths)
        {
            filePathEnumerator = filePaths.GetEnumerator();
            filePathEnumerator.MoveNext();
            currentStream = new BinaryReader(File.OpenRead(filePathEnumerator.Current));
        }

        public void FillDistributions()
        {
            
        }

        private ulong? ReadUInt64()
        {
            // WYLO: okay, this all sucks.
            // We need Pri.LongPath, we need to skip BinaryReader because it's
            // little-endian by design. We need to build our own, make auto-properties
            // ...there's a lot. Plus all the graphics stuff. Ugh.
            // Two buffers for that Stream - one 4KB, one 8 bytes to actually fill
            // the samples.
            if (currentStream.BaseStream.Position == currentStream.BaseStream.Length)
            {
                if (!filePathEnumerator.MoveNext())
                {
                    return null;
                }
                currentStream.Dispose();
                currentStream = new BinaryReader(File.OpenRead(filePathEnumerator.Current));
            }
            
            return currentStream.ReadUInt64();
        }
    }
}
