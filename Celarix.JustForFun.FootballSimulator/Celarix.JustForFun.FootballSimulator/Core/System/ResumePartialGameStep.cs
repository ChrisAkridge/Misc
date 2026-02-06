using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.System
{
    internal static class ResumePartialGameStep
    {
        public static SystemContext Run(SystemContext context)
        {
            var repository = context.Environment.FootballRepository;
            var physicsParams = repository.GetPhysicsParams()
                .ToDictionary(p => p.Name, p => p);
            var random = context.Environment.RandomFactory.Create();
            var gameRecord = repository.GetPartialGameRecord();

            var airTemperature = Helpers.GetTemperatureForGame(gameRecord, physicsParams, random);
            var resumingPlayContext = Helpers.CreateInitialPlayContext(random,
                gameRecord,
                physicsParams["StartWindSpeedStddev"].Value,
                airTemperature);
            Log.Information("ResumePartialGameStep: Initialized base wind direction to {WindDirection} degrees (0 = toward home, 180 = toward away).", resumingPlayContext.BaseWindDirection);
            Log.Information("ResumePartialGameStep: Initialized base wind speed to {WindSpeed} mph.", resumingPlayContext.BaseWindSpeed);
            resumingPlayContext = SetupClockForPartialGame(resumingPlayContext, gameRecord);
            resumingPlayContext = SetupScoreForPartialGame(resumingPlayContext, gameRecord);
            resumingPlayContext = SetupNextPlayAndNextStateForPartiallyCompletedGame(resumingPlayContext, gameRecord);

            // We only write completed drives to the database, so the possessing team is the team
            // that DIDN'T have possession on the last drive.
            resumingPlayContext = resumingPlayContext with
            {
                TeamWithPossession = gameRecord.TeamDriveRecords.Last().Team.Equals(gameRecord.HomeTeam)
                    ? GameTeam.Away
                    : GameTeam.Home
            };

            var playEnvironment = new PlayEnvironment
            {
                DecisionParameters = new GameDecisionParameters
                {
                    Random = random,
                    AwayTeam = gameRecord.AwayTeam ?? throw new InvalidOperationException("Away team is null."),
                    HomeTeam = gameRecord.HomeTeam ?? throw new InvalidOperationException("Home team is null."),
                    GameType = gameRecord.GameType,
                    AwayTeamActualStrengths = TeamStrengthSet.FromTeamDirectly(gameRecord.AwayTeam, GameTeam.Away),
                    HomeTeamActualStrengths = TeamStrengthSet.FromTeamDirectly(gameRecord.HomeTeam, GameTeam.Home),
                    AwayTeamEstimateOfAway = Helpers.EstimateStrengthSetForTeam(gameRecord.AwayTeam, gameRecord.AwayTeam, gameRecord, random, physicsParams),
                    AwayTeamEstimateOfHome = Helpers.EstimateStrengthSetForTeam(gameRecord.HomeTeam, gameRecord.AwayTeam, gameRecord, random, physicsParams),
                    HomeTeamEstimateOfAway = Helpers.EstimateStrengthSetForTeam(gameRecord.AwayTeam, gameRecord.HomeTeam, gameRecord, random, physicsParams),
                    HomeTeamEstimateOfHome = Helpers.EstimateStrengthSetForTeam(gameRecord.HomeTeam, gameRecord.HomeTeam, gameRecord, random, physicsParams),
                },
                PhysicsParams = physicsParams,
                EventBus = context.Environment.EventBus
            };
            resumingPlayContext = resumingPlayContext with
            {
                Environment = playEnvironment
            };

            var awayMainStadium = repository.GetStadium(gameRecord.AwayTeam.HomeStadiumID);
            var homeMainStadium = repository.GetStadium(gameRecord.HomeTeam.HomeStadiumID);
            var gameMonth = gameRecord.KickoffTime.Month;
            var awayAcclimatedTemperature = Helpers.GetTemperatureForStadiumAndMonth(awayMainStadium, gameMonth);
            var homeAcclimatedTemperature = Helpers.GetTemperatureForStadiumAndMonth(homeMainStadium, gameMonth);

            context.Environment.CurrentGameRecord = gameRecord;
            context.Environment.CurrentGameContext = new GameContext(
                Version: 0L,
                NextState: GameState.Start,
                Environment: new GameEnvironment
                {
                    PhysicsParams = physicsParams,
                    CurrentPlayContext = resumingPlayContext,
                    CurrentGameRecord = gameRecord,
                    FootballRepository = repository,
                    RandomFactory = context.Environment.RandomFactory,
                    DebugContextWriter = context.Environment.DebugContextWriter,
                    AwayActiveRoster = repository.GetActiveRosterForTeam(gameRecord.AwayTeamID),
                    HomeActiveRoster = repository.GetActiveRosterForTeam(gameRecord.HomeTeamID),
                    EventBus = context.Environment.EventBus
                },
                AwayTeamAcclimatedTemperature: awayAcclimatedTemperature,
                HomeTeamAcclimatedTemperature: homeAcclimatedTemperature,
                TeamWithPossession: GameTeam.Away,
                PlayCountOnDrive: 0
            );

            Log.Information("ResumePartialGameStep: Resumed partial game {GameID}.",
                gameRecord.GameID);
            return context.WithNextState(SystemState.InGame);
        }

        internal static PlayContext SetupClockForPartialGame(PlayContext playContext, GameRecord gameRecord)
        {
            var drivesByQuarter = gameRecord.TeamDriveRecords
                .GroupBy(d => d.QuarterNumber)
                .ToDictionary(g => g.Key, g => g.ToArray());

            // Sanity check: the sum of durations of all drives in the second quarter, fourth quarter, or
            // any overtime period n % 2 == 1 cannot exceed 15 minutes
            foreach (var quarterDrives in drivesByQuarter.Where(kvp => kvp.Key % 2 == 1))
            {
                var quarterNumber = quarterDrives.Key;
                var drives = quarterDrives.Value;
                var totalDriveDuration = drives.Sum(d => d.DriveDurationSeconds);
                var maximumAllowedDuration = quarterNumber <= 4
                    ? Constants.SecondsPerQuarter
                    : Constants.SecondsPerOvertimePeriod;
                if (totalDriveDuration > maximumAllowedDuration)
                {
                    throw new InvalidOperationException(
                        $"Total drive duration {totalDriveDuration} in quarter {quarterNumber} exceeds maximum allowed {maximumAllowedDuration} (game ID {gameRecord.GameID}.");
                }
            }

            var totalDurationOfAllDrives = gameRecord.TeamDriveRecords.Sum(d => d.DriveDurationSeconds);
            int secondsIntoPeriod;
            int periodNumber;

            if (totalDurationOfAllDrives <= Constants.SecondsPerQuarter * 4)
            {
                periodNumber = (totalDurationOfAllDrives / Constants.SecondsPerQuarter) + 1;
                secondsIntoPeriod = totalDurationOfAllDrives % Constants.SecondsPerQuarter;
            }
            else
            {
                var overtimeDuration = totalDurationOfAllDrives - (Constants.SecondsPerQuarter * 4);
                periodNumber = 4 + (overtimeDuration / Constants.SecondsPerOvertimePeriod) + 1;
                secondsIntoPeriod = overtimeDuration % Constants.SecondsPerOvertimePeriod;
            }

            int secondsRemainingInPeriod = (periodNumber <= 4
                ? Constants.SecondsPerQuarter
                : Constants.SecondsPerOvertimePeriod) - secondsIntoPeriod;
            Log.Information("ResumePartialGameStep: Resuming game {GameID} from period {PeriodNumber} with {SecondsRemaining} seconds remaining in period.",
                gameRecord.GameID, periodNumber, secondsRemainingInPeriod);
            return playContext with
            {
                PeriodNumber = periodNumber,
                SecondsLeftInPeriod = secondsRemainingInPeriod
            };
        }

        internal static PlayContext SetupScoreForPartialGame(PlayContext playContext, GameRecord gameRecord)
        {
            DriveResult[] scoringDriveResults = [
                DriveResult.FieldGoalSuccess,
                DriveResult.TouchdownNoXP,
                DriveResult.TouchdownWithXP,
                DriveResult.TouchdownWithTwoPointConversion,
                DriveResult.TouchdownWithOffensiveSafety
            ];

            int homeTeamScore = 0;
            int awayTeamScore = 0;

            if (gameRecord.AwayTeam is null || gameRecord.HomeTeam is null)
            {
                throw new InvalidOperationException("ResumePartialGameStep: Teams for partial game not loaded from database.");
            }

            foreach (var drive in gameRecord.TeamDriveRecords.Where(r => scoringDriveResults.Contains(r.Result)))
            {
                var scoringTeam = drive.Team.Equals(gameRecord.HomeTeam) ? GameTeam.Home : GameTeam.Away;
                var otherTeam = scoringTeam == GameTeam.Home ? GameTeam.Away : GameTeam.Home;
                var scoringTeamScore = drive.Result switch
                {
                    DriveResult.Safety => 0,
                    DriveResult.FieldGoalSuccess => 3,
                    DriveResult.TouchdownNoXP => 6,
                    DriveResult.TouchdownWithXP => 7,
                    DriveResult.TouchdownWithTwoPointConversion => 8,
                    DriveResult.TouchdownWithOffensiveSafety => 6,
                    DriveResult.TouchdownWithDefensiveScore => 6,
                    _ => throw new InvalidOperationException("Impossible code path; got non-scoring drive despite filtering them out.")
                };
                var otherTeamScore = drive.Result switch
                {
                    DriveResult.Safety => 2,
                    DriveResult.TouchdownWithOffensiveSafety => 1,
                    DriveResult.TouchdownWithDefensiveScore => 2,
                    _ => 0
                };

                homeTeamScore += scoringTeam == GameTeam.Home ? scoringTeamScore : otherTeamScore;
                awayTeamScore += scoringTeam == GameTeam.Away ? scoringTeamScore : otherTeamScore;
            }

            Log.Information("ResumePartialGameStep: Resuming game {GameID} with score {AwayAbbreviation} {AwayTeamScore} @ {HomeTeamScore} {HomeAbbreviation}.",
                gameRecord.GameID,
                gameRecord.AwayTeam.Abbreviation,
                awayTeamScore,
                homeTeamScore,
                gameRecord.HomeTeam.Abbreviation);
            return playContext with
            {
                HomeScore = homeTeamScore,
                AwayScore = awayTeamScore
            };
        }

        internal static PlayContext SetupNextPlayAndNextStateForPartiallyCompletedGame(PlayContext playContext, GameRecord gameRecord)
        {
            var lastDrive = gameRecord.TeamDriveRecords.Last();

            // Drive results that would be a first down for the team receiving possession
            if (lastDrive.Result is DriveResult.FieldGoalMiss or
                DriveResult.FumbleLost or
                DriveResult.Interception or
                DriveResult.Punt or
                DriveResult.Safety or
                DriveResult.TurnoverOnDowns)
            {
                Log.Information("ResumePartialGameStep: Resuming game {GameID} with next play kind FirstDown after defensive change of possession.",
                    gameRecord.GameID);
                // Kind of a hacky way to actually kick off the main loop.
                return playContext.WithFirstDownLineOfScrimmage(PlayContextExtensions.TeamYardToInternalYard(playContext.TeamWithPossession, 25),
                    playContext.TeamWithPossession,
                    "Resuming game. {OffAbbr} has first down at their own 25.",
                    startOfDrive: true)
                    .WithNextState(PlayEvaluationState.MainGameDecision);
            }

            var nextPlayKind = lastDrive.Result switch
            {
                DriveResult.FieldGoalSuccess => NextPlayKind.Kickoff,
                DriveResult.TouchdownNoXP => NextPlayKind.Kickoff,
                DriveResult.TouchdownWithXP => NextPlayKind.Kickoff,
                DriveResult.TouchdownWithTwoPointConversion => NextPlayKind.Kickoff,
                DriveResult.TouchdownWithOffensiveSafety => NextPlayKind.Kickoff,
                DriveResult.TouchdownWithDefensiveScore => NextPlayKind.Kickoff,
                DriveResult.Safety => NextPlayKind.FreeKick,
                DriveResult.EndOfHalf => NextPlayKind.Kickoff,
                _ => throw new InvalidOperationException("Impossible code path; got unknown drive result.")
            };

            var nextState = nextPlayKind == NextPlayKind.Kickoff
                ? PlayEvaluationState.KickoffDecision
                : PlayEvaluationState.FreeKickDecision;

            Log.Information("ResumePartialGameStep: Resuming game {GameID} with next play kind {NextPlayKind}.",
                gameRecord.GameID, nextPlayKind);
            return playContext.WithNextState(nextState) with
            {
                NextPlay = nextPlayKind
            };
        }
    }
}
