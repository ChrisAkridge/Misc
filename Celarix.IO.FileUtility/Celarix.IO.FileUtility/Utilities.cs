using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Celarix.IO.FileUtility
{
	internal static partial class Utilities
	{
		public static bool FileIsHiddenOrSystem(string filePath)
		{
			var fileAttributes = File.GetAttributes(filePath);

			return fileAttributes.HasFlag(FileAttributes.Hidden) || fileAttributes.HasFlag(FileAttributes.System);
		}

		public static int GetIndexFromSeriesFileName(string? seriesFileName, SeriesType seriesType)
		{
			if (seriesType == SeriesType.Invalid
				|| seriesFileName == null
				|| seriesFileName.Length == 0) { return -1; }

			if (seriesType == SeriesType.Picture)
			{
				var seriesPrefix = char.ToLowerInvariant(seriesFileName[0]);

				if (!char.IsLetter(seriesPrefix))
				{
					Console.WriteLine("The series prefix must be a letter.");

					return -1;
				}

				var seriesBasis = (seriesPrefix - 'a') * 500;

				if (seriesFileName.Length != 4 || !int.TryParse(seriesFileName.AsSpan(1), out var fileNameNumber))
				{
					Console.WriteLine("The remainder of the starting filename must be a 3-digit number");

					return -1;
				}

				if (fileNameNumber > 500)
				{
					Console.WriteLine("The file number must be less than 500.");

					return -1;
				}

				// Return with fileNameNumber - 1 because the series starts at 1, not 0, but
				// we want to represent the series as 0-based internally.
				return seriesBasis + (fileNameNumber - 1);
			}

			if (seriesType == SeriesType.Screenshot)
			{
				var lowercaseStartFrom = seriesFileName.ToLowerInvariant();
				if (lowercaseStartFrom.Count(c => c == 's') != 1)
				{
					Console.WriteLine("Screenshot-like series filenames are a series number, followed by a lowercase s, then a 6-digit file number, like 3s001204.");

					return -1;
				}

				var seriesNumberDigits = new string(seriesFileName.TakeWhile(char.IsDigit).ToArray());

				if (!int.TryParse(seriesNumberDigits, out var seriesNumber))
				{
					Console.WriteLine("The series number must be a number.");

					return -1;
				}

				var seriesBasis = (seriesNumber - 1) * 2000;
				var fileNameNumberText = lowercaseStartFrom.Split('s')[1];

				if (fileNameNumberText.Length != 6 || !int.TryParse(fileNameNumberText, out var fileNameNumber))
				{
					Console.WriteLine("The file number must be a six-digit number.");

					return -1;
				}

				if (fileNameNumber > 2000) { Console.WriteLine("The file number must be less than 2000."); }

				// Return with fileNameNumber - 1 because the series starts at 1, not 0, but
				// we want to represent the series as 0-based internally.
				return seriesBasis + (fileNameNumber - 1);
			}

			throw new InvalidOperationException("Unreachable.");
		}

		// https://stackoverflow.com/a/11720793/2709212
		public static IOrderedEnumerable<T> OrderByAlphaNumeric<T>(this IReadOnlyList<T> source, Func<T, string> selector)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}

			var max = source
					.SelectMany(i => DigitMatch().Matches(selector(i)).Select(m => (int?)m.Value.Length))
					.Max()
				?? 0;

			return source.OrderBy(i => DigitMatch().Replace(selector(i), m => m.Value.PadLeft(max, '0')));
		}

		[GeneratedRegex(@"\d+")]
		private static partial Regex DigitMatch();
	}
}
