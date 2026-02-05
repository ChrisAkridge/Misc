using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    public sealed class TeamStrengthSet : IStrengths
    {
        public bool IsEstimate { get; init; }
        public GameTeam Team { get; init; }
        public StrengthSetKind StrengthSetKind { get; init; }
        public double RunningOffenseStrength { get; set; }
        public double RunningDefenseStrength { get; set; }
        public double PassingOffenseStrength { get; set; }
        public double PassingDefenseStrength { get; set; }
        public double OffensiveLineStrength { get; set; }
        public double DefensiveLineStrength { get; set; }
        public double KickingStrength { get; set; }
        public double FieldGoalStrength { get; set; }
        public double KickReturnStrength { get; set; }
        public double KickDefenseStrength { get; set; }
        public double ClockManagementStrength { get; set; }

        public double OverallAverageStrength =>
            (RunningOffenseStrength +
             RunningDefenseStrength +
             PassingOffenseStrength +
             PassingDefenseStrength +
             OffensiveLineStrength +
             DefensiveLineStrength +
             KickingStrength +
             FieldGoalStrength +
             KickReturnStrength +
             KickDefenseStrength +
             ClockManagementStrength) / 11.0;

        public static TeamStrengthSet FromTeamDirectly(Team team, GameTeam gameTeam)
        {
            return new TeamStrengthSet
            {
                IsEstimate = false,
                Team = gameTeam,
                RunningOffenseStrength = team.RunningOffenseStrength,
                RunningDefenseStrength = team.RunningDefenseStrength,
                PassingOffenseStrength = team.PassingOffenseStrength,
                PassingDefenseStrength = team.PassingDefenseStrength,
                OffensiveLineStrength = team.OffensiveLineStrength,
                DefensiveLineStrength = team.DefensiveLineStrength,
                KickingStrength = team.KickingStrength,
                FieldGoalStrength = team.FieldGoalStrength,
                KickReturnStrength = team.KickReturnStrength,
                KickDefenseStrength = team.KickDefenseStrength,
                ClockManagementStrength = team.ClockManagementStrength,
            };
        }
    }
}
