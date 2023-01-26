using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Data.Models
{
    public class Stadium
    {
        [Key]
        public int StadiumID { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string AverageTemperatures { get; set; }
        public double TotalPrecipitationOverSeason { get; set; }
        public double AverageWindSpeed { get; set; }
    }
}
