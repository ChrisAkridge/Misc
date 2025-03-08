using System;
using System.Collections.Generic;
using System.Linq;

namespace Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.Observation;

public class Presentweather
{
    public string intensity { get; set; }
    public object modifier { get; set; }
    public string weather { get; set; }
    public string rawString { get; set; }
}