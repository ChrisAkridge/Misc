using System;
using System.Collections.Generic;
using System.Linq;

namespace Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.Points;

public class Relativelocation
{
    public string type { get; set; }
    public PointsSubgeometry geometry { get; set; }
    public PointsSubproperties properties { get; set; }
}