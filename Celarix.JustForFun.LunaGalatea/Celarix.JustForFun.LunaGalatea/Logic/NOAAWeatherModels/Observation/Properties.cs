using System;
using System.Collections.Generic;
using System.Linq;

namespace Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.Observation;

public class Properties
{
    public string id { get; set; }
    public string type { get; set; }
    public Elevation elevation { get; set; }
    public string station { get; set; }
    public DateTime timestamp { get; set; }
    public string rawMessage { get; set; }
    public string textDescription { get; set; }
    public string icon { get; set; }
    public Presentweather[] presentWeather { get; set; }
    public Temperature temperature { get; set; }
    public Dewpoint dewpoint { get; set; }
    public Winddirection windDirection { get; set; }
    public Windspeed windSpeed { get; set; }
    public Windgust windGust { get; set; }
    public Barometricpressure barometricPressure { get; set; }
    public Sealevelpressure seaLevelPressure { get; set; }
    public Visibility visibility { get; set; }
    public Maxtemperaturelast24hours maxTemperatureLast24Hours { get; set; }
    public Mintemperaturelast24hours minTemperatureLast24Hours { get; set; }
    public Precipitationlasthour precipitationLastHour { get; set; }
    public Precipitationlast3hours precipitationLast3Hours { get; set; }
    public Precipitationlast6hours precipitationLast6Hours { get; set; }
    public Relativehumidity relativeHumidity { get; set; }
    public Windchill windChill { get; set; }
    public Heatindex heatIndex { get; set; }
    public Cloudlayer[] cloudLayers { get; set; }
}