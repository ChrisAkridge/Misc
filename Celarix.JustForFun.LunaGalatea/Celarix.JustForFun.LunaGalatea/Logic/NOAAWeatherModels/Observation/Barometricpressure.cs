using System;
using System.Collections.Generic;
using System.Linq;

namespace Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.Observation;

public class Barometricpressure
{
    public string unitCode { get; set; }
    public int? value { get; set; }
    public string qualityControl { get; set; }
}