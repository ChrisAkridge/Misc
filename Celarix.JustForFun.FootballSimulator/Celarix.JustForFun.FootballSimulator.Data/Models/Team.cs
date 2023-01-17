using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Data.Models
{
    public class Team
    {
        public int TeamID { get; set; }
        public string CityName { get; set; }
        public string TeamName { get; set; }
        public string Abbreviation { get; set; }
        public Conference Conference { get; set; }
        public Division Division { get; set; }
        
        public double RunningOffenseStrength { get; set; }
        public double RunningDefenseStrength { get; set; }
        public double PassingOffenseStrength { get; set; }
        public double PassingDefenseStrength { get; set; }
        public double OffensiveLineStrength { get; set; }
        public double DefensiveLineStrength { get; set; }
        public double KickingStrength { get; set; }
        public double FieldGoalStrength { get; set; }
        public double KickReturnStrength { get; set; }
        public double KickDefenseStrength { get; set; }
        public double ClockManagementStrength { get; set; }
        
        public TeamDisposition Disposition { get; set; }
        
        public int HomeStadiumID { get; set; }
        public Stadium HomeStadium { get; set; }
    }
}
