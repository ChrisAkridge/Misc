using Celarix.JustForFun.FootballSimulator.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    internal sealed record GameState(
        // Weather conditions
        double BaseWindDirection,
        double BaseWindSpeed,

        // Game status
        GameTeam TeamWithPossession,
        int AwayScore,
        int HomeScore,
        int PeriodNumber,
        int SecondsLeftInPeriod,
        bool ClockRunning,
        int HomeTimeoutsRemaining,
        int AwayTimeoutsRemaining,

        // Down-and-distance
        int LineOfScrimmage,
        int? LineToGain,
        NextPlayKind NextPlay,

        // Display properties
        string LastPlayDescriptionTemplate,

        // Internal properties
        PossessionOnPlay PossessionOnPlay,
        GameTeam? TeamCallingTimeout
    );
}
