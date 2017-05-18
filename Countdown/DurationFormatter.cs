using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace Countdown
{
	internal static class DurationFormatter
	{
		private const double eCubed = 20.0855369232d;
		private static readonly DateTimeZone Eastern;
		private static readonly int LowestInstantYear;

		static DurationFormatter()
		{
			Eastern = DateTimeZoneProviders.Tzdb["America/New_York"];
			LowestInstantYear = -9997;
		}

		public static string AsDefault(Duration duration)
		{
			int totalYears = (int)(duration.TotalDays / 365d);
			int days = (int)(duration.TotalDays % 365d);
			int weeks = days / 7;
			days = days % 7;

			StringBuilder builder = new StringBuilder();
			if (totalYears > 0)
			{
				builder.Append(totalYears);
				builder.Append("y");
			}
			if (weeks > 0 || totalYears > 0)
			{
				builder.Append(weeks);
				builder.Append("w");
			}
			if (days > 0 || weeks > 0 || totalYears > 0)
			{
				builder.Append(days);
				builder.Append("d ");
			}
			builder.Append(duration.Hours.ToString("D2"));
			builder.Append(":");
			builder.Append(duration.Minutes.ToString("D2"));
			builder.Append(":");
			builder.Append(duration.Seconds.ToString("D2"));

			return builder.ToString();
		}

		public static string AsRemainingSeconds(Duration duration, int decimalPlaces)
		{
			string suffix = (int)duration.TotalSeconds == 1 ? " second" : " seconds";
			return duration.TotalSeconds.ToString($"F{decimalPlaces}") + suffix;
		}

		public static string AsRemainingMinutes(Duration duration, int decimalPlaces)
		{
			string suffix = (int)duration.TotalMinutes == 1 ? " minute" : " minutes";
			return duration.TotalMinutes.ToString($"F{decimalPlaces}") + suffix;
		}

		public static string AsRemainingHours(Duration duration, int decimalPlaces)
		{
			string suffix = (int)duration.TotalHours == 1 ? " hour" : " hours";
			return duration.TotalHours.ToString($"F{decimalPlaces}") + suffix;
		}

		public static string AsRemainingDays(Duration duration, int decimalPlaces)
		{
			string suffix = (int)duration.TotalDays == 1 ? " day" : " days";
			return duration.TotalDays.ToString($"F{decimalPlaces}") + suffix;
		}

		public static string AsRemainingWeeks(Duration duration, int decimalPlaces)
		{
			double weeks = duration.TotalDays / 7d;
			string suffix = weeks == 1 ? " week" : " weeks";
			return weeks.ToString($"F{decimalPlaces}") + suffix;
		}

		public static string AsRemainingYears(Duration duration, int decimalPlaces)
		{
			double years = duration.TotalDays / 365d;
			string suffix = (int)years == 1 ? " year" : " years";
			return years.ToString($"F{decimalPlaces}") + suffix;
		}

		public static string AsDecibelsSeconds(Duration duration, int decimalPlaces)
		{
			// Yes, I know a 20 dB change is a factor-of-ten change. I don't care.
			double decibels = 10 * Math.Log10(duration.TotalSeconds);
			return decibels.ToString($"F{decimalPlaces}") + " dB-seconds";
		}

		public static string AsXKCD1017Equation(Instant startTime, Instant endTime, 
			Instant currentTime, int decimalPlaces)
		{
			long periodLength = endTime.ToUnixTimeMilliseconds() - startTime.ToUnixTimeMilliseconds();
			long elapsedPeriodLength = currentTime.ToUnixTimeMilliseconds() - startTime.ToUnixTimeMilliseconds();
			double progress = (double)elapsedPeriodLength / periodLength;
			double yearsAgo = XKCD1017YearsAgo(progress);

			bool inRange = XKCD1017YearsInRange(currentTime, yearsAgo);
			if (inRange)
			{
				var timeInPast = currentTime - Duration.FromDays(yearsAgo * 365d);
				var period = currentTime - timeInPast;
				return AsDefault(period) + " ago";
			}
			else
			{
				return yearsAgo.ToString($"F{decimalPlaces}") + " years ago";
			}
		}

		private static double XKCD1017YearsAgo(double progress)
		{
			// Equation:
			// T = (now) - (e^(20.3444p^3 + 3) - e^3) years
			double progressTerm = 20.3444d * Math.Pow(progress, 3d) + 3d;
			double eRaisedToTerm = Math.Exp(progressTerm);
			return eRaisedToTerm - eCubed;
		}

		private static bool XKCD1017YearsInRange(Instant instant, double yearsAgo)
		{
			var zdt = instant.InZone(Eastern);
			return (zdt.Year - yearsAgo >= LowestInstantYear);
		}
	}
}
