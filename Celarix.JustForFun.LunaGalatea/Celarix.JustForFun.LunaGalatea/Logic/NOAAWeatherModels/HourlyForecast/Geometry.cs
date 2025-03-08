using System;
using System.Collections.Generic;
using System.Linq;

namespace Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.HourlyForecast;

public class Geometry
{
    public string type { get; set; }
    public float[][][] coordinates { get; set; }
}