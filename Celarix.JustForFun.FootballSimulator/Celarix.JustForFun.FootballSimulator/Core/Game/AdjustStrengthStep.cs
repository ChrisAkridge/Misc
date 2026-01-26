using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Game
{
    internal static class AdjustStrengthStep
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
            var offenseStrengthsToAdjust = new List<string>();
            var defenseStrengthsToAdjust = new List<string>();
            var playInvolvement = context.Environment.CurrentPlayContext.PlayInvolvement;

            offenseStrengthsToAdjust.Add(nameof(Team.OffensiveLineStrength));
            defenseStrengthsToAdjust.Add(nameof(Team.DefensiveLineStrength));

            if (playInvolvement.InvolvesOffenseRun)
            {
                offenseStrengthsToAdjust.Add(nameof(Team.RunningOffenseStrength));
                defenseStrengthsToAdjust.Add(nameof(Team.RunningDefenseStrength));
            }
           
            if (playInvolvement.InvolvesOffensePass)
            {
                offenseStrengthsToAdjust.Add(nameof(Team.PassingOffenseStrength));
                defenseStrengthsToAdjust.Add(nameof(Team.PassingDefenseStrength));
            }

            if (playInvolvement.InvolvesKick)
            {
                // Not every kick is a field goal, but since it's the same player, it makes sense to adjust both
                offenseStrengthsToAdjust.Add(nameof(Team.KickingStrength));
                offenseStrengthsToAdjust.Add(nameof(Team.FieldGoalStrength));
            }

            if (playInvolvement.InvolvesDefenseRun)
            {
                if (playInvolvement.InvolvesKick)
                {
                    defenseStrengthsToAdjust.Add(nameof(Team.KickReturnStrength));
                    offenseStrengthsToAdjust.Add(nameof(Team.KickDefenseStrength));
                }
                else
                {
                    defenseStrengthsToAdjust.Add(nameof(Team.RunningOffenseStrength));
                    offenseStrengthsToAdjust.Add(nameof(Team.RunningDefenseStrength));
                }
            }

            var random = context.Environment.RandomFactory.Create();
            IReadOnlyDictionary<string, PhysicsParam> physicsParams = context.Environment.PhysicsParams;
            var adjustmentMean = physicsParams["StrengthAdjustmentMultiplierMean"].Value;
            var adjustmentStddev = physicsParams["StrengthAdjustmentMultiplierStddev"].Value;
            foreach (var strengthPropertyName in offenseStrengthsToAdjust)
            {
                AdjustStrength(offenseTeam, random, strengthPropertyName, adjustmentMean, adjustmentStddev);
            }
            foreach (var strengthPropertyName in defenseStrengthsToAdjust)
            {
                AdjustStrength(defenseTeam, random, strengthPropertyName, adjustmentMean, adjustmentStddev);
            }

            // Rebuild decision parameter strength sets
            Helpers.RebuildStrengthsInDecisionParameters(context, random);

            return context.WithNextState(GameState.DeterminePlayersOnPlay);
        }

        internal static void AdjustStrength(Team team, IRandom random, string strengthPropertyName,
            double adjustmentMean, double adjustmentStddev)
        {
            double multiplier = random.SampleNormalDistribution(adjustmentMean, adjustmentStddev);
            double currentStrength = GetStrength(team, strengthPropertyName);
            double newStrength = currentStrength * multiplier;
            SetStrength(team, strengthPropertyName, newStrength);
        }

        internal static double GetStrength(Team team, string strengthPropertyName)
        {
            return strengthPropertyName switch
            {
                nameof(Team.RunningOffenseStrength) => team.RunningOffenseStrength,
                nameof(Team.RunningDefenseStrength) => team.RunningDefenseStrength,
                nameof(Team.PassingOffenseStrength) => team.PassingOffenseStrength,
                nameof(Team.PassingDefenseStrength) => team.PassingDefenseStrength,
                nameof(Team.OffensiveLineStrength) => team.OffensiveLineStrength,
                nameof(Team.DefensiveLineStrength) => team.DefensiveLineStrength,
                nameof(Team.KickingStrength) => team.KickingStrength,
                nameof(Team.FieldGoalStrength) => team.FieldGoalStrength,
                nameof(Team.KickReturnStrength) => team.KickReturnStrength,
                nameof(Team.KickDefenseStrength) => team.KickDefenseStrength,
                nameof(Team.ClockManagementStrength) => team.ClockManagementStrength,
                _ => throw new ArgumentException($"Invalid strength property name: {strengthPropertyName}"),
            };
        }

        internal static void SetStrength(Team team, string strengthPropertyName, double value)
        {
            switch (strengthPropertyName)
            {
                case nameof(Team.RunningOffenseStrength):
                    team.RunningOffenseStrength = value;
                    break;
                case nameof(Team.RunningDefenseStrength):
                    team.RunningDefenseStrength = value;
                    break;
                case nameof(Team.PassingOffenseStrength):
                    team.PassingOffenseStrength = value;
                    break;
                case nameof(Team.PassingDefenseStrength):
                    team.PassingDefenseStrength = value;
                    break;
                case nameof(Team.OffensiveLineStrength):
                    team.OffensiveLineStrength = value;
                    break;
                case nameof(Team.DefensiveLineStrength):
                    team.DefensiveLineStrength = value;
                    break;
                case nameof(Team.KickingStrength):
                    team.KickingStrength = value;
                    break;
                case nameof(Team.FieldGoalStrength):
                    team.FieldGoalStrength = value;
                    break;
                case nameof(Team.KickReturnStrength):
                    team.KickReturnStrength = value;
                    break;
                case nameof(Team.KickDefenseStrength):
                    team.KickDefenseStrength = value;
                    break;
                case nameof(Team.ClockManagementStrength):
                    team.ClockManagementStrength = value;
                    break;
                default:
                    throw new ArgumentException($"Invalid strength property name: {strengthPropertyName}");
            }
        }
    }
}
