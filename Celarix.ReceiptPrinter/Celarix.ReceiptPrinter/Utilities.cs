using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.ReceiptPrinter
{
    internal static class Utilities
    {
        public static string GetProgress(int numerator, int divisor, int width)
        {
            if (width <= 2)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Width must be greater than 2.");
            }

            var portion = (double)numerator / divisor;
            var totalSegments = width - 2;
            var expandedSegments = totalSegments * 10;
            var filledExpandedSegments = (int)Math.Round(expandedSegments * portion);
            var filledSegments = filledExpandedSegments / 10;
            var leftoverSegment = filledExpandedSegments % 10;

            var builder = new StringBuilder(width);
            builder.Append('[');
            for (int i = 0; i < filledSegments; i++)
            {
                builder.Append('#');
            }

            if (leftoverSegment > 0)
            {
                builder.Append((char)('0' + leftoverSegment));
            }
            else
            {
                builder.Append('.');
            }

            for (int i = filledSegments + 1; i < totalSegments; i++)
            {
                builder.Append('.');
            }
            builder.Append(']');

            return builder.ToString();
        }
    }
}
