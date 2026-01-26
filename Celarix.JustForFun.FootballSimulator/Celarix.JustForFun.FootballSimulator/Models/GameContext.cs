using Celarix.JustForFun.FootballSimulator.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    internal sealed record GameContext(
        // State machine
        long Version,
        GameState NextState,

        // Environment
        GameEnvironment Environment,

        // Injury Chance Modifiers
        double AwayTeamAcclimatedTemperature,
        double HomeTeamAcclimatedTemperature,

        // Game Properties That Persist Through the Play Evaluation
        GameTeam TeamWithPossession,
        int PlayCountOnDrive,

        // Players Involved
        IReadOnlyList<PlayerRosterPosition>? OffensePlayersOnPlay = null,
        IReadOnlyList<PlayerRosterPosition>? DefensePlayersOnPlay = null
    );
}
