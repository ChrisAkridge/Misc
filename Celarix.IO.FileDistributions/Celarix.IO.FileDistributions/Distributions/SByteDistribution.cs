using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileDistributions.Distributions
{
    public class SByteDistribution
    {
        private readonly UnderlyingDistribution distribution;

        public long this[int bucketIndex] => distribution[bucketIndex];

        public SByteDistribution() => distribution = new UnderlyingDistribution(8);

        public void AddSample(sbyte sample) => distribution.AddSample8OrFewer(unchecked((byte)sample));

        public double GetMean() => distribution.GetMean();

        public string GetDataText(bool useCommaDelimiter = false) => distribution.GetDataText(i =>
        {
            unchecked { return (sbyte)(int)i; }
        }, s => s.ToString(), useCommaDelimiter);
    }
}
