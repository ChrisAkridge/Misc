using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileDistributions.Distributions
{
    public sealed class DoubleDistribution
    {
        private readonly UnderlyingDistribution distribution;

        public long this[int bucketIndex] => distribution[bucketIndex];

        public DoubleDistribution() => distribution = new UnderlyingDistribution(64);

        public void AddSample(double sample)
        {
            var sampleBits = BitConverter.DoubleToUInt64Bits(sample);
            distribution.AddSample64(sampleBits);
        }

        public double GetMean() => distribution.GetMean();

        public string GetDataText(bool useCommaDelimiter = false) =>
            distribution.GetDataText(i => BitConverter.UInt64BitsToDouble((ulong)i),
                s => s.ToString(CultureInfo.InvariantCulture), useCommaDelimiter);
    }
}
