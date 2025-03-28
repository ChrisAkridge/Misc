using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.GraphingPlayground.Logic
{
	internal static class CommonTickGenerators
	{
		public static string SecondsAfterMidnightFormatter(double position)
		{
			if (position is < 0d or > 86400d)
			{
				return "OoB";
			}

			var timeSpan = TimeSpan.FromSeconds(position);
			var time = TimeOnly.FromTimeSpan(timeSpan);
			return time.ToString();
		}

		public static string MonthAndDayFromDayNumber(double position)
		{
			if (position is < 0d or > 366d)
			{
				return "OoB";
			}

			var dayNumber = (int)position;
			// Pick a leap year to ensure all days are accounted for
			var startOfYear = new DateOnly(2024, 1, 1);
			var date = startOfYear.AddDays(dayNumber - 1);
			return date.ToString("MMM d");
		}

		public static string DayOfSeniorityFromDayNumber(double position)
		{
			if (position is < 1d or > 1707d)
			{
				return "OoB";
			}

			var dayNumber = (int)position;
			var firstDayOfSeniority = DateOnly.Parse("2013-09-17");
			var date = firstDayOfSeniority.AddDays(dayNumber);
			return date.ToString("yy-MM-dd");
		}

		public static Func<double, string> MakeDateTimeTickGeneratorFromBaseDate(DateTime baseDate)
		{
			return position =>
			{
				var date = baseDate.AddDays(position);
				return date.ToString("yyyy-MM-dd");
			};
		}
	}
}
