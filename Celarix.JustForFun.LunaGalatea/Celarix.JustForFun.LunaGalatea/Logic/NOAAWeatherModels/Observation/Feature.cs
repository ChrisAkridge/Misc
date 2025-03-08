using System;
using System.Collections.Generic;
using System.Linq;

namespace Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.Observation;

public class Feature
{
    public string id { get; set; }
    public string type { get; set; }
    public Geometry geometry { get; set; }
    public Properties properties { get; set; }
}