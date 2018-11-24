using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeijerStatAnalyzer
{
	public static class Extensions
	{
		public static string Prefix(this DayType dayType)
		{
			switch (dayType)
			{
				case DayType.Working: return "W";
				case DayType.Off: return "N";
				case DayType.PaidTimeOff: return "PV";
				case DayType.UnpaidTimeOff:return "NV";
				default:
					throw new ArgumentOutOfRangeException(nameof(dayType), dayType, null);
			}
		}
	}
}
