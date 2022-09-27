using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileDistributions.Distributions
{
    internal sealed class FourBitDistribution
    {
        private readonly UnderlyingDistribution distribution;

        public long this[int bucketIndex] => distribution[bucketIndex];

        public FourBitDistribution() => distribution = new UnderlyingDistribution(4);

        public void AddSample(byte sample)
        {
            if (sample > 15) { throw new ArgumentOutOfRangeException(nameof(sample)); }

            distribution.AddSample8OrFewer(sample);
        }

        public double GetMean() => distribution.GetMean();

        public string GetDataText() => distribution.GetDataText();
    }
}
