﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.HourlyForecast;

public class Period
{
    public int number { get; set; }
    public string name { get; set; }
    public DateTime startTime { get; set; }
    public DateTime endTime { get; set; }
    public bool isDaytime { get; set; }
    public int temperature { get; set; }
    public string temperatureUnit { get; set; }
    public object temperatureTrend { get; set; }
    public Probabilityofprecipitation probabilityOfPrecipitation { get; set; }
    public Dewpoint dewpoint { get; set; }
    public Relativehumidity relativeHumidity { get; set; }
    public string windSpeed { get; set; }
    public string windDirection { get; set; }
    public string icon { get; set; }
    public string shortForecast { get; set; }
    public string detailedForecast { get; set; }
}