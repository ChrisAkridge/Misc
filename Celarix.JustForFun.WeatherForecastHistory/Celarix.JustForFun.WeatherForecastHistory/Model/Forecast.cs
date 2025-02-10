using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.WeatherForecastHistory.Model
{
	public sealed class Forecast
	{
		public int ForecastId { get; set; }
		public int ForecastSourceId { get; set; }
		public DateTimeOffset RecordedOn { get; set; }
		
		public required ForecastSource ForecastSource { get; set; }
		public required ICollection<ForecastDay> ForecastDays { get; set; }
	}
}
