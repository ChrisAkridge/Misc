using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeijerStatAnalyzer
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Meijer Statistics Analyzer");
			Console.WriteLine();
			Console.WriteLine("Press a key to perform an action. Press H for help.");

			bool shouldContinue = true;
			while (shouldContinue)
			{
				var key = Console.ReadKey(true).KeyChar;
				
				switch (char.ToLowerInvariant(key))
				{
					case 'h':
						DisplayHelp();
						break;
					case 'o':
						OpenFolder();
						break;
					case 's':
						ListShiftCounts();
						break;
				}

				Console.WriteLine();
				Console.WriteLine("Press X to exit. Press any other key to return to the menu.");
				shouldContinue = char.ToLowerInvariant(Console.ReadKey(true).KeyChar) != 'x';
			}
		}

		private static void DisplayHelp()
		{
			Console.Clear();
			Console.WriteLine("Press the following keys to perform an action:");
			Console.WriteLine();
			Console.WriteLine("   O: Open the directory containing the files in which to place data");
			Console.WriteLine("   S: List shift counts by (open|mid|close|third) [uses 1 file]");
			Console.ReadKey(true);
		}

		private static void OpenFolder()
		{
			string path = Directory.GetCurrentDirectory();
			Process.Start(path);
			Console.Clear();
		}

		private static void ListShiftCounts()
		{
			string path = string.Concat(Directory.GetCurrentDirectory(), @"\0.txt");
			var textArray = File.ReadAllLines(path);

			// OKAY SO APPARENTLY THE BELOW MIND-BENDING LINQ IS MISTAKENLY MAKING 744 DAYS OUT OF 300-ODD
			// MAYBE DUPLICATING?

			var schedules = textArray.Where(l => !string.IsNullOrEmpty(l)).Where(l => char.IsNumber(l.First())).Select(l => new string(l.Where(c => char.IsLetterOrDigit(c) || c == ':' || c == '-').ToArray<char>())).OrderBy(l => l).ToList(); // good lord

			int open = 0;		// Starts from 5am-8am
			int mid = 0;		// Starts from 8am-4pm and ends at or before 9pm
			int close = 0;		// Ends from 9pm-11pm
			int third = 0;		// Starts from 9pm-5am

			foreach (var shift in schedules)
			{
				string[] times = shift.Split('-');

				// The second time string is guaranteed to have AM or PM
				string tt = times[1].Substring(times[1].Length - 2);

				if (!times[0].EndsWith("M", StringComparison.InvariantCultureIgnoreCase))
				{
					times[0] = string.Concat(times[0], tt);
				}

				DateTime start = DateTime.Now;
				DateTime end = DateTime.Now;

				if (!DateTime.TryParseExact(times[0], "htt", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out start))
				{
					start = DateTime.ParseExact(times[0], "h:mmtt", CultureInfo.InvariantCulture);
				}

				if (!DateTime.TryParseExact(times[1], "htt", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out end))
				{
					end = DateTime.ParseExact(times[1], "h:mmtt", CultureInfo.InvariantCulture);
				}

				if (start.Hour >= 5 && start.Hour <= 8)
				{
					open++;
				}
				else if ((start.Hour >= 8 && start.Hour <= 16) && (end.Hour <= 21))
				{
					mid++;
				}
				else if (end.Hour >= 21 && end.Hour <= 23)
				{
					close++;
				}
				else
				{
					third++;
				}
			}

			double sum = open + mid + close + third;

			Console.Clear();
			Console.WriteLine("Of {0} shifts:", sum);
			Console.WriteLine("Number of openings: {0} ({1}%)", open, ((double)open / sum));
			Console.WriteLine("Number of midshifts: {0} ({1}%)", mid, ((double)mid / sum));
			Console.WriteLine("Number of closings: {0} ({1}%)", close, ((double)mid / sum));
			Console.WriteLine("Number of third shifts: {0} ({1}%)", third, ((double)third / sum));
			Console.ReadKey(true);
		}
	}
}
