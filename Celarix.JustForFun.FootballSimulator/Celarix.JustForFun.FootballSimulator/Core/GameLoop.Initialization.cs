using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core
{
    internal sealed partial class GameLoop
    {
        public void Initialize()
        {
            double airTemperature = GetTemperatureForGame(gameRecord, physicsParams);
            currentState = new PlayContext(
                Version: 0L,
                NextState: PlayEvaluationState.Start,
                StateHistory: ImmutableList<StateHistoryEntry>.Empty,
                AdditionalParameters: ImmutableList<AdditionalParameter<object>>.Empty,
                Environment: null,
                BaseWindDirection: random.NextDouble() * 360.0,
                BaseWindSpeed: random.SampleNormalDistribution(gameRecord.Stadium.AverageWindSpeed, physicsParams["StartWindSpeedStddev"].Value),
                AirTemperature: airTemperature,
                CoinFlipWinner: GameTeam.Home,
                TeamWithPossession: GameTeam.Home,
                AwayScore: 0,
                HomeScore: 0,
                PeriodNumber: 1,
                SecondsLeftInPeriod: Constants.SecondsPerQuarter,
                ClockRunning: false,
                HomeTimeoutsRemaining: 3,
                AwayTimeoutsRemaining: 3,
                PlayInvolvement: null,
                LineOfScrimmage: 50,
                LineToGain: null,
                NextPlay: NextPlayKind.Kickoff,
                DriveStartingFieldPosition: 50,
                DriveStartingPeriodNumber: 1,
                DriveStartingSecondsLeftInPeriod: Constants.SecondsPerQuarter,
                DriveResult: null,
                LastPlayDescriptionTemplate: "Gameplay loop initializing.",
                AwayScoredThisPlay: false,
                HomeScoredThisPlay: false,
                PossessionOnPlay: PossessionOnPlay.None,
                TeamCallingTimeout: null
            );
            gamePlayerManager.StadiumCurrentTemperature = airTemperature;

            currentState = SetupClockForPartiallyCompletedGame(currentState, gameRecord);

            if (currentState.PeriodNumber == 1 && currentState.SecondsLeftInPeriod == Constants.SecondsPerQuarter)
            {
                // New game initialization
                firstPossessingTeam = random.Chance(0.5)
                        ? GameTeam.Away
                        : GameTeam.Home;
                currentState = currentState with
                {
                    TeamWithPossession = firstPossessingTeam
                };
            }
            else
            {
                // Load existing game state from gameRecord
                currentState = SetupScoreForPartiallyCompletedGame(currentState, gameRecord);
                currentState = SetupNextPlayAndNextStateForPartiallyCompletedGame(currentState, gameRecord);

                // We only write completed drives to the database, so the possessing team is the team
                // that DIDN'T have possession on the last drive.
                currentState = currentState with
                {
                    TeamWithPossession = gameRecord.TeamDriveRecords.Last().Team.Equals(gameRecord.HomeTeam)
                        ? GameTeam.Away
                        : GameTeam.Home
                };
            }

            Log.Information("Initialized base wind direction to {WindDirection} degrees (0 = toward home, 180 = toward away).", currentState.BaseWindDirection);
            Log.Information("Initialized base wind speed to {WindSpeed} mph.", currentState.BaseWindSpeed);

            currentParameters = new GameDecisionParameters
            {
                Random = random,
                AwayTeam = gameRecord.AwayTeam,
                HomeTeam = gameRecord.HomeTeam,
                GameType = gameRecord.GameType,
                AwayTeamActualStrengths = TeamStrengthSet.FromTeamDirectly(gameRecord.AwayTeam, GameTeam.Away),
                HomeTeamActualStrengths = TeamStrengthSet.FromTeamDirectly(gameRecord.HomeTeam, GameTeam.Home),
                AwayTeamEstimateOfAway = EstimateStrengthSetForTeam(gameRecord.AwayTeam, gameRecord.AwayTeam),
                AwayTeamEstimateOfHome = EstimateStrengthSetForTeam(gameRecord.HomeTeam, gameRecord.AwayTeam),
                HomeTeamEstimateOfAway = EstimateStrengthSetForTeam(gameRecord.AwayTeam, gameRecord.HomeTeam),
                HomeTeamEstimateOfHome = EstimateStrengthSetForTeam(gameRecord.HomeTeam, gameRecord.HomeTeam),
            };
        }

        private static PlayContext SetupClockForPartiallyCompletedGame(PlayContext priorState,
            GameRecord gameRecord)
        {
            if (gameRecord.TeamDriveRecords.Count == 0)
            {
                Log.Information("No drive records found for current game. Starting from beginning of game.");
                return priorState;
            }

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
            Log.Information("Resuming game {GameID} from period {PeriodNumber} with {SecondsRemaining} seconds remaining in period.",
                gameRecord.GameID, periodNumber, secondsRemainingInPeriod); ;
            return priorState with
            {
                PeriodNumber = periodNumber,
                SecondsLeftInPeriod = secondsRemainingInPeriod
            };
        }

        private static PlayContext SetupScoreForPartiallyCompletedGame(PlayContext priorState, GameRecord gameRecord)
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

            Log.Information("Resuming game {GameID} with score {AwayAbbreviation} {AwayTeamScore} @ {HomeTeamScore} {HomeAbbreviation}.",
                gameRecord.GameID,
                gameRecord.AwayTeam.Abbreviation,
                awayTeamScore,
                homeTeamScore,
                gameRecord.HomeTeam.Abbreviation);
            return priorState with
            {
                HomeScore = homeTeamScore,
                AwayScore = awayTeamScore
            };
        }

        private static PlayContext SetupNextPlayAndNextStateForPartiallyCompletedGame(PlayContext priorState, GameRecord gameRecord)
        {
            if (gameRecord.TeamDriveRecords.Count == 0)
            {
                Log.Information("No drive records found for current game. Starting from beginning of game.");
                return priorState.WithNextState(PlayEvaluationState.KickoffDecision) with
                {
                    NextPlay = NextPlayKind.Kickoff
                };
            }
            var lastDrive = gameRecord.TeamDriveRecords.Last();

            // Drive results that would be a first down for the team receiving possession
            if (lastDrive.Result is DriveResult.FieldGoalMiss or
                DriveResult.FumbleLost or
                DriveResult.Interception or
                DriveResult.Punt or
                DriveResult.Safety or
                DriveResult.TurnoverOnDowns)
            {
                Log.Information("Resuming game {GameID} with next play kind FirstDown after defensive change of possession.",
                    gameRecord.GameID);
                // Kind of a hacky way to actually kick off the main loop.
                return priorState.WithFirstDownLineOfScrimmage(priorState.TeamYardToInternalYard(priorState.TeamWithPossession, 25),
                    priorState.TeamWithPossession,
                    "Resuming game. {OffAbbr} has first down at their own 25.", startOfDrive: true)
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

            Log.Information("Resuming game {GameID} with next play kind {NextPlayKind}.",
                gameRecord.GameID, nextPlayKind);
            return priorState.WithNextState(nextState) with
            {
                NextPlay = nextPlayKind
            };
        }

        private TeamStrengthSet EstimateStrengthSetForTeam(Team team, Team requestingTeam)
        {
            double[] strengths = [
                team.RunningOffenseStrength,
                team.RunningDefenseStrength,
                team.PassingOffenseStrength,
                team.PassingDefenseStrength,
                team.OffensiveLineStrength,
                team.DefensiveLineStrength,
                team.KickingStrength,
                team.FieldGoalStrength,
                team.KickReturnStrength,
                team.KickDefenseStrength,
                team.ClockManagementStrength
            ];

            for (var i = 0; i < strengths.Length; i++)
            {
                var strengthRange = random.SampleNormalDistribution(physicsParams["StrengthEstimatorOffsetMean"].Value,
                    physicsParams["StrengthEstimatorOffsetStddev"].Value);
                strengthRange += team.Disposition switch
                {
                    TeamDisposition.UltraConservative => physicsParams["StrengthEstimatorUltraConservativeAdjustment"].Value,
                    TeamDisposition.Conservative => physicsParams["StrengthEstimatorConservativeAdjustment"].Value,
                    TeamDisposition.Insane => physicsParams["StrengthEstimatorInsaneAdjustment"].Value,
                    TeamDisposition.UltraInsane => physicsParams["StrengthEstimatorUltraInsaneAdjustment"].Value,
                    _ => throw new InvalidOperationException($"Team {team.TeamName} has invalid disposition {team.Disposition}.")
                };
                var rangeSample = random.NextDouble() * strengthRange;
                var shouldAdjustDownward = random.Chance(0.5);
                var strengthMultiplier = shouldAdjustDownward
                    ? 1.0 - rangeSample
                    : 1.0 + rangeSample;
                strengths[i] *= strengthMultiplier;
            }

            return new TeamStrengthSet
            {
                IsEstimate = true,
                Team = team.Equals(gameRecord.HomeTeam) ? GameTeam.Home : GameTeam.Away,
                StrengthSetKind = team.Equals(requestingTeam)
                    ? StrengthSetKind.TeamOfItself
                    : StrengthSetKind.TeamOfOpponent,
                RunningOffenseStrength = strengths[0],
                RunningDefenseStrength = strengths[1],
                PassingOffenseStrength = strengths[2],
                PassingDefenseStrength = strengths[3],
                OffensiveLineStrength = strengths[4],
                DefensiveLineStrength = strengths[5],
                KickingStrength = strengths[6],
                FieldGoalStrength = strengths[7],
                KickReturnStrength = strengths[8],
                KickDefenseStrength = strengths[9],
                ClockManagementStrength = strengths[10]
            };
        }

        private double GetTemperatureForGame(GameRecord gameRecord,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            var stadium = gameRecord.Stadium;
            var month = gameRecord.KickoffTime.Month;
            var averageTemperatures = stadium.AverageTemperatures
                .Split(',')
                .Select(double.Parse);
            double averageTemperatureThisMonth;
            if (month >= 8)
            {
                // Indexes 0-4 correspond to August-December
                averageTemperatureThisMonth = averageTemperatures.ElementAt(month - 8);
            }
            else if (month <= 2)
            {
                // Indexes 5-6 correspond to January-February
                averageTemperatureThisMonth = averageTemperatures.ElementAt(month + 4);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Invalid kickoff month {month} for game ID {gameRecord.GameID}.");
            }

            var temperatureStddev = physicsParams["StartTemperatureStddev"].Value;
            return random.SampleNormalDistribution(averageTemperatureThisMonth, temperatureStddev);
        }
    }
}
