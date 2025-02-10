using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.WeatherForecastHistory.Model
{
	public sealed class ForecastSource
	{
		public int ForecastSourceId { get; set; }
		public required string DisplayName { get; set; }
		public string? URL { get; set; }
		
		public required ICollection<Forecast> Forecasts { get; set; }
	}
}
