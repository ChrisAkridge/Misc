using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII.FileDistributions
{
	public sealed class BucketedDistribution<T>
		where T : struct
	{
		private const int BucketCount = 4096;
		
		private readonly int[] buckets = new int[BucketCount];
		private readonly T max;
		
		// Okay after making separate types for UInt64 and UInt128 because they don't support
		// the specific division-by-int interface, I'm just going to go the lazy route.
		private readonly Func<T> maxValueFunc;
		private readonly Func<T, int, T> divideByInt32Func;
		private readonly Func<T, T, T> divideFunc;
		private readonly Func<T, T, bool> greaterThanFunc;
		private readonly Func<T, int> createOverflowFunc;
		private readonly Func<T, T, T> addFunc;
		private readonly Func<T> oneFunc;

		public T BucketSize { get; }

		public BucketedDistribution(Func<T> maxValueFunc,
			Func<T, int, T> divideByInt32Func,
			Func<T, T, bool> greaterThanFunc,
			Func<T, int> createOverflowFunc,
			Func<T, T, T> divideFunc,
			Func<T, T, T> addFunc,
			Func<T> oneFunc)
		{
			this.maxValueFunc = maxValueFunc;
			this.divideByInt32Func = divideByInt32Func;
			this.greaterThanFunc = greaterThanFunc;
			this.createOverflowFunc = createOverflowFunc;
			this.divideFunc = divideFunc;
			this.addFunc = addFunc;
			this.oneFunc = oneFunc;
			max = maxValueFunc();
			BucketSize = addFunc(divideByInt32Func(max, BucketCount), oneFunc());
		}
		
		public BucketedDistribution(T max,
			Func<T> maxValueFunc,
			Func<T, int, T> divideByInt32Func,
			Func<T, T, bool> greaterThanFunc,
			Func<T, int> createOverflowFunc,
			Func<T, T, T> divideFunc,
			Func<T, T, T> addFunc,
			Func<T> oneFunc)
		{
			this.max = max;
			this.maxValueFunc = maxValueFunc;
			this.divideByInt32Func = divideByInt32Func;
			this.greaterThanFunc = greaterThanFunc;
			this.createOverflowFunc = createOverflowFunc;
			this.divideFunc = divideFunc;
			this.addFunc = addFunc;
			this.oneFunc = oneFunc;
			BucketSize = addFunc(divideByInt32Func(max, BucketCount), oneFunc());
		}

		public void Add(T value)
		{
			if (greaterThanFunc(value, max))
			{
				throw new ArgumentOutOfRangeException(nameof(value), "Value is outside the range of this distribution.");
			}
			
			var bucketIndex = createOverflowFunc(divideFunc(value, BucketSize));
			buckets[bucketIndex]++;
		}
	}
}
