using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.FootballSimulator.Data.Models;

namespace Celarix.JustForFun.FootballSimulator.Models;

internal sealed class InGameTeamStrengths
{
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

    public static InGameTeamStrengths FromTeam(Team team) =>
        new InGameTeamStrengths
        {
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
            ClockManagementStrength = team.ClockManagementStrength
        };
}