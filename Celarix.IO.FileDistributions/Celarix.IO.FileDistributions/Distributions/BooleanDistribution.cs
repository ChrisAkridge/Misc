using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileDistributions.Distributions
{
    internal sealed class BooleanDistribution
    {
        private readonly UnderlyingDistribution distribution;

        public long this[int bucketIndex] => distribution[bucketIndex];

        public BooleanDistribution() => distribution = new UnderlyingDistribution(1);

        public void AddSample(bool sample) => distribution.AddSample8OrFewer((byte)(sample ? 1 : 0));

        public double GetMean() => distribution.GetMean();

        public string GetDataText(bool useCommaDelimiter = false) => distribution.GetDataText(useCommaDelimiter);
    }
}
