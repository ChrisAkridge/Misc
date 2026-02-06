using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Data.Models
{
    public class SimulatorSettings
    {
        [Key]
        public int SimulatorSettingsID { get; set; }
        
        public bool SeedDataInitialized { get; set; }
        public bool SaveStateMachineContextsForDebugging { get; set; }
        public required string StateMachineContextSavePath { get; set; }
    }
}
