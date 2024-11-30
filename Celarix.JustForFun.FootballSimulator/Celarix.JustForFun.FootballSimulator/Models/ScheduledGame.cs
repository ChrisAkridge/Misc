using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Scheduling;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    internal sealed class ScheduledGame
    {
        public GameRecord GameRecord { get; set; }
        public ScheduledGameType GameType { get; set; }
        public BasicTeamInfo HomeTeamInfo { get; set; }
        public BasicTeamInfo AwayTeamInfo { get; set; }
    }
}
