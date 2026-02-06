using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Tests.Core
{
    internal static class TestHelpers
    {
        public static SystemContext EmptySystemContext => new SystemContext(
            Version: 0L,
            NextState: SystemState.Start,
            Environment: null!
        );

        public static GameContext EmptyGameContext => new GameContext(
            Version: 0L,
            NextState: GameState.Start,
            Environment: null!,
            AwayTeamAcclimatedTemperature: 65,
            HomeTeamAcclimatedTemperature: 65,
            TeamWithPossession: GameTeam.Away,
            PlayCountOnDrive: 0,
            OffensePlayersOnPlay: [],
            DefensePlayersOnPlay: []
        );

        public static PlayContext EmptyPlayContext => new PlayContext(
            Version: 0L,
            NextState: PlayEvaluationState.Start,
            AdditionalParameters: [],
            StateHistory: [],
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
            PlayInvolvement: new PlayInvolvement(
                InvolvesOffenseRun: false,
                InvolvesOffensePass: false,
                InvolvesKick: false,
                InvolvesDefenseRun: false,
                OffensivePlayersInvolved: 0,
                DefensivePlayersInvolved: 0),
            LineOfScrimmage: 25,
            LineToGain: null,
            NextPlay: NextPlayKind.Kickoff,
            DriveStartingFieldPosition: 25,
            DriveStartingPeriodNumber: 1,
            DriveStartingSecondsLeftInPeriod: 900,
            DriveResult: null,
            LastPlayDescriptionTemplate: string.Empty,
            AwayScoredThisPlay: false,
            HomeScoredThisPlay: false,
            PossessionOnPlay: PossessionOnPlay.AwayTeamOnly,
            TeamCallingTimeout: null
        );

        public static IReadOnlyDictionary<string, PhysicsParam> EmptyPhysicsParams =>
            new Dictionary<string, PhysicsParam>();

        public static PlayInvolvement EmptyPlayInvolvement =>
            new PlayInvolvement(
                InvolvesOffenseRun: false,
                InvolvesOffensePass: false,
                InvolvesKick: false,
                InvolvesDefenseRun: false,
                OffensivePlayersInvolved: 0,
                DefensivePlayersInvolved: 0);

        public static IReadOnlyDictionary<string, PhysicsParam> CreatePhysicsParams(string key, double value)
        {
            return new Dictionary<string, PhysicsParam>
            {
                { key, new PhysicsParam(key, value, "", "") }
            };
        }

        public static void SetRandomStrengths(Team team)
        {
            var random = new global::System.Random();
            team.ClockManagementStrength = random.Next(1, 100);
            team.DefensiveLineStrength = random.Next(1, 100);
            team.OffensiveLineStrength = random.Next(1, 100);
            team.RunningDefenseStrength = random.Next(1, 100);
            team.OffensiveLineStrength = random.Next(1, 100);
            team.PassingDefenseStrength = random.Next(1, 100);
            team.PassingOffenseStrength = random.Next(1, 100);
            team.RunningOffenseStrength = random.Next(1, 100);
            team.KickDefenseStrength = random.Next(1, 100);
            team.KickingStrength = random.Next(1, 100);
            team.KickReturnStrength = random.Next(1, 100);
            team.FieldGoalStrength = random.Next(1, 100);
        }

        public static void SetFixedStrengths(Team team, double strength)
        {
            team.ClockManagementStrength = strength;
            team.DefensiveLineStrength = strength;
            team.OffensiveLineStrength = strength;
            team.RunningDefenseStrength = strength;
            team.OffensiveLineStrength = strength;
            team.PassingDefenseStrength = strength;
            team.PassingOffenseStrength = strength;
            team.RunningOffenseStrength = strength;
            team.KickDefenseStrength = strength;
            team.KickingStrength = strength;
            team.KickReturnStrength = strength;
            team.FieldGoalStrength = strength;
        }

        public static void AssertStrengthJSON(string? json, Team team)
        {
            Assert.NotNull(json);

            // Deserialize to JSON object
            var deserialized = global::System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, double>>(json);

            Assert.NotNull(deserialized);

            // Assert each strength value
            Assert.Equal(team.ClockManagementStrength, deserialized["ClockManagementStrength"]);
            Assert.Equal(team.DefensiveLineStrength, deserialized["DefensiveLineStrength"]);
            Assert.Equal(team.OffensiveLineStrength, deserialized["OffensiveLineStrength"]);
            Assert.Equal(team.RunningDefenseStrength, deserialized["RunningDefenseStrength"]);
            Assert.Equal(team.OffensiveLineStrength, deserialized["OffensiveLineStrength"]);
            Assert.Equal(team.PassingDefenseStrength, deserialized["PassingDefenseStrength"]);
            Assert.Equal(team.PassingOffenseStrength, deserialized["PassingOffenseStrength"]);
            Assert.Equal(team.RunningOffenseStrength, deserialized["RunningOffenseStrength"]);
            Assert.Equal(team.KickDefenseStrength, deserialized["KickDefenseStrength"]);
            Assert.Equal(team.KickingStrength, deserialized["KickingStrength"]);
            Assert.Equal(team.KickReturnStrength, deserialized["KickReturnStrength"]);
        }

        public static void AssertStrengthsEqual(TeamStrengthSet expected, Team actual)
        {
            Assert.Equal(expected.RunningOffenseStrength, actual.RunningOffenseStrength);
            Assert.Equal(expected.RunningDefenseStrength, actual.RunningDefenseStrength);
            Assert.Equal(expected.PassingOffenseStrength, actual.PassingOffenseStrength);
            Assert.Equal(expected.PassingDefenseStrength, actual.PassingDefenseStrength);
            Assert.Equal(expected.OffensiveLineStrength, actual.OffensiveLineStrength);
            Assert.Equal(expected.DefensiveLineStrength, actual.DefensiveLineStrength);
            Assert.Equal(expected.KickingStrength, actual.KickingStrength);
            Assert.Equal(expected.FieldGoalStrength, actual.FieldGoalStrength);
            Assert.Equal(expected.KickReturnStrength, actual.KickReturnStrength);
            Assert.Equal(expected.KickDefenseStrength, actual.KickDefenseStrength);
            Assert.Equal(expected.ClockManagementStrength, actual.ClockManagementStrength);
        }

        public static void AssertEstimatedStrengthsEqual(TeamStrengthSet actual, Team expected, double factor)
        {
            Assert.Equal(expected.RunningOffenseStrength * factor, actual.RunningOffenseStrength);
            Assert.Equal(expected.RunningDefenseStrength * factor, actual.RunningDefenseStrength);
            Assert.Equal(expected.PassingOffenseStrength * factor, actual.PassingOffenseStrength);
            Assert.Equal(expected.PassingDefenseStrength * factor, actual.PassingDefenseStrength);
            Assert.Equal(expected.OffensiveLineStrength * factor, actual.OffensiveLineStrength);
            Assert.Equal(expected.DefensiveLineStrength * factor, actual.DefensiveLineStrength);
            Assert.Equal(expected.KickingStrength * factor, actual.KickingStrength);
            Assert.Equal(expected.FieldGoalStrength * factor, actual.FieldGoalStrength);
            Assert.Equal(expected.KickReturnStrength * factor, actual.KickReturnStrength);
            Assert.Equal(expected.KickDefenseStrength * factor, actual.KickDefenseStrength);
            Assert.Equal(expected.ClockManagementStrength * factor, actual.ClockManagementStrength);
        }
    }
}
