using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Data.Models
{
    public class InjuryRecovery
    {
        [Key]
        public int InjuryRecoveryID { get; set; }
        public int TeamID { get; set; }
        public string Strength { get; set; }
        public double StrengthDelta { get; set; }
        public DateTime RecoverOn { get; set; }
        public bool Recovered { get; set; }
    }
}
