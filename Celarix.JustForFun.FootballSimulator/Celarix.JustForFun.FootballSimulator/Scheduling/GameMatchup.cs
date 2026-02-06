using System;
using System.Collections.Generic;
using System.Linq;

namespace Celarix.JustForFun.FootballSimulator.Scheduling;

internal sealed class GameMatchup
{
    public required BasicTeamInfo TeamA { get; set; }
    public required BasicTeamInfo TeamB { get; set; }
    public ScheduledGameType GameType { get; set; }
    public bool? HomeTeamIsTeamA { get; set; }

    /// <summary>Returns a string that represents the current object.</summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => $"{TeamA.Name} vs. {TeamB.Name} ({GameType})";
}