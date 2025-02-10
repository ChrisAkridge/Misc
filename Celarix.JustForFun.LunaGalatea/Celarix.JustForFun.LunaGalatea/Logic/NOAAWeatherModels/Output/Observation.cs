using System;
using System.Collections.Generic;
using System.Linq;
using Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.Output.Units;

namespace Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.Output;

/// <summary>
/// A single observation of current weather conditions at an observation station.
/// Unless otherwise specified, all units are in metric.
/// </summary>
public sealed class Observation
{
    public DateTimeOffset Timestamp { get; set; }
    public string? TextDescription { get; set; }
    public DegreeCelsius? Temperature { get; set; }
    public DegreeCelsius? DewPoint { get; set; }
    public KilometerPerHour? WindSpeed { get; set; }
    public KilometerPerHour? WindGust { get; set; }
    public Meter? Visibility { get; set; }
    public Millimeter? PrecipitationLast6Hours { get; set; }
    public double? RelativeHumidity { get; set; }
    public Pascal? BarometricPressure { get; set; }
    public DegreeCelsius? WindChill { get; set; }
    public DegreeCelsius? HeatIndex { get; set; }
}