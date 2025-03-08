using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.Observation
{
    public class ObservationResponse
    {
        public object[] context { get; set; }
        public string type { get; set; }
        public Feature[] features { get; set; }
    }
}
