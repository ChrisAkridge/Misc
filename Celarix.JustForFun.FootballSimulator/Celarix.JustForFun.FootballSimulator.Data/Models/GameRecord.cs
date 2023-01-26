using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Data.Models
{
    public class GameRecord
    {
        [Key]
        public int GameID { get; set; }
        public int SeasonRecordID { get; set; }
        public SeasonRecord SeasonRecord { get; set; }
        public GameType GameType { get; set; }
        public int WeekNumber { get; set; }
        
        public int HomeTeamID { get; set; }
        public Team HomeTeam { get; set; }
        public int AwayTeamID { get; set; }
        public Team AwayTeam { get; set; }
        public int StadiumID { get; set; }
        public Stadium Stadium { get; set; }
        
        public DateTimeOffset KickoffTime { get; set; }
        public double TemperatureAtKickoff { get; set; }
        public StadiumWeather WeatherAtKickoff { get; set; }
        public string HomeTeamStrengthsAtKickoffJSON { get; set; }
        public string AwayTeamStrengthsAtKickoffJSON { get; set; }

        public List<QuarterBoxScore> QuarterBoxScores = new List<QuarterBoxScore>();
        public List<TeamGameRecord> TeamGameRecords = new List<TeamGameRecord>();
        public List<TeamDriveRecord> TeamDriveRecords = new List<TeamDriveRecord>();
    }
}
