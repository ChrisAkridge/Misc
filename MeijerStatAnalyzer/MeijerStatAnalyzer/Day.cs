﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeijerStatAnalyzer
{
	public sealed class Day
	{
		public static readonly DateTime SeniorityDate = new DateTime(2013, 9, 17);

		public DateTime Date { get; }
		public string Weekday => Date.DayOfWeek.ToString();
		public string Note { get; }
		public string DateNote { get; }
		public string TimeNote { get; }
		public int DayNumber => (int)(Date - SeniorityDate).TotalDays;
		public Shift Shift { get; }
		public DayType Type { get; }
		public int DayNumberOfType { get; set; }

		public Day(DateTime date, string note, string dateNote, string timeNote, 
			Shift shift, DayType type)
		{
			Date = date;
			Note = note;
			DateNote = dateNote;
			TimeNote = timeNote;
			Shift = shift;
			Type = type;
		}

		public override string ToString()
		{
			string shiftInfo = (Shift != null) ? Shift.ToString() : "Off";
			return $"{Date.ToShortDateString()} | {DayNumber}/{Type.Prefix()}{DayNumber} | {shiftInfo}";
		}
	}

	public enum DayType
	{
		Working,
		Off,
		PaidTimeOff,
		UnpaidTimeOff
	}
}
