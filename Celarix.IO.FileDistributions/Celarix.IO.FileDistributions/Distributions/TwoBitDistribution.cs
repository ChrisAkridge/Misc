using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileDistributions.Distributions
{
    internal sealed class TwoBitDistribution
    {
        private readonly UnderlyingDistribution distribution;

        public long this[int bucketIndex] => distribution[bucketIndex];

        public TwoBitDistribution() => distribution = new UnderlyingDistribution(2);

        public void AddSample(byte sample)
        {
            if (sample > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(sample));
            }
            
            distribution.AddSample8OrFewer(sample);
        }

        public double GetMean() => distribution.GetMean();

        public string GetDataText(bool useCommaDelimiter = false) => distribution.GetDataText(useCommaDelimiter);
    }
}
