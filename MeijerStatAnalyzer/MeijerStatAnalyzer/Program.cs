using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;

namespace MeijerStatAnalyzer
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			Queries.RunQuery(Parser.Parse());
			Console.ReadKey(intercept: true);
		}

		private static void MatchDayNumbersToLabels()
		{
			StatsByDay stats = Parser.Parse();

			var package = new ExcelPackage(new FileInfo(@"C:\Users\Chris\Documents\Documents\Microsoft Office\Excel\Meijer - Employment Statistics Final.xlsx"));
			ExcelWorksheet ws = package.Workbook.Worksheets["Stats"];

			var workingDays = stats.Days.Where(d => d.Type == DayType.Working);
			var daysOff = stats.Days.Where(d => d.Type == DayType.Off);
			var paidDaysOff = stats.Days.Where(d => d.Type == DayType.PaidTimeOff);
			var unpaidDaysOff = stats.Days.Where(d => d.Type == DayType.UnpaidTimeOff);

			var dayLabels = Enumerable.Range(8, 1706).Select(d => ((int rowNumber, string label))(d, ws.Cells[$"F{d}"].Value));
			var workingDayLabels = dayLabels.Where(l => l.label.StartsWith("W"));
			var daysOffLabels = dayLabels.Where(l => l.label.StartsWith("N") && !l.label.StartsWith("NV"));
			var paidDaysOffLabels = dayLabels.Where(l => l.label.StartsWith("PV"));
			var unpaidDaysOffLabels = dayLabels.Where(l => l.label.StartsWith("NV"));

			var builder = new StringBuilder();
			var workingDayEnumerator = paidDaysOff.GetEnumerator();
			var workingDayLabelEnumerator = paidDaysOffLabels.GetEnumerator();
			workingDayLabelEnumerator.MoveNext();

			var daysOffWithNoLabel = new List<Day>();
			while (workingDayEnumerator.MoveNext())
			{
				int dayNumber = workingDayEnumerator.Current.DayNumberOfType;
				builder.Append($"{workingDayEnumerator.Current.Date.ToShortDateString()} {dayNumber} ");
				if (!workingDayLabelEnumerator.Current.Equals(default(ValueTuple<int, string>)))
				{
					builder.Append($"| {ws.Cells[$"C{workingDayLabelEnumerator.Current.rowNumber}"].Value} ");
					builder.Append($"{workingDayLabelEnumerator.Current.label}");

					int labelNumber = int.Parse(Parser.GetNumberFromEndOfString(workingDayLabelEnumerator.Current.label));
					if (labelNumber != dayNumber)
					{
						builder.Append($" {labelNumber - dayNumber}");
					}

					builder.Append("\r\n");
					workingDayLabelEnumerator.MoveNext();
				}
				else
				{
					daysOffWithNoLabel.Add(workingDayEnumerator.Current);
					builder.Append("\r\n");
				}
			}

			File.WriteAllText(@"C:\Users\Chris\Documents\meijerDaysExport.txt", builder.ToString());

			Console.WriteLine("Done.");
		}

		private static void ExportStatsToTextFile()
		{
			Console.WriteLine("Parsing stats...");
			StatsByDay stats = Parser.Parse();

			Console.WriteLine("Creating export file...");
			var builder = new StringBuilder();
			int dayNumber = 1;

			foreach (Day day in stats.Days)
			{
				builder.Append($"{dayNumber}:{day.Date.ToShortDateString()} | ");
				builder.Append(
					$"{day.Date.DayOfWeek.ToString().Substring(0, 3)}/{day.Type.Prefix()}{day.DayNumberOfType} | ");

				if (day.Shift != null)
				{
					Shift shift = day.Shift;
					builder.Append($"{shift.StartTime.ToTimeString()}-{shift.EndTime.ToTimeString()} | ");
					builder.Append($"{shift.WorkedHours:F1} hours | {shift.PaidHours:F1} hours paid | {shift.UnpaidHours:F1} hours unpaid | ");
					builder.Append($"{shift.WaitHours:F1} hours waited | ${shift.PayRatePerHour:F2}/hour | ");
					builder.Append($"${shift.Pay:F2} paid | ${shift.EstimatedFinalPay} est. aftertax\r\n");
				}
				else
				{
					builder.Append("No shift.\r\n");
				}

				dayNumber++;
			}

			string exportFile = builder.ToString();
			Console.WriteLine($"Created export file (length {exportFile.Length}).");
			Console.WriteLine($"Exporting file...");
			File.WriteAllText(@"C:\Users\Chris\Documents\meijerStatsExport.txt", exportFile);
			Console.WriteLine("Export complete.");
		}

		private static void OldMain()
		{
			Console.WriteLine("Meijer Statistics Analyzer");
			Console.WriteLine();
			Console.WriteLine("Press a key to perform an action. Press H for help.");

			bool shouldContinue = true;
			while (shouldContinue)
			{
				char key = Console.ReadKey(true).KeyChar;

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
			string[] textArray = File.ReadAllLines(path);

			// OKAY SO APPARENTLY THE BELOW MIND-BENDING LINQ IS MISTAKENLY MAKING 744 DAYS OUT OF 300-ODD
			// MAYBE DUPLICATING?

			var schedules = textArray.Where(l => !string.IsNullOrEmpty(l)).Where(l => char.IsNumber(l.First())).Select(l => new string(l.Where(c => char.IsLetterOrDigit(c) || c == ':' || c == '-').ToArray<char>())).OrderBy(l => l).ToList(); // good lord

			int open = 0;		// Starts from 5am-8am
			int mid = 0;		// Starts from 8am-4pm and ends at or before 9pm
			int close = 0;		// Ends from 9pm-11pm
			int third = 0;		// Starts from 9pm-5am

			foreach (string shift in schedules)
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
