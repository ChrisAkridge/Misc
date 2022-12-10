using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileDistributions.Distributions
{
    public class ByteDistribution
    {
        private readonly UnderlyingDistribution distribution;

        public long this[int bucketIndex] => distribution[bucketIndex];

        public ByteDistribution() => distribution = new UnderlyingDistribution(8);

        public void AddSample(byte sample) => distribution.AddSample8OrFewer(sample);

        public double GetMean() => distribution.GetMean();

        public string GetDataText(bool useCommaDelimiter = false) => distribution.GetDataText(useCommaDelimiter);
    }
}
