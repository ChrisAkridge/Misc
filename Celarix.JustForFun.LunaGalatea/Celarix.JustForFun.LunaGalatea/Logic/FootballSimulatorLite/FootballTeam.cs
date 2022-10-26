using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Logic.FootballSimulatorLite
{
    public sealed class FootballTeam
    {
        public string Location { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public int TotalGames { get; set; }
        public int Wins { get; set; }
        public int Ties { get; set; }
        public int PointsScored { get; set; }
        public int PointsAllowed { get; set; }

        [JsonIgnore]
        public double Record
        {
            get
            {
                var winValue = 1d / TotalGames;
                var tieValue = 0.5d / TotalGames;
                return (winValue * Wins) + (tieValue * Ties);
            }
        }
        
        public double KickoffStrength { get; set; }
        public double KickReturnStrength { get; set; }
        public double PuntingStrength { get; set; }
        public double FreeKickStrength { get; set; }
        public double FieldGoalStrength { get; set; }
        public double RushingStrength { get; set; }
        public double PassingStrength { get; set; }
        public double RushDefenseStrength { get; set; }
        public double PassDefenseStrength { get; set; }
        public double BallCarryStrength { get; set; }
    }
}
