using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Countdown
{
	internal enum TimeLeftForm
	{
		Default,
		RemainingSeconds,
		RemainingMinutes,
		RemainingHours,
		RemainingDays,
		RemainingWeeks,
		RemainingYears,
		DecibelsSeconds,
		XKCD1017Equation
	}

	internal enum RecurrenceType
	{
		DailyRecurrence,
		WeeklyRecurrence,
		MonthlyRecurrence,
		YearlyRecurrence
	}
}
