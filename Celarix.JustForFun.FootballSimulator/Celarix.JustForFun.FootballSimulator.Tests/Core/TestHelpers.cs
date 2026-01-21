using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Tests.Core
{
    internal static class TestHelpers
    {
        public static PlayContext EmptyState => new PlayContext(
            Version: 0L,
            NextState: PlayEvaluationState.Start,
            AdditionalParameters: Array.Empty<AdditionalParameter<object>>(),
            StateHistory: Array.Empty<StateHistoryEntry>(),
            Environment: null!,
            BaseWindDirection: 0.0,
            BaseWindSpeed: 0.0,
            AirTemperature: 0.0,
            CoinFlipWinner: GameTeam.Home,
            TeamWithPossession: GameTeam.Away,
            AwayScore: 0,
            HomeScore: 0,
            PeriodNumber: 1,
            SecondsLeftInPeriod: 900,
            ClockRunning: false,
            HomeTimeoutsRemaining: 3,
            AwayTimeoutsRemaining: 3,
            LineOfScrimmage: 25,
            LineToGain: null,
            NextPlay: NextPlayKind.Kickoff,
            DriveStartingFieldPosition: 25,
            DriveStartingPeriodNumber: 1,
            DriveStartingSecondsLeftInPeriod: 900,
            LastPlayDescriptionTemplate: string.Empty,
            PossessionOnPlay: PossessionOnPlay.AwayTeamOnly,
            TeamCallingTimeout: null
        );

        public static IReadOnlyDictionary<string, PhysicsParam> EmptyPhysicsParams =>
            new Dictionary<string, PhysicsParam>();

        public static IReadOnlyDictionary<string, PhysicsParam> CreatePhysicsParams(string key, double value)
        {
            return new Dictionary<string, PhysicsParam>
            {
                { key, new PhysicsParam(key, value, "", "") }
            };
        }
    }
}
