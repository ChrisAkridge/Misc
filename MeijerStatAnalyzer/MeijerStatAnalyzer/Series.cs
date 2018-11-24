using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeijerStatAnalyzer
{
	public sealed class Series
	{
		private readonly List<Day> days;

		public int SeriesNumber { get; }
		public int DaysInSeries => days.Count;
		public double DownsInSeries => days.Sum(d => d.Shift.WorkedHours);

		public IReadOnlyList<Day> Days => days.AsReadOnly();

		public Series(IEnumerable<Day> days)
		{
			if (!days.All(d => d.Shift != null)) { throw new ArgumentNullException(nameof(days), "Series must only consist of working shifts."); }

			this.days = days.ToList();
		}
	}
}
