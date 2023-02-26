using System;
using System.Collections.Generic;
using System.Linq;

namespace Celarix.JustForFun.FootballSimulator.Scheduling;

internal sealed class GameMatchupComparer : IEqualityComparer<GameMatchup>
{
    public bool Equals(GameMatchup? x, GameMatchup? y) =>
        ReferenceEquals(x, y)
        || (!ReferenceEquals(x, null)
            && !ReferenceEquals(y, null)
            && x.GetType() == y.GetType()
            && (x.AwayTeam.Equals(y.AwayTeam) || x.AwayTeam.Equals(y.HomeTeam))
            && (x.HomeTeam.Equals(y.HomeTeam) || x.HomeTeam.Equals(y.AwayTeam)));

    public int GetHashCode(GameMatchup obj) => HashCode.Combine(obj.AwayTeam, obj.HomeTeam);
}