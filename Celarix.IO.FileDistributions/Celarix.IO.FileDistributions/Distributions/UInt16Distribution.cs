using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileDistributions.Distributions
{
    public sealed class UInt16Distribution
    {
        private readonly UnderlyingDistribution distribution;

        public long this[int bucketIndex] => distribution[bucketIndex];

        public UInt16Distribution() => distribution = new UnderlyingDistribution(16);

        public void AddSample(ushort sample) => distribution.AddSample16(sample);

        public double GetMean() => distribution.GetMean();

        public string GetDataText(bool useCommaDelimiter = false) =>
            distribution.GetDataText(i =>
            {
                unchecked { return (ushort)(int)i; }
            }, s => s.ToString(), useCommaDelimiter);
    }
}
