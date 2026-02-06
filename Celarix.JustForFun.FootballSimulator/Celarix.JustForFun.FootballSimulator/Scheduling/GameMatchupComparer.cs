using System;
using System.Collections.Generic;
using System.Linq;

namespace Celarix.JustForFun.FootballSimulator.Scheduling;

internal sealed class GameMatchupComparer(IEqualityComparer<BasicTeamInfo> teamComparer) : IEqualityComparer<GameMatchup>
{
    private readonly IEqualityComparer<BasicTeamInfo> teamComparer = teamComparer;

    public bool Equals(GameMatchup? x, GameMatchup? y)
    {
        if (x == null || y == null) { return false; }
        if (x.GameType != y.GameType) { return false; }
        
        if (teamComparer.Equals(x.TeamA, y.TeamA) && teamComparer.Equals(x.TeamB, y.TeamB)) { return true; }
        if (teamComparer.Equals(x.TeamA, y.TeamB) && teamComparer.Equals(x.TeamB, y.TeamA)) { return true; }

        return false;
    }

    public int GetHashCode(GameMatchup obj) =>
        obj == null
            ? throw new ArgumentNullException(nameof(obj))
            : HashCode.Combine(obj.GameType, obj.TeamA.GetHashCode() + obj.TeamB.GetHashCode());
}