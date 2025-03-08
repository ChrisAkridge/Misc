using System;
using System.Collections.Generic;
using System.Linq;

namespace Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.Stations;

public class Properties
{
    public string id { get; set; }
    public string type { get; set; }
    public Elevation elevation { get; set; }
    public string stationIdentifier { get; set; }
    public string name { get; set; }
    public string timeZone { get; set; }
    public string forecast { get; set; }
    public string county { get; set; }
    public string fireWeatherZone { get; set; }
}