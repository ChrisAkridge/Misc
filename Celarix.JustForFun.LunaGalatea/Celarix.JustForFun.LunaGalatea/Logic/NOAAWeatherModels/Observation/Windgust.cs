using System;
using System.Collections.Generic;
using System.Linq;

namespace Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.Observation;

public class Windgust
{
    public string unitCode { get; set; }
    public float? value { get; set; }
    public string qualityControl { get; set; }
}