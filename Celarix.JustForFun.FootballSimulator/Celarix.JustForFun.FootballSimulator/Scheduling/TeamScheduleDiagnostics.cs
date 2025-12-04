using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Scheduling
{
    public sealed class TeamScheduleDiagnostics
    {
        internal record MatchupDiagnostic(
            ScheduledGameType GameType,
            BasicTeamInfo Opponent
        );
        
        private readonly List<MatchupDiagnostic> matchupDiagnostics = new();

        public BasicTeamInfo Team { get; internal set; }
        public bool TeamDivisionPlaysSelfForIntraconference { get; internal set; }
        public bool TeamDivisionPlaysSelfForRemainingIntraconference { get; internal set; }

        internal void AddMatchup(ScheduledGameType gameType, BasicTeamInfo opponent)
        {
            matchupDiagnostics.Add(new MatchupDiagnostic(gameType, opponent));
        }

        public IReadOnlyList<BasicTeamInfo> GetOpponentsByGameType(ScheduledGameType gameType)
        {
            return matchupDiagnostics
                .Where(md => md.GameType == gameType)
                .Select(md => md.Opponent)
                .ToArray();
        }
    }
}
