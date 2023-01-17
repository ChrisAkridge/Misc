using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileDistributions.Distributions
{
    public class UInt32Distribution
    {
        private readonly UnderlyingDistribution distribution;

        public long this[int bucketIndex] => distribution[bucketIndex];

        public UInt32Distribution() => distribution = new UnderlyingDistribution(32);

        public void AddSample(uint sample) => distribution.AddSample32(sample);

        public double GetMean() => distribution.GetMean();

        public string GetDataText(bool useCommaDelimiter = false) => distribution.GetDataText(i => (uint)(i & uint.MaxValue), s => s.ToString(), useCommaDelimiter);
    }
}
