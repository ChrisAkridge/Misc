using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    internal class TeamWinLossTie
    {
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Ties { get; set; }

        public decimal WinPercentage
        {
            get
            {
                var totalGames = Wins + Losses + Ties;
                if (totalGames == 0)
                {
                    return 0m;
                }
                return (decimal)(Wins + (Ties * 0.5)) / totalGames;
            }
        }
    }
}
