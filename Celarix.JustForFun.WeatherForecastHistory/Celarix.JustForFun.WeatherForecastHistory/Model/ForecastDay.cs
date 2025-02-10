using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.WeatherForecastHistory.Model
{
	public sealed class ForecastDay
	{
		public int ForecastDayId { get; set; }
		public int ForecastId { get; set; }
		public DateTimeOffset ForDate { get; set; }
		public int HighTemperature { get; set; }
		
		public required Forecast Forecast { get; set; }
	}
}
