using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileDistributions.Distributions
{
    public sealed class Int32Distribution
    {
        private readonly UnderlyingDistribution distribution;

        public long this[int bucketIndex] => distribution[bucketIndex];

        public Int32Distribution() => distribution = new UnderlyingDistribution(32);

        public void AddSample(int sample) => distribution.AddSample32(unchecked((uint)sample));

        public double GetMean() => distribution.GetMean();

        public string GetDataText() =>
            distribution.GetDataText(i => (int)(uint)i, s => s.ToString());
    }
}
