using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Core
{
    internal sealed class GameLoop
    {
        private Random random;
        private FootballContext footballContext;
        private GameRecord gameRecord;
        private readonly IReadOnlyDictionary<string, PhysicsParam> physicsParams;

        // Game clock
        private int periodNumber;
        private int secondsRemainingInPeriod;

        // Game scoreboard
        private int awayTeamScore;
        private int homeTeamScore;
        private int awayTimeoutsRemaining;
        private int homeTimeoutsRemaining;
        private GameTeam possessingTeam;

        // Simulation parameters
        private double baseWindDirection;
        private double baseWindSpeed;
        private TeamStrengthSet awayTeamActualStrengths;
        private TeamStrengthSet homeTeamActualStrengths;
        private TeamStrengthSet awayTeamEstimateOfAway;
        private TeamStrengthSet awayTeamEstimateOfHome;
        private TeamStrengthSet homeTeamEstimateOfAway;
        private TeamStrengthSet homeTeamEstimateOfHome;

        // Gameplay flags
        private bool pointAfterTouchdownFlag;
        private bool onePointOffensiveSafetyFlag;
        private bool dualPossessionFlag;

        public GameLoop(FootballContext footballContext, Random random, GameRecord gameRecord)
        {
            this.footballContext = footballContext;
            this.random = random;
            this.gameRecord = gameRecord;
            physicsParams = footballContext.GetAllPhysicsParams();

            foreach (var param in physicsParams)
            {
                Log.Debug("Loaded physics parameter {ParamName} with value {ParamValue} {ParamUnit}.", param.Key, param.Value.Value, param.Value.Unit);
            }
        }

        public void Initialize()
        {
            SetupClockForPartiallyCompletedGame();

            if (periodNumber == 1 && secondsRemainingInPeriod == Constants.SecondsPerQuarter)
            {
                // New game initialization
                awayTeamScore = 0;
                homeTeamScore = 0;
                possessingTeam = random.Next(2) == 0 ? GameTeam.Away : GameTeam.Home;
            }
            else
            {
                // Load existing game state from gameRecord
                SetupScoreForPartiallyCompletedGame();

                // We only write completed drives to the database, so the possessing team is the team
                // that DIDN'T have possession on the last drive.
                possessingTeam = gameRecord.TeamDriveRecords.Last().Team.Equals(gameRecord.HomeTeam)
                    ? GameTeam.Away
                    : GameTeam.Home;
            }

            // We don't store timeouts used in the database, so always start with 3 per team
            awayTimeoutsRemaining = 3;
            homeTimeoutsRemaining = 3;

            // Initialize weather conditions
            baseWindDirection = random.NextDouble() * 360.0;
            Log.Information("Initialized base wind direction to {WindDirection} degrees (0 = toward home, 180 = toward away).", baseWindDirection);
            baseWindSpeed = random.SampleNormalDistribution(gameRecord.Stadium.AverageWindSpeed, physicsParams["StartWindSpeedStddev"].Value);
            Log.Information("Initialized base wind speed to {WindSpeed} mph.", baseWindSpeed);

            // Initialize team strength sets
            awayTeamActualStrengths = TeamStrengthSet.FromTeamDirectly(gameRecord.AwayTeam, GameTeam.Away);
            homeTeamActualStrengths = TeamStrengthSet.FromTeamDirectly(gameRecord.HomeTeam, GameTeam.Home);
            awayTeamEstimateOfAway = EstimateStrengthSetForTeam(gameRecord.AwayTeam, gameRecord.AwayTeam);
            awayTeamEstimateOfHome = EstimateStrengthSetForTeam(gameRecord.HomeTeam, gameRecord.AwayTeam);
            homeTeamEstimateOfAway = EstimateStrengthSetForTeam(gameRecord.AwayTeam, gameRecord.HomeTeam);
            homeTeamEstimateOfHome = EstimateStrengthSetForTeam(gameRecord.HomeTeam, gameRecord.HomeTeam);
        }

        private void SetupClockForPartiallyCompletedGame()
        {
            if (gameRecord.TeamDriveRecords.Count == 0)
            {
                Log.Information("No drive records found for current game. Starting from beginning of game.");
                periodNumber = 1;
                secondsRemainingInPeriod = Constants.SecondsPerQuarter;
                return;
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

            secondsRemainingInPeriod = (periodNumber <= 4
                ? Constants.SecondsPerQuarter
                : Constants.SecondsPerOvertimePeriod) - secondsIntoPeriod;
            Log.Information("Resuming game {GameID} from period {PeriodNumber} with {SecondsRemaining} seconds remaining in period.",
                gameRecord.GameID, periodNumber, secondsRemainingInPeriod);;
        }

        private void SetupScoreForPartiallyCompletedGame()
        {
            DriveResult[] scoringDriveResults = [
                DriveResult.FieldGoalSuccess,
                DriveResult.TouchdownNoXP,
                DriveResult.TouchdownWithXP,
                DriveResult.TouchdownWithTwoPointConversion,
                DriveResult.TouchdownWithOffensiveSafety
            ];

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
    }
}
