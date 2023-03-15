using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.FootballSimulator.Data.Models;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    internal sealed class NextPlay
    {
        public NextPlayKind Kind { get; set; }
        public GameTeam Team { get; set; }
        public int LineOfScrimmage { get; set; }
        public int? FirstDownLine { get; set; }
        public DriveDirection Direction { get; set; }
    }
}
