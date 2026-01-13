using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Data.Models
{
    public class TeamPlayoffSeed
    {
        [Key]
        public int TeamPlayoffSeedID { get; set; }
        public int TeamID { get; set; }
        public int SeasonRecordID { get; set; }
        public int Seed { get; set; }
    }
}
