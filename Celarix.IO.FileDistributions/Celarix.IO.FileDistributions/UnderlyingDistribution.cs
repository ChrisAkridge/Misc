using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileDistributions
{
    internal sealed class UnderlyingDistribution
    {
        private const int TotalBucketCount = 2048;

        private readonly long[] buckets;
        private BigInteger sum;
        private long samples;
        
        public int BitsPerSample { get; }
        public BigInteger PossibleValues => BigInteger.Pow(2, BitsPerSample);
        public bool UsesBuckets => PossibleValues > TotalBucketCount;
        public BigInteger BucketSize => PossibleValues / TotalBucketCount;
        
        public long this[int bucketIndex] => buckets[bucketIndex];

        public UnderlyingDistribution(int bitsPerSample)
        {
            BitsPerSample = bitsPerSample;
            buckets = new long[UsesBuckets ? TotalBucketCount : (int)PossibleValues];
        }

        public void AddSample8OrFewer(byte sample)
        {
            var mask = (1 << BitsPerSample) - 1;
            sample = (byte)(sample & mask);
            buckets[sample] += 1;
            sum += sample;
            samples += 1;
        }

        public void AddSample16(ushort sample)
        {
            var bucketIndex = (int)(sample / BucketSize);
            buckets[bucketIndex] += 1;
            sum += sample;
            samples += 1;
        }

        public void AddSample32(uint sample)
        {
            var bucketIndex = (int)(sample / BucketSize);
            buckets[bucketIndex] += 1;
            sum += sample;
            samples += 1;
        }

        public void AddSample64(ulong sample)
        {
            var bucketIndex = (int)(sample / BucketSize);
            buckets[bucketIndex] += 1;
            sum += sample;
            samples += 1;
        }

        public void AddLargerSample(BigInteger sample)
        {
            var bucketIndex = (int)(sample / BucketSize);
            buckets[bucketIndex] += 1;
            sum += sample;
            samples += 1;
        }

        public double GetMean() => (double)sum / samples;

        public string GetDataText()
        {
            var builder = new StringBuilder();
            for (var i = 0; i < buckets.Length; i++)
            {
                var bucket = buckets[i];
                builder.AppendLine($"{GetBucketRangeText(i)}: {bucket}");
            }

            builder.AppendLine($"Mean: {GetMean():F4}");
            return builder.ToString();
        }

        public string GetDataText<TBucket>(Func<BigInteger, TBucket> bucketValueFunc, Func<TBucket, string> valueFormatter)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < buckets.Length; i++)
            {
                var bucket = buckets[i];
                
                if (UsesBuckets)
                {
                    var bucketValueLow = bucketValueFunc(i * BucketSize);
                    var bucketValueHigh = bucketValueFunc(((i + 1) * BucketSize) - 1);
                    builder.AppendLine($"{valueFormatter(bucketValueLow)} to {valueFormatter(bucketValueHigh)}: {bucket}");
                }
                else
                {
                    var bucketValue = bucketValueFunc(i);
                    builder.AppendLine($"{valueFormatter(bucketValue)}: {bucket}");
                }
            }

            var mean = GetMean();
            var integerMean = new BigInteger(mean);
            builder.AppendLine($"Mean: {valueFormatter(bucketValueFunc(integerMean))}");
            return builder.ToString();
        }

        private string GetBucketRangeText(int bucketIndex)
        {
            if (!UsesBuckets)
            {
                return bucketIndex.ToString();
            }

            var bucketLowValue = bucketIndex * BucketSize;
            var bucketHighValue = ((bucketIndex + 1) * BucketSize) - 1;
            return $"{bucketLowValue} to {bucketHighValue}";
        }
    }
}
