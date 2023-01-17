using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileDistributions.Distributions
{
    public sealed class Int16Distribution
    {
        private readonly UnderlyingDistribution distribution;

        public long this[int bucketIndex] => distribution[bucketIndex];

        public Int16Distribution() => distribution = new UnderlyingDistribution(16);

        public void AddSample(short sample) => distribution.AddSample16(unchecked((ushort)sample));

        public double GetMean() => distribution.GetMean();

        public string GetDataText(bool useCommaDelimiter = false) =>
            distribution.GetDataText(i =>
            {
                unchecked { return (short)(int)i; }
            }, s => s.ToString(), useCommaDelimiter);
    }
}
