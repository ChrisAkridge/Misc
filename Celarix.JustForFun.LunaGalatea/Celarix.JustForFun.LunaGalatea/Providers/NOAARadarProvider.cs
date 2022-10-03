using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Providers
{
    public class NOAARadarProvider : IProvider<string>
    {
        public string GetDisplayObject() => "https://radar.weather.gov/ridge/standard/KLVX_0.gif";
    }
}
