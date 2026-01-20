using Celarix.JustForFun.FootballSimulator.Scheduling;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Standings
{
    public sealed class TieGroup
    {
        private readonly List<BasicTeamInfo> tiedTeams;

        public IReadOnlyList<BasicTeamInfo> TiedTeams => tiedTeams.AsReadOnly();
        public int Ranking { get; set; }

        public TieGroup(params IEnumerable<BasicTeamInfo> teamStats) => tiedTeams = [.. teamStats];
    }
}
