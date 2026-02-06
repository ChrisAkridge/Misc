using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Data.Models
{
    public class TeamGameRecord
    {
        [Key]
        public int TeamGameRecordID { get; set; }
        public int GameRecordID { get; set; }
        public GameRecord? GameRecord { get; set; }
        
        public GameTeam Team { get; set; }
        
        public int FirstDowns { get; set; }
        public int RushAttempts { get; set; }
        public int RushYards { get; set; }
        public int RushTouchdowns { get; set; }
        
        public int PassAttempts { get; set; }
        public int PassCompletions { get; set; }
        public int PassYards { get; set; }
        public int PassTouchdowns { get; set; }
        public int PassInterceptions { get; set; }
        
        public int Sacks { get; set; }
        public int SackYards { get; set; }
        public int Fumbles { get; set; }
        public int FumblesLost { get; set; }
        public int Penalties { get; set; }
        public int PenaltyYards { get; set; }
        
        public int ThirdDownConversionAttempts { get; set; }
        public double AverageThirdDownDistance { get; set; }
        public int ThirdDownConversions { get; set; }
        public int FourthDownConversionAttempts { get; set; }
        public double AverageFourthDownDistance { get; set; }
        public int FourthDownConversions { get; set; }
        
        public int FieldGoalAttempts { get; set; }
        public int FieldGoalsMade { get; set; }
        public int ExtraPointAttempts { get; set; }
        public int ExtraPointAttemptsMade { get; set; }
        public int TwoPointConversionAttempts { get; set; }
        public int TwoPointConversionAttemptsMade { get; set; }
        
        public int Punts { get; set; }
        public int PuntYards { get; set; }

        public int TimeOfPossessionSeconds { get; set; }
    }
}
