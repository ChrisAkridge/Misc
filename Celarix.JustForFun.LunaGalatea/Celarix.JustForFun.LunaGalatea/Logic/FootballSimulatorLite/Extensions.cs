using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Logic.FootballSimulatorLite
{
    public static class Extensions
    {
        public static double ClampLow(this double value, double min) =>
            (value < min)
                ? min
                : value;
    }
}
