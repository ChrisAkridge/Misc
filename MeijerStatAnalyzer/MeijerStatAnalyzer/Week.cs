using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeijerStatAnalyzer
{
	public sealed class Week
	{
		private readonly List<Day> days;
		
		public DateTime SundayDate { get; }
		public DateTime SaturdayDate => SundayDate.AddDays(6d);
		public int WeekNumber => (int)((SundayDate - Day.SeniorityDate).TotalDays / 7d);

		public Day this[int dayNumber]
		{
			get
			{
				if (dayNumber < 0 || dayNumber > 6) { throw new ArgumentOutOfRangeException(); }
				return days[dayNumber];
			}
		}

		public int WorkingDays => days.Count(d => d.Shift != null);
		public int DaysOff => 7 - WorkingDays;

		public Week(DateTime sundayDate, Day sunday, Day monday, Day tuesday,
		 Day wednesday, Day thursday, Day friday, Day saturday)
		{
			if (sundayDate.DayOfWeek != DayOfWeek.Sunday) { throw new ArgumentException(); }
			days = new List<Day> {sunday, monday, tuesday, wednesday, thursday, friday, saturday};
		}
	}
}
