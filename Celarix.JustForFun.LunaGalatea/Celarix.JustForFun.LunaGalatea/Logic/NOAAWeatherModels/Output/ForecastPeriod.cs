using System;
using System.Collections.Generic;
using System.Linq;

namespace Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.Output;

/// <summary>
/// An hourly or periodic forecast for a single period.
/// </summary>
public sealed class ForecastPeriod
{
    public string PeriodName { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public double TemperatureF { get; set; }
    public double? PrecipitationChance { get; set; }
    public double? RelativeHumidity { get; set; }
    public double WindSpeedMPH { get; set; }
    public string ShortForecast { get; set; }
    public string DetailedForecast { get; set; }
}