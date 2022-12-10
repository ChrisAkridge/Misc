using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileDistributions.Distributions
{
    public sealed class CharDistribution
    {
        private readonly UnderlyingDistribution distribution;

        public long this[int bucketIndex] => distribution[bucketIndex];

        public CharDistribution() => distribution = new UnderlyingDistribution(16);

        public void AddSample(char sample) => distribution.AddSample16(sample);

        public double GetMean() => distribution.GetMean();

        public string GetDataText(bool useCommaDelimiter = false) =>
            distribution.GetDataText(i => (char)(ushort)i, c => c.ToString(), useCommaDelimiter);
    }
}
