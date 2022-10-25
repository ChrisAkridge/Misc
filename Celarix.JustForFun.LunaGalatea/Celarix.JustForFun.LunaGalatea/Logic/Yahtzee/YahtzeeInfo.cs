using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Logic.Yahtzee
{
    public sealed class YahtzeeInfo
    {
        public int TotalGamesPlayed { get; set; }
        public long TotalPointsScored { get; set; }
        public double AveragePointsPerGame { get; set; }
        public int TotalYahtzeeCount { get; set; }
        public int HighScore { get; set; }
    }
}
