using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Game
{
    internal static class InjuryCheckStep
    {
        public static GameContext Run(GameContext context)
        {
            var offenseIsHomeTeam = context.Environment.CurrentPlayContext!.TeamWithPossession == GameTeam.Home;
            var offenseTeam = offenseIsHomeTeam
                ? context.Environment.CurrentGameRecord!.HomeTeam
                : context.Environment.CurrentGameRecord!.AwayTeam;
            var defenseTeam = offenseIsHomeTeam
                ? context.Environment.CurrentGameRecord!.AwayTeam
                : context.Environment.CurrentGameRecord!.HomeTeam;
            var random = context.Environment.RandomFactory.Create();
            var kickoffTime = context.Environment.CurrentGameRecord!.KickoffTime;

            var offensivePlayers = context.OffensePlayersOnPlay;
            var defensivePlayers = context.DefensePlayersOnPlay;
            var baseInjuryChancePerPlay = context.Environment.PhysicsParams["BaseInjuryChancePerPlay"].Value;
            var injuryTemperatureStep = context.Environment.PhysicsParams["InjuryTemperatureStep"].Value;
            var injuryTemperatureMultiplierPerStep = context.Environment.PhysicsParams["InjuryTemperatureMultiplierPerStep"].Value;

            var currentTemperature = context.Environment.CurrentPlayContext!.AirTemperature;
            var awayTeamAcclimatedTemperature = context.AwayTeamAcclimatedTemperature;
            var homeTeamAcclimatedTemperature = context.HomeTeamAcclimatedTemperature;

            var awayTeamTemperatureDifference = Math.Abs(currentTemperature - awayTeamAcclimatedTemperature);
            var awayTeamTemperatureSteps = Math.Floor(awayTeamTemperatureDifference / injuryTemperatureStep);
            var awayTeamInjuryChanceMultiplier = Math.Pow(injuryTemperatureMultiplierPerStep, awayTeamTemperatureSteps);

            var homeTeamTemperatureDifference = Math.Abs(currentTemperature - homeTeamAcclimatedTemperature);
            var homeTeamTemperatureSteps = Math.Floor(homeTeamTemperatureDifference / injuryTemperatureStep);
            var homeTeamInjuryChanceMultiplier = Math.Pow(injuryTemperatureMultiplierPerStep, homeTeamTemperatureSteps);

            var awayTeamInjuryChancePerPlay = baseInjuryChancePerPlay * awayTeamInjuryChanceMultiplier;
            var homeTeamInjuryChancePerPlay = baseInjuryChancePerPlay * homeTeamInjuryChanceMultiplier;
            var injuryStrengthAdjustmentMean = context.Environment.PhysicsParams["InjuryStrengthAdjustmentMean"].Value;
            var injuryStrengthAdjustmentStdDev = context.Environment.PhysicsParams["InjuryStrengthAdjustmentStdDev"].Value;

            var newInjuryRecoveries = new List<InjuryRecovery>();

            foreach (var player in offensivePlayers!)
            {
                var injuryRoll = random.Chance(offenseIsHomeTeam ? homeTeamInjuryChancePerPlay : awayTeamInjuryChancePerPlay);
                if (injuryRoll)
                {
                    context.AddTag("offensive-injury");
                    Log.Verbose("InjuryCheckStep: Player {PlayerName} ({TeamName}) injured on play.",
                        player.Player.FirstName + " " + player.Player.LastName,
                        offenseTeam.TeamName);
                    newInjuryRecoveries.AddRange(InjurePlayerAndCreateRecoveries(random, player, offenseTeam, context.Environment.PhysicsParams, kickoffTime));
                }
            }

            foreach (var player in defensivePlayers!)
            {
                var injuryRoll = random.Chance(offenseIsHomeTeam ? awayTeamInjuryChancePerPlay : homeTeamInjuryChancePerPlay);
                if (injuryRoll)
                {
                    context.AddTag("defensive-injury");
                    Log.Verbose("InjuryCheckStep: Player {PlayerName} ({TeamName}) injured on play.",
                        player.Player.FirstName + " " + player.Player.LastName,
                        defenseTeam.TeamName);
                    newInjuryRecoveries.AddRange(InjurePlayerAndCreateRecoveries(random, player, defenseTeam, context.Environment.PhysicsParams, kickoffTime));
                }
            }

            Helpers.RebuildStrengthsInDecisionParameters(context, random);
            context.Environment.FootballRepository.AddInjuryRecoveries(newInjuryRecoveries);
            context.Environment.FootballRepository.SaveChanges();

            Log.Verbose("InjuryCheckStep: Completed injury checks for play.");
            return context.WithNextState(GameState.AdjustClock);
        }

        internal static IEnumerable<InjuryRecovery> InjurePlayerAndCreateRecoveries(IRandom random, PlayerRosterPosition playerRosterPosition,
            Team team, IReadOnlyDictionary<string, PhysicsParam> physicsParams, DateTimeOffset gameKickoffTime)
        {
            var strengths = team as IStrengths;
            var injuryStrengthAdjustmentMean = physicsParams["InjuryStrengthAdjustmentMean"].Value;
            var injuryStrengthAdjustmentStdDev = physicsParams["InjuryStrengthAdjustmentStdDev"].Value;
            var injuryRecoveryDaysMean = physicsParams["InjuryRecoveryDaysMean"].Value;
            var injuryRecoveryDaysStdDev = physicsParams["InjuryRecoveryDaysStdDev"].Value;

            string[] strengthsToAdjust = playerRosterPosition.Position switch
            {
                BasicPlayerPosition.Offense =>
                [
                    nameof(strengths.RunningOffenseStrength),
                    nameof(strengths.OffensiveLineStrength),
                    nameof(strengths.KickReturnStrength)
                ],
                BasicPlayerPosition.Defense =>
                [
                    nameof(strengths.RunningDefenseStrength),
                    nameof(strengths.DefensiveLineStrength),
                    nameof(strengths.KickDefenseStrength)
                ],
                BasicPlayerPosition.Kicker =>
                [
                    nameof(strengths.KickingStrength),
                    nameof(strengths.FieldGoalStrength)
                ],
                BasicPlayerPosition.Quarterback =>
                [
                    nameof(strengths.PassingOffenseStrength)
                ],
                _ => throw new InvalidOperationException("Unknown player position for injury strength adjustment.")
            };

            foreach (var strengthName in strengthsToAdjust)
            {
                var currentValue = (double)strengths!.GetType().GetProperty(strengthName)!.GetValue(strengths)!;
                var adjustment = random.SampleNormalDistribution(injuryStrengthAdjustmentMean, injuryStrengthAdjustmentStdDev);
                var newValue = currentValue * adjustment;
                strengths.GetType().GetProperty(strengthName)!.SetValue(strengths, newValue);
                
                var recoveryDays = (int)Math.Ceiling(random.SampleNormalDistribution(injuryRecoveryDaysMean, injuryRecoveryDaysStdDev));
                recoveryDays = recoveryDays < 3 ? 3 : recoveryDays;
                var recoverOn = gameKickoffTime.AddDays(recoveryDays);
                var recovery = new InjuryRecovery
                {
                    TeamID = team.TeamID,
                    Strength = strengthName,
                    StrengthDelta = newValue - currentValue,
                    RecoverOn = recoverOn
                };
                yield return recovery;
            }
        }
    }
}
