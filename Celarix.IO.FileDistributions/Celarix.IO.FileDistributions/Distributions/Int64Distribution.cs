using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileDistributions.Distributions
{
    public sealed class Int64Distribution
    {
        private readonly UnderlyingDistribution distribution;

        public long this[int bucketIndex] => distribution[bucketIndex];

        public Int64Distribution() => distribution = new UnderlyingDistribution(64);

        public void AddSample(long sample) => distribution.AddSample64(unchecked((ulong)sample));

        public double GetMean() => distribution.GetMean();

        public string GetDataText() => distribution.GetDataText(i => (long)(ulong)i, s => s.ToString());
    }
}
