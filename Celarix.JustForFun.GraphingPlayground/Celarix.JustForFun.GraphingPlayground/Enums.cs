using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.GraphingPlayground
{
	internal enum PlotIndexType
	{
		Data,
		LinearRegression,
		RollingAverage
	}
	
	[Flags]
	internal enum AdditionalSupport
	{
		LinearRegression = 0x1,
		RollingAverage = 0x2,
		Distribution = 0x4
	}

	internal enum GraphPropertyType
	{
		Numeric,
		Date
	}

	internal enum MeijerDayType
	{
		Working,
		NonWorking,
		PaidTimeOff,
		NonPaidTimeOff,
		Other
	}

	internal enum MeijerShiftType
	{
		NonWorking,
		Open,
		Midshift,
		Closing,
		ThirdShift
	}
}