using Celarix.JustForFun.FootballSimulator.Scheduling;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Standings
{
    public sealed class TieGroup(params IEnumerable<BasicTeamInfo> teamStats)
    {
        private readonly List<BasicTeamInfo> tiedTeams = [.. teamStats];

        public IReadOnlyList<BasicTeamInfo> TiedTeams => tiedTeams.AsReadOnly();
        public int Ranking { get; set; }
    }
}
