using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Scheduling
{
    internal sealed class BasicTeamInfoComparer : IEqualityComparer<BasicTeamInfo?>
    {
        public bool Equals(BasicTeamInfo? x, BasicTeamInfo? y)
        {
            if (x == null && y == null) { return true; }

            return x?.CompareTo(y) == 0;
        }

        public int GetHashCode([DisallowNull] BasicTeamInfo obj)
        {
            return obj?.GetHashCode() ?? 0;
        }
    }
}
