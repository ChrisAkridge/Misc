using Celarix.JustForFun.FootballSimulator.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    public sealed record GameContext(
        // State machine
        long Version,
        GameState NextState,

        // Environment
        [property: JsonIgnore] GameEnvironment Environment,

        // Injury Chance Modifiers
        double AwayTeamAcclimatedTemperature,
        double HomeTeamAcclimatedTemperature,

        // Game Properties That Persist Through the Play Evaluation
        GameTeam TeamWithPossession,
        int PlayCountOnDrive,

        // Players Involved
        [property: JsonIgnore] IReadOnlyList<PlayerRosterPosition>? OffensePlayersOnPlay = null,
        [property: JsonIgnore] IReadOnlyList<PlayerRosterPosition>? DefensePlayersOnPlay = null
    );
}
