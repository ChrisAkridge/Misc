using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileDistributions.Distributions
{
    public sealed class UInt64Distribution
    {
        private readonly UnderlyingDistribution distribution;

        public long this[int bucketIndex] => distribution[bucketIndex];

        public UInt64Distribution() => distribution = new UnderlyingDistribution(64);

        public void AddSample(ulong sample) => distribution.AddSample64(sample);

        public double GetMean() => distribution.GetMean();

        public string GetDataText() => distribution.GetDataText(i => (ulong)i, s => s.ToString());
    }
}
