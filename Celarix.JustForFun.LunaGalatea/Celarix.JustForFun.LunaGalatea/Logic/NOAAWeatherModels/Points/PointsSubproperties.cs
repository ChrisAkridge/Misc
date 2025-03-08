using System;
using System.Collections.Generic;
using System.Linq;

namespace Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.Points;

public class PointsSubproperties
{
    public string city { get; set; }
    public string state { get; set; }
    public Distance distance { get; set; }
    public Bearing bearing { get; set; }
}