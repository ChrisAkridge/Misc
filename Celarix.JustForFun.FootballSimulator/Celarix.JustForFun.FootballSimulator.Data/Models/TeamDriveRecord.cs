using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Data.Models
{
    public class TeamDriveRecord
    {
        [Key]
        public int TeamDriveRecordID { get; set; }
        public int GameRecordID { get; set; }
        public int TeamID { get; set; }
        public GameRecord? GameRecord { get; set; }
        public GameTeam Team { get; set; }
        
        public int QuarterNumber { get; set; }
        public int DriveStartTimeSeconds { get; set; }
        public int StartingFieldPosition { get; set; }
        public int PlayCount { get; set; }
        public int DriveDurationSeconds { get; set; }
        public int NetYards { get; set; }
        public DriveResult Result { get; set; }
    }
}
