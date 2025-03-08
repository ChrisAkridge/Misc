using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.GraphingPlayground
{
	internal static class Helpers
	{
		public static int GetSecondsAfterMidnight(DateOnly date, TimeOnly time)
		{
			var offset = date.GetTimezoneOffsetByDate();
			var offsetOfNextDay = date.AddDays(1).GetTimezoneOffsetByDate();
			var offsetBias = 0;

			if (offsetOfNextDay > offset)
			{
				// We've gone from UTC-4 to UTC-5, losing an hour
				offsetBias = -3600;
			}
			else if (offsetOfNextDay < offset)
			{
				// We've gone from UTC-5 to UTC-4, gaining an hour
				offsetBias = 3600;
			}

			return (int)time.ToTimeSpan().TotalSeconds + offsetBias;
		}

		// Adapted from https://stackoverflow.com/a/70905450/2709212
		public static ScottPlot.Color FromHSV(int h, int s, int v)
		{
			var rgb = new int[3];

			var baseColor = ((h + 60) % 360) / 120;
			var shift = ((h + 60) % 360) - ((120 * baseColor) + 60);
			var secondaryColor = (baseColor + (shift >= 0 ? 1 : -1) + 3) % 3;
			
			// Setting hue
			rgb[baseColor] = 255;
			rgb[secondaryColor] = (int)((MathF.Abs(shift) / 60f) * 255f);
			
			// Setting saturation
			for (var i = 0; i < 3; i++)
			{
				rgb[i] += (int)((255 - rgb[i]) * ((100 - s) / 100f));
			}
			
			// Setting value
			for (var i = 0; i < 3; i++)
			{
				rgb[i] -= (int)((rgb[i] * (100 - v)) / 100f);
			}

			return ScottPlot.Color.FromARGB(0xFF000000u | (uint)(rgb[0] << 16) | (uint)(rgb[1] << 8) | (uint)rgb[2]);
		}

		public static ScottPlot.Color[] GeneratePaletteOfEvenSpacing(int count)
		{
			var palette = new ScottPlot.Color[count];
			for (var i = 0; i < count; i++)
			{
				var hue = (int)((360f / count) * i);
				palette[i] = FromHSV(hue, 100, 100);
			}

			return palette;
		}
	}
}
