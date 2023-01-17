using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Data.Models
{
    public class Stadium
    {
        public int StadiumID { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public double AverageTemperature { get; set; }
        public double OddsOfPrecipitation { get; set; }
        public double AverageWindSpeed { get; set; }
    }
}
