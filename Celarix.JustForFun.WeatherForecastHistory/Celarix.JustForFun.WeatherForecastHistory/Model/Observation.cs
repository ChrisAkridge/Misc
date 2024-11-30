using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.WeatherForecastHistory.Model
{
	public sealed class Observation
	{
		public int ObservationId { get; set; }
		public DateTimeOffset RecordedOn { get; set; }
		public int ActualHighTemperature { get; set; }
	}
}
