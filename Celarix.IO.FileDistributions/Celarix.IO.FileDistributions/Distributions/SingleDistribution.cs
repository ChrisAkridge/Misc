using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileDistributions.Distributions
{
    public sealed class SingleDistribution
    {
        private readonly UnderlyingDistribution distribution;

        public long this[int bucketIndex] => distribution[bucketIndex];

        public SingleDistribution() => distribution = new UnderlyingDistribution(32);

        public void AddSample(float sample)
        {
            var sampleBits = BitConverter.SingleToUInt32Bits(sample);
            distribution.AddSample32(sampleBits);
        }

        public double GetMean() => distribution.GetMean();

        public string GetDataText(bool useCommaDelimiter = false) =>
            distribution.GetDataText(i => BitConverter.UInt32BitsToSingle((uint)i), s => s.ToString(CultureInfo.InvariantCulture), useCommaDelimiter);
    }
}
