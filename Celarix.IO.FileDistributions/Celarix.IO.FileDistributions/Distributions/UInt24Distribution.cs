using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileDistributions.Distributions
{
    public sealed class UInt24Distribution
    {
        private readonly UnderlyingDistribution distribution;
        public long this[int bucketIndex] => distribution[bucketIndex];

        public UInt24Distribution() => distribution = new UnderlyingDistribution(24);

        public void AddSample(byte a, byte b, byte c) => distribution.AddSample32((uint)((a << 16) | (b << 8) | c));

        public double GetMean() => distribution.GetMean();

        public string GetDataText(bool useCommaDelimiter = false) => distribution.GetDataText(useCommaDelimiter);
    }
}
