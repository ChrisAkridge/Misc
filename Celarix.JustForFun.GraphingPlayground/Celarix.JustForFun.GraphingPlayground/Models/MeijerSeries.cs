using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.GraphingPlayground.Models
{
	internal sealed class MeijerSeries
	{
		public int OverallSeriesNumber { get; init; }
		public MeijerDayType SeriesType { get; init; }
		public int SeriesNumberOfType { get; init; }
		public int DaysInSeries { get; init; }
		public decimal HoursInSeries { get; init; }
		public DateOnly SeriesFirstDay { get; init; }
		public DateOnly SeriesLastDay { get; init; }
		public DateTimeOffset SeriesMidway { get; init; }
	}
}
