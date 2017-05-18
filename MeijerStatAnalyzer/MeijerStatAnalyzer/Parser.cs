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
		private static string XlsxFilePath = @"C:\Users\Chris\Documents\Documents\Meijer - Employment Statistics 2.0.xlsx";
		private static readonly DateTime fileLastSavedDate = new DateTime(2016, 10, 7);

		public static StatsByDay Parse()
		{
			var result = new StatsByDay();

			var package = new ExcelPackage(new FileInfo(XlsxFilePath));
			var ws = package.Workbook.Worksheets["Stats"];

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
			DateTime date = (DateTime)ws.Cells[$"C{rowNumber}"].Value;
			Tuple<SecondsAfterMidnight, SecondsAfterMidnight> schedule = null;
			SecondsAfterMidnight startTime = new SecondsAfterMidnight(0);
			SecondsAfterMidnight endTime = new SecondsAfterMidnight(0);
			string dayNote = null;
			string dateNote = null;
			string timeNote = null;
			DayType type = DayType.Off;
			int dayNumberOfType = -1;
			Shift shift = null;

			object scheduleValue = ws.Cells[$"D{rowNumber}"].Value;
			if (scheduleValue != null)
			{
				bool wasSchedule = false;
				schedule = ParseScheduleString((string)scheduleValue, out wasSchedule);
				
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
			if (dayNoteValue != null && dayNoteValue is string)
			{
				dayNote = (string)dayNoteValue;
				if (dayNote.StartsWith("Week")) { dayNote = null; }
				else if (dayNote.StartsWith("(")) { dayNote = null; }
			}

			var dateComment = ws.Cells[$"C{rowNumber}"].Comment;
			if (dateComment != null)
			{
				dateNote = dateComment.Text;
			}

			var timeComment = ws.Cells[$"D{rowNumber}"].Comment;
			if (timeComment != null)
			{
				timeNote = timeComment.Text;
			}

			string dayNumberValue = (string)ws.Cells[$"F{rowNumber}"].Value;
			if (dayNumberValue[0] == 'W') { type = DayType.Working; }
			else if (dayNumberValue[0] == 'N') { type = DayType.Off; }
			else if (dayNumberValue.StartsWith("NV")) { type = DayType.UnpaidTimeOff; }
			else if (dayNumberValue.StartsWith("PV")) { type = DayType.PaidTimeOff; }
			dayNumberOfType = int.Parse(GetNumberFromEndOfString(dayNumberValue));

			if (schedule != null && date < fileLastSavedDate)
			{
				shift = ParseShift(startTime, endTime, ws, rowNumber);
			}

			return new Day(date, dayNote, dateNote, timeNote, shift, type, dayNumberOfType);
		}

		private static Shift ParseShift(SecondsAfterMidnight start, SecondsAfterMidnight end, ExcelWorksheet ws, int rowNumber)
		{
			double workingHours = (double)ws.Cells[$"G{rowNumber}"].Value;
			double waitingHours = (double)ws.Cells[$"I{rowNumber}"].Value;
			double paidHours = (double)ws.Cells[$"J{rowNumber}"].Value;
			double payRate = double.Parse(ws.Cells[$"L{rowNumber}"].Value.ToString()) / paidHours;

			return new Shift(start, end, workingHours, waitingHours, paidHours, payRate);
		}

		private static Tuple<SecondsAfterMidnight, SecondsAfterMidnight> ParseScheduleString(string schedule, out bool wasSchedule)
		{
			if (!char.IsDigit(schedule[0])) { wasSchedule = false;  return null; }
			schedule = schedule.Replace("*", "");

			string[] portions = schedule.Split('-');
			if (!portions[0].EndsWith("m")) { portions[0] = string.Concat(portions[0], "pm"); }

			SecondsAfterMidnight startTime = new SecondsAfterMidnight(DateTime.Parse(portions[0]));
			SecondsAfterMidnight endTime = new SecondsAfterMidnight(DateTime.Parse(portions[1]));

			wasSchedule = true;
			return new Tuple<SecondsAfterMidnight, SecondsAfterMidnight>(startTime, endTime);
		}

		private static string GetNumberFromEndOfString(string s)
		{
			// http://stackoverflow.com/questions/13169393/extract-number-at-end-of-string-in-c-sharp

			return Regex.Match(s, @"\d+$").Value;
		}
	}
}
