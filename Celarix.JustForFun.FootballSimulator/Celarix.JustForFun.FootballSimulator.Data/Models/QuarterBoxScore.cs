using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Data.Models
{
    public class QuarterBoxScore
    {
        [Key]
        public int QuarterBoxScoreID { get; set; }
        public int GameRecordID { get; set; }
        public GameRecord? GameRecord { get; set; }
        
        public int QuarterNumber { get; set; }
        public int Score { get; set; }
        public GameTeam Team { get; set; }
    }
}
