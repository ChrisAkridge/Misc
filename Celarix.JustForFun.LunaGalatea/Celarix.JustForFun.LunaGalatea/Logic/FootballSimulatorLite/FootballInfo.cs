using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Logic.FootballSimulatorLite
{
    public class FootballInfo
    {
        public int Year { get; set; }
        public int Week { get; set; }
        public int GameNumber { get; set; }
        public FootballTeam[] Teams { get; set; }
    }
}
