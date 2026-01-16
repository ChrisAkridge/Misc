using Celarix.JustForFun.FootballSimulator.Scheduling;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Standings
{
    public sealed class TeamStats
    {
        public BasicTeamInfo BasicTeamInfo { get; }
        public int Wins { get; }
        public int Losses { get; }
        public int Ties { get; }
        public decimal WinPercentage
        {
            get
            {
                int totalGames = Wins + Losses + Ties;
                if (totalGames == 0) return 0m;
                return (Wins + (Ties * 0.5m)) / totalGames;
            }
        }

        public TeamStats(BasicTeamInfo basicTeamInfo, int wins, int losses, int ties)
        {
            BasicTeamInfo = basicTeamInfo;
            Wins = wins;
            Losses = losses;
            Ties = ties;
        }
    }
}
