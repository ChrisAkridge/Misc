using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.Output
{
    public sealed class WeatherResponse
    {
        /// <summary>
        /// Gets or sets the time that the 15-minute cycle of requests started (typically is application
        /// startup).
        /// </summary>
        public DateTimeOffset RequestCycleStart { get; set; }
        public WeatherResponseType WeatherResponseType { get; set; }
        public bool RequestSuccessful { get; set; }
        public Exception? ThrownException { get; set; }
        
        /// <summary>
        /// Gets or sets the time this request was made.
        /// </summary>
        public DateTimeOffset RequestTime { get; set; }
        public DateTimeOffset NextPlannedRequestTime { get; set; }
        
        /// <summary>
        /// Gets or sets the time when the next full (observations and forecasts) request will be made.
        /// </summary>
        public DateTimeOffset NextPlannedFullRequestTime { get; set; }
        
        public Observation Observation { get; set; }
        public List<ForecastPeriod> PeriodForecasts { get; set; }
        public List<ForecastPeriod> HourlyForecasts { get; set; }
    }
}
