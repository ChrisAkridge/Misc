using Celarix.JustForFun.FootballSimulator.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    public sealed record PlayContext(
        // State machine
        long Version,
        PlayEvaluationState NextState,
        IReadOnlyList<AdditionalParameter<object>> AdditionalParameters,
        [property: JsonIgnore] IReadOnlyList<StateHistoryEntry> StateHistory,

        // Environment
        [property: JsonIgnore] PlayEnvironment? Environment,

        // Initial conditions
        double BaseWindDirection,
        double BaseWindSpeed,
        double AirTemperature,
        GameTeam CoinFlipWinner,

        // Game status
        GameTeam TeamWithPossession,
        int AwayScore,
        int HomeScore,
        int PeriodNumber,
        int SecondsLeftInPeriod,
        bool ClockRunning,
        int HomeTimeoutsRemaining,
        int AwayTimeoutsRemaining,
        PlayInvolvement PlayInvolvement,

        // Down-and-distance
        int LineOfScrimmage,
        int? LineToGain,
        NextPlayKind NextPlay,

        // Start-of-drive properties
        int DriveStartingFieldPosition,
        int DriveStartingPeriodNumber,
        int DriveStartingSecondsLeftInPeriod,

        // End-of-drive properties
        DriveResult? DriveResult,

        // Play results
        string LastPlayDescriptionTemplate,
        bool AwayScoredThisPlay,
        bool HomeScoredThisPlay,

        // Internal properties
        PossessionOnPlay PossessionOnPlay,
        GameTeam? TeamCallingTimeout
    );

    public sealed record AdditionalParameter<T>(
        string Key,
        T Value,
        long AddedInVersion
    );

    public sealed record StateHistoryEntry(
        PlayEvaluationState State,
        GameTeam TeamWithPossession,
        long Version
    );
}
