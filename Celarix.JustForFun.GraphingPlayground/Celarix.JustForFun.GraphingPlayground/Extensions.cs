using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.GraphingPlayground
{
	internal static class Extensions
	{
		public static void RemoveAtIndices<T>(this IList<T> list, int[] indices)
		{
			// Remove the indices in reverse order to avoid shifting elements
			Array.Sort(indices);
			for (int i = indices.Length - 1; i >= 0; i--)
			{
				list.RemoveAt(indices[i]);
			}
		}

		public static string Ordinal(this int number)
		{
			if (number <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(number), "Number must be greater than zero.");
			}

			var lastDigit = number % 10;
			var lastTwoDigits = number % 100;
			if (lastTwoDigits is >= 11 and <= 13)
			{
				return $"{number}th";
			}

			return lastDigit switch
			{
				1 => $"{number}st",
				2 => $"{number}nd",
				3 => $"{number}rd",
				_ => $"{number}th"
			};
		}
		
		public static IEnumerable<(T? First, T? Second)> Pair<T>(this IEnumerable<T?> source)
		{
			using var enumerator = source.GetEnumerator();
			while (enumerator.MoveNext())
			{
				var first = enumerator.Current;
				if (!enumerator.MoveNext())
				{
					yield return (first, default);
				}

				var second = enumerator.Current;
				yield return (first, second);
			}
		}

		public static T[] Flatten2DArray<T>(this T[,] array)
		{
			var flatArray = new T[array.GetLength(0) * array.GetLength(1)];
			for (int i = 0; i < array.GetLength(0); i++)
			{
				for (int j = 0; j < array.GetLength(1); j++)
				{
					flatArray[(i * array.GetLength(1)) + j] = array[i, j];
				}
			}

			return flatArray;
		}

		public static TimeSpan GetTimezoneOffsetByDate(this DateOnly date)
		{
			// Figure out if Daylight Savings Time was in effect for the Eastern Time Zone on the given date
			// No shift ever ran over the time change, so we don't need to worry about that
			var dateTime = new DateTime(date.Year, date.Month, date.Day);
			var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
			var isDST = timeZoneInfo.IsDaylightSavingTime(dateTime);

			return isDST ? TimeSpan.FromHours(-4d) : TimeSpan.FromHours(-5d);
		}

		// https://stackoverflow.com/a/11155102/2709212
		public static int GetIso8601WeekOfYear(this DateOnly date)
		{
			// Seriously cheat.  If it's Monday, Tuesday or Wednesday, then it'll 
			// be the same week number as whatever Thursday, Friday or Saturday are,
			// and we always get those right.
			var day = date.DayOfWeek;
			if (day is >= DayOfWeek.Monday and <= DayOfWeek.Wednesday)
			{
				date = date.AddDays(3);
			}
			
			// Return the week of our adjusted day
			return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(new DateTime(date, TimeOnly.MinValue),
				CalendarWeekRule.FirstFourDayWeek,
				DayOfWeek.Monday);
		}
	}
}
