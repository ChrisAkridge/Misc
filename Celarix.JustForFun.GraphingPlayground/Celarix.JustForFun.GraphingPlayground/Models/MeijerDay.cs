using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.GraphingPlayground.Models
{
	internal sealed class MeijerDay
	{
		public int WeekNumber { get; init; }
		public DateOnly Date { get; init; }
		public DayOfWeek Weekday => Date.DayOfWeek;
		public TimeOnly? ShiftStart { get; init; }
		public TimeOnly? ShiftEnd { get; init; }
		
		public int OverallDayNumber { get; init; }
		public MeijerDayType DayType { get; init; }
		public int DayNumberOfType { get; init; }
		
		public decimal? HoursWorked { get; init; }
		public decimal? QuartersWorker => HoursWorked * 4;
		public decimal? HoursWaited { get; init; }
		public decimal? PaidHours { get; init; }
		public decimal? UnpaidHours { get; init; }
		public decimal? Pay { get; init; }

		public MeijerShiftType ShiftType =>
			DayType != MeijerDayType.Working
				? MeijerShiftType.NonWorking
				: ShiftEnd!.Value.Hour switch
				{
					< 12 => MeijerShiftType.ThirdShift,
					>= 12 and < 18 => MeijerShiftType.Open,
					>= 18 and < 21 => MeijerShiftType.Midshift,
					>= 21 => MeijerShiftType.Closing
				};
	}
}
