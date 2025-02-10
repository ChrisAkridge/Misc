using System;
using System.Collections.Generic;
using System.Linq;

namespace Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.Points;

public class Properties
{
    public string id { get; set; }
    public string type { get; set; }
    public string cwa { get; set; }
    public string forecastOffice { get; set; }
    public string gridId { get; set; }
    public int gridX { get; set; }
    public int gridY { get; set; }
    public string forecast { get; set; }
    public string forecastHourly { get; set; }
    public string forecastGridData { get; set; }
    public string observationStations { get; set; }
    public Relativelocation relativeLocation { get; set; }
    public string forecastZone { get; set; }
    public string county { get; set; }
    public string fireWeatherZone { get; set; }
    public string timeZone { get; set; }
    public string radarStation { get; set; }
}