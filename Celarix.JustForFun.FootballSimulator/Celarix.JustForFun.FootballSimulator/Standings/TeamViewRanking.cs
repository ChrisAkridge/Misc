using Celarix.JustForFun.FootballSimulator.Scheduling;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Standings
{
    public sealed class TeamViewRanking
    {
        public required BasicTeamInfo BasicTeamInfo { get; init; }
        public required int Ranking { get; init; }

        public override string ToString() => $"#{Ranking} {BasicTeamInfo.Name}";
    }
}
