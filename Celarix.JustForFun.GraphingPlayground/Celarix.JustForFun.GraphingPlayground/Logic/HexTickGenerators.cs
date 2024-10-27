using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.GraphingPlayground.Logic
{
	internal static class HexTickGenerators
	{
		public static string Sub256TickFormatter(double position)
		{
			if (position is < int.MinValue or > int.MaxValue) { return "OoB"; }

			var intPosition = (int)position;
			var hexPosition = $"0x{intPosition:X2}";

			if (intPosition is < 0x20 or > 0x7E) { return hexPosition; }

			var asciiChar = (char)intPosition;

			return $"{hexPosition} '{asciiChar}'";
		}

		public static Func<double, string> BucketedTickFormatterGenerator(int bucketSize)
		{
			return position =>
			{
				if (position is < int.MinValue or > int.MaxValue) { return "OoB"; }

				var intPosition = (int)position;
				var bucketLow = intPosition * bucketSize;
				var bucketHigh = (bucketLow + bucketSize) - 1;

				return $"0x{bucketLow:X2} - 0x{bucketHigh:X2}";
			};
		}

		public static Func<double, string> HeatmapHighPartTickFormatterGenerator(int distributionCount)
		{
			return xPosition =>
			{
				var bitCount = (int)Math.Log2(distributionCount);
				var hexDigitCountPerPart = bitCount / 8;
				var maxAllowedCount = (1 << (bitCount / 2)) - 1;
				if (xPosition < 0 || xPosition > maxAllowedCount) { return "OoB"; }

				return HeatmapHighPart((ulong)xPosition, hexDigitCountPerPart);
			};
		}
		
		public static Func<double, string> HeatmapLowPartTickFormatterGenerator(int distributionCount)
		{
			return xPosition =>
			{
				var bitCount = (int)Math.Log2(distributionCount);
				var hexDigitCountPerPart = bitCount / 8;
				var maxAllowedCount = (1 << bitCount) - 1;
				if (xPosition < 0 || xPosition > maxAllowedCount) { return "OoB"; }
				
				return HeatmapLowPart((ulong)xPosition, hexDigitCountPerPart);
			};
		}

		public static Func<double, string> BucketedHeatmapHighPartTickFormatterGenerator(int spaceSizeInBits,
			int bucketSizeInBits)
		{
			return yPosition =>
			{
				// Okay, so this is getting complicated. Let's recap:
				// - Files are made of bytes in the range 0x00 to 0xFF
				// - You can split or group these bytes in any way you like, but we only consider splitting
				//   into 1, 2, or 4 bits, leaving them as 8-bit bytes, or grouping them into 16-bit,
				//   24-bit, 32-bit, 64-bit, or 128-bit integers
				// - We can count the number of these splits or groupings in a distribution, which can then
				//   be displayed as a bar chart where the X-axis is that value and the Y-axis is how
				//   many times that value appears in the distribution.
				// - But for really large distributions, we can't display all the values on the X-axis,
				//   so we instead bucket them. Buckets are always sized as a power of 2, which splits
				//   the space of all possible values into power-of-2-sized chunks. For example, if we're
				//   considering the 32-bit space and our bucket size is 20 bits, then we have 2^(32 - 20)
				//   = 4096 buckets of size 2^20 = 1048576. Any value within the bucket is included in
				//   that bucket.
				// - Buckets have a low end and a high end, as well as a number. In the example, bucket
				//   #0 goes from 0 to (2^20 - 1), or 1,048,575. Bucket #1 goes from 1,048,576 to 2,097,151.
				//   And so forth.
				// - Heatmaps are a way to compress the bar graph into 3 dimensions. Think of it like a 2D grid
				//   of bars where their heights are just along a color gradient where the minimum is dark blue
				//   and the maximum is bright yellow.
				// - We can make a square heat map by taking the square root of the total number of possible
				//   values. For 8-bit space, that's sqrt(256) = 16, so the heatmap would be 16 by 16 in size.
				// - We then need to set the axis markers. The value of each heatmap cell increases
				//   by 1 left-to-right and by sqrt(x) top-to-bottom. For 8-bit space, that means the
				//   X-axis goes from 0 to 15 and the Y-axis goes from 0 to 15, each step on the Y-axis
				//   being +/-16 values.
				// - The tick marks then have a "low part", which goes up by one, and a "high part",
				//   which goes up by 16. All cells on the same column have the same low part, and all
				//   cells on the same row have the same high part. Thus, we can display low parts as
				//   "x0, x1, x2..." and high parts as "0x, 1x, 2x...".
				// - The code for that is easy and already done. But heatmaps are still not good for
				//   very large spaces. The 32-bit space would result in a 65,536 by 65,536 heatmap,
				//   which is way too big (32 GB of memory, when using doubles!). So we need to make
				//   a bucketed heatmap where BOTH axes are bucketed, and bucketed separately.
				// - Let's take the same example as before, using the 32-bit space of 2^32 values and
				//   a bucket size of 2^20. Actually, let's pick 2^10 on each axis as our bucket size,
				//   since each axis only has 2^16 < 2^20 possible values.
				// - So each axis has (2^16 - 2^10) = 2^6 = 64 buckets, for a total of (2^6)^2 = 2^12
				//   = 4,096 cells in the heatmap. Each cell represents a range, but in sort of a strange
				//   way. Let's take a single 32-bit value: 0x3FFF7FFF. This value, in an unbucketed heat map,
				//   would go at x = 0x7FFF, y = 0x3FFF. But due to the bucketing, it can be thought of as
				//   combining each 1024x1024 range of cells into a single such cell, using the highest
				//   6 bits of each half of the value to determine which combined cell to put it in.
				//   So we basically 0 out the bottom 10 bits of each half (x = 0x7C, y = 0x3C), which
				//   becomes the two bucket numbers we use for this value.
				// - The buckets themselves are still a range, and we want to display this range in the
				//   tick marks. Along the X-axis, each cell is a step by the bucket size, where it was
				//   1 before. So bucket (0, 0) is the range from 0x00000000 to 0x000003FF. The same idea
				//   applies for the Y-axis, except in the upper 16 bits. So bucket (0, 1) would be from
				//   0x03FF0000 to 0x03FF03FF. Really, each cell represents a two-dimensional range with
				//   four values: The bucket (0, 0) represents all values where EITHER 16-bit part is
				//   in the range 0x0000 to 0x03FF.
				// - And that gives us an easy way to think about all this. We need to know or compute
				//   these variables:
				//	 - The size of the space in bits (32 for 32-bit space)
				//   - Half of that, which would be the step size for a single Y-axis step if this were
				//     unbucketed. (16 for 32-bit space)
				//   - The bucket size in bits, which we'll take as a single value (20 if we want 4,096
				//     cells total).
				//   - Half the bucket size, to figure out the per-axis bucket size (10, so each axis
				//     is divided into 64 buckets).
				// - The xPosition and yPosition variables are provided by ScottPlot and are in the range
				//   of 0 to (bucketCount - 1). For our 10-bit-per-axis example, that'd be 0 to 63, or 6
				//   bits of values. So we just need to cast the position to an integer, shift it left
				//   by half the bucket size in bits, and that'd be the low end of the bucket on this
				//   axis. Then take (1 << halfBucketSizeInBits) (in this example, (1 << 10) = 1024),
				//   subtract 1 from it, then OR it with the low end to get the high end.
				// - FINALLY, we just have to either put 'x's on the low half or the high half of the
				//   overall low end and high end strings, based on which half we're computing.
				// - Sanity check.
				//   - spaceSizeInBits = 128
				//   - halfSpaceSizeInBits = 64
				//   - bucketSizeInBits = 108
				//   - halfBucketSizeInBits = 54
				//   - xPosition = 1023
				//   - lowEnd = (1023 << 54) = 0xFFC00000_00000000, which needs a ulong to fit, but that's fine
				//   - bucketSizeOnAxis = (1 << 54) = 0x00400000_00000000
				//   - highEnd = lowEnd + (bucketSizeOnAxis - 1) = 0xFFC3FFFFF_FFFFFFFF
				//   - Then the overall result would either be
				//     - If we're computing the high half, "FFC0000000000000xxxxxxxxxxxxxxxx to FFC3FFFFFFFFFFFFFxxxxxxxxxxxxxxxx"
				//     - If we're computing the low half,  "xxxxxxxxxxxxxxxxFFC0000000000000 to xxxxxxxxxxxxxxxxFFC3FFFFFFFFFFFFF"
				// OKAY. LET'S ACTUALLY DO THIS NOW.

				var bucketNumber = (int)yPosition;
				var halfBucketSizeInBits = bucketSizeInBits / 2;
				var bucketSizeOnAxis = 1UL << halfBucketSizeInBits;
				if (bucketNumber < 0 || (ulong)bucketNumber >= bucketSizeOnAxis) { return "OoB"; }

				var halfSpaceSizeInBits = spaceSizeInBits / 2;
				var hexDigitsPerPart = halfSpaceSizeInBits / 4;
				var lowEnd = (ulong)(bucketNumber) << halfBucketSizeInBits;
				var highEnd = (lowEnd + (bucketSizeOnAxis)) - 1;

				var lowEndString = HeatmapHighPart(lowEnd, hexDigitsPerPart);
				var highEndString = HeatmapHighPart(highEnd, hexDigitsPerPart);

				return $"{lowEndString} - {highEndString}";
			};
		}

		public static Func<double, string> BucketedHeatmapLowPartTickFormatterGenerator(int spaceSizeInBits,
			int bucketSizeInBits)
		{
			return xPosition =>
			{
				// Okay, so this is getting complicated... wait, I'm not writing that all again.

				var bucketNumber = (int)xPosition;
				var halfBucketSizeInBits = bucketSizeInBits / 2;
				var bucketSizeOnAxis = 1UL << halfBucketSizeInBits;
				if (bucketNumber < 0 || (ulong)bucketNumber >= bucketSizeOnAxis) { return "OoB"; }

				var halfSpaceSizeInBits = spaceSizeInBits / 2;
				var hexDigitsPerPart = halfSpaceSizeInBits / 4;
				var lowEnd = (ulong)(bucketNumber) << halfBucketSizeInBits;
				
				var highEnd = (lowEnd + (bucketSizeOnAxis)) - 1;

				var lowEndString = HeatmapLowPart(lowEnd, hexDigitsPerPart);
				var highEndString = HeatmapLowPart(highEnd, hexDigitsPerPart);

				return $"{lowEndString} - {highEndString}";
			};
		}

		private static string HeatmapHighPart(ulong value, int hexDigitCountPerPart)
		{
			var highPart = value.ToString("X" + hexDigitCountPerPart)[..hexDigitCountPerPart];
			var lowPart = new string('x', hexDigitCountPerPart);

			return $"{highPart}{lowPart}";
		}
		
		private static string HeatmapLowPart(ulong value, int hexDigitCountPerPart)
		{
			var highPart = new string('x', hexDigitCountPerPart);
			var lowPart = value.ToString("X" + hexDigitCountPerPart);

			return $"{highPart}{lowPart}";
		}
	}
}
