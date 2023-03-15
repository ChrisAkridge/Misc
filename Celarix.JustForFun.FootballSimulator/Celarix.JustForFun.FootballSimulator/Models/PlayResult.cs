using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.FootballSimulator.Data.Models;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    internal sealed class PlayResult
    {
        public PlayResultKind Kind { get; set; }
        public GameTeam Team { get; set; }
        public int? DownNumber { get; set; }
        public int? BallDeadYard { get; set; }
        public int? FirstDownLine { get; set; }
        public DriveDirection Direction { get; set; }
    }
}
