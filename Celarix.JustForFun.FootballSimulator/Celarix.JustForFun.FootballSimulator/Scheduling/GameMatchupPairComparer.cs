using System;
using System.Collections.Generic;
using System.Linq;

namespace Celarix.JustForFun.FootballSimulator.Scheduling;

internal sealed class GameMatchupPairComparer : IEqualityComparer<GameMatchup>
{
    public bool Equals(GameMatchup? x, GameMatchup? y) =>
        ReferenceEquals(x, y)
        || (!ReferenceEquals(x, null)
            && !ReferenceEquals(y, null)
            && x.GetType() == y.GetType()
            && x.GamePairId.Equals(y.GamePairId));

    public int GetHashCode(GameMatchup obj) => obj.GamePairId.GetHashCode();
}