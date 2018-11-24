using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OfficeOpenXml;

namespace MeijerStatAnalyzer
{
	public static class Parser
	{
		private const string XlsxFilePath = @"C:\Users\Chris\Documents\Documents\Microsoft Office\Excel\Meijer - Employment Statistics Final.xlsx";
		private static readonly DateTime fileLastSavedDate = new DateTime(2018, 5, 26);

		public static StatsByDay Parse()
		{
			var result = new StatsByDay();

			var package = new ExcelPackage(new FileInfo(XlsxFilePath));
			ExcelWorksheet ws = package.Workbook.Worksheets["Stats"];

			int currentRow = 8;
			bool valueExists = (ws.Cells[$"B{currentRow}"].Value != null);
			
			while (valueExists)
			{
				result.AddDay(ParseDay(ws, currentRow));

				currentRow++;
				valueExists = (ws.Cells[$"B{currentRow}"].Value != null);
			}

			result.FinalConstruction();
			return result;
		}

		private static Day ParseDay(ExcelWorksheet ws, int rowNumber)
		{
			var date = (DateTime)ws.Cells[$"C{rowNumber}"].Value;
			Tuple<SecondsAfterMidnight, SecondsAfterMidnight> schedule = null;
			var startTime = new SecondsAfterMidnight(0);
			var endTime = new SecondsAfterMidnight(0);
			string dayNote = null;
			string dateNote = null;
			string timeNote = null;
			DayType type = DayType.Off;
			Shift shift = null;

			object scheduleValue = ws.Cells[$"D{rowNumber}"].Value;
			if (scheduleValue != null)
			{
				schedule = ParseScheduleString((string)scheduleValue, out bool wasSchedule);

				if (!wasSchedule)
				{
					dayNote = (string)scheduleValue;
				}
				else
				{
					startTime = schedule.Item1;
					endTime = schedule.Item2;
				}
			}

			object dayNoteValue = ws.Cells[$"A{rowNumber}"].Value;
			if (dayNoteValue is string s)
			{
				dayNote = s;
				if (dayNote.StartsWith("Week")) { dayNote = null; }
				else if (dayNote.StartsWith("(")) { dayNote = null; }
			}

			ExcelComment dateComment = ws.Cells[$"C{rowNumber}"].Comment;
			if (dateComment != null)
			{
				dateNote = dateComment.Text;
			}

			ExcelComment timeComment = ws.Cells[$"D{rowNumber}"].Comment;
			if (timeComment != null)
			{
				timeNote = timeComment.Text;
			}

			string dayNumberValue = (string)ws.Cells[$"F{rowNumber}"].Value.ToString().ToUpperInvariant();
			if (dayNumberValue[0] == 'W') { type = DayType.Working; }
			else if (dayNumberValue.StartsWith("NV")) { type = DayType.UnpaidTimeOff; }
			else if (dayNumberValue[0] == 'N') { type = DayType.Off; }
			else if (dayNumberValue.StartsWith("PV")) { type = DayType.PaidTimeOff; }

			if (type == DayType.PaidTimeOff)
			{
				shift = ParseShift(new SecondsAfterMidnight(0), new SecondsAfterMidnight(21600), ws, rowNumber);
			}
			else if (schedule != null && date < fileLastSavedDate)
			{
				shift = ParseShift(startTime, endTime, ws, rowNumber);
			}

			return new Day(date, dayNote, dateNote, timeNote, shift, type);
		}

		private static Shift ParseShift(SecondsAfterMidnight start, SecondsAfterMidnight end, ExcelWorksheet ws, int rowNumber)
		{
			var date = (DateTime)ws.Cells[$"C{rowNumber}"].Value;
			double workingHours = (double)ws.Cells[$"G{rowNumber}"].Value;
			double waitingHours = (double)ws.Cells[$"I{rowNumber}"].Value;
			double paidHours = (double)ws.Cells[$"J{rowNumber}"].Value;
			double payRate = double.Parse(ws.Cells[$"L{rowNumber}"].Value.ToString()) / paidHours;

			return new Shift(date, start, end, workingHours, waitingHours, paidHours, payRate);
		}

		private static Tuple<SecondsAfterMidnight, SecondsAfterMidnight> ParseScheduleString(string schedule, out bool wasSchedule)
		{
			if (!char.IsDigit(schedule[0])) { wasSchedule = false;  return null; }
			schedule = schedule.Replace("*", "");

			string[] portions = schedule.Split('-');
			if (!portions[0].EndsWith("m")) { portions[0] = string.Concat(portions[0], "pm"); }

			var startTime = new SecondsAfterMidnight(DateTime.Parse(portions[0]));
			var endTime = new SecondsAfterMidnight(DateTime.Parse(portions[1]));

			wasSchedule = true;
			return new Tuple<SecondsAfterMidnight, SecondsAfterMidnight>(startTime, endTime);
		}

		// http://stackoverflow.com/questions/13169393/extract-number-at-end-of-string-in-c-sharp
		internal static string GetNumberFromEndOfString(string s) => Regex.Match(s, @"\d+$").Value;
	}
}
