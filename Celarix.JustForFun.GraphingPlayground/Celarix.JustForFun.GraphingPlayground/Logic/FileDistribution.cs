using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.GraphingPlayground.Logic
{
	internal sealed class FileDistribution
	{
		private long[] oneBitDistribution;
		private long[] twoBitDistribution;
		private long[] fourBitDistribution;
		private long[] eightBitDistribution;
		private long[] sixteenBitDistribution;
		
		public IReadOnlyList<long> OneBitDistribution => oneBitDistribution;
		public IReadOnlyList<long> TwoBitDistribution => twoBitDistribution;
		public IReadOnlyList<long> FourBitDistribution => fourBitDistribution;
		public IReadOnlyList<long> EightBitDistribution => eightBitDistribution;
		public IReadOnlyList<long> SixteenBitDistribution => sixteenBitDistribution;
		
		public FileDistribution(string filePath)
		{
			var buffer = new byte[1024];
			using var fileStream = File.OpenRead(filePath);

			oneBitDistribution = new long[2];
			twoBitDistribution = new long[4];
			fourBitDistribution = new long[16];
			eightBitDistribution = new long[256];
			sixteenBitDistribution = new long[65536];

			int read;

			do
			{
				read = fileStream.Read(buffer, 0, buffer.Length);

				for (int i = 0; i < read; i += 16)
				{
					ulong hi = BitConverter.ToUInt64(buffer, i);
					ulong lo = BitConverter.ToUInt64(buffer, i + 8);

					CountBits(hi);
					CountBits(lo);
					CountBitPairs(hi);
					CountBitPairs(lo);
					CountNybbles(hi);
					CountNybbles(lo);
					CountBytes(hi);
					CountBytes(lo);
					CountShorts(hi);
					CountShorts(lo);
				}
			} while (read != 0);
		}

		public double[,] BucketByTotalDistribution(IReadOnlyList<long> distribution, int bucketSize)
		{
			// Man all these extra arrays kinda suck
			var bucketSizeInBits = (int)Math.Log2(bucketSize);
			var halfBucketSizeInBits = bucketSizeInBits / 2;
			var halfBucketSize = 1 << halfBucketSizeInBits;
			var buckets = new long[halfBucketSize, halfBucketSize];
			
			for (int i = 0; i < distribution.Count; i++)
			{
				var x = (i & (bucketSize - 1)) >> halfBucketSizeInBits;
				var y = i >> (bucketSizeInBits + halfBucketSizeInBits);
				buckets[x, y] += distribution[i];
			}
			
			var doubleBuckets = new double[halfBucketSize, halfBucketSize];
			for (int x = 0; x < halfBucketSize; x++)
			{
				for (int y = 0; y < halfBucketSize; y++)
				{
					doubleBuckets[x, y] = buckets[x, y];
				}
			}

			return doubleBuckets;
		}

		private void CountBits(ulong value)
		{
			var oneBits = (long)ulong.PopCount(value);
			oneBitDistribution[1] += oneBits;
			oneBitDistribution[0] += 64 - oneBits;
		}

		private void CountBitPairs(ulong value)
		{
			for (int b = 0; b < 64; b += 2)
			{
				ulong mask = 0b11UL << b;
				ulong masked = (value & mask) >> b;
				twoBitDistribution[masked] += 1;
			}
		}

		private void CountNybbles(ulong value)
		{
			for (int b = 0; b < 64; b += 4)
			{
				ulong mask = 0xFUL << b;
				ulong masked = (value & mask) >> b;
				fourBitDistribution[masked] += 1;
			}
		}

		private void CountBytes(ulong value)
		{
			for (int b = 0; b < 64; b += 8)
			{
				ulong mask = 0xFFUL << b;
				ulong masked = (value & mask) >> b;
				eightBitDistribution[masked] += 1;
			}
		}

		private void CountShorts(ulong value)
		{
			for (int b = 0; b < 64; b += 16)
			{
				ulong mask = 0xFFFFUL << b;
				ulong masked = (value & mask) >> b;
				sixteenBitDistribution[masked] += 1;
			}
		}
	}
}
