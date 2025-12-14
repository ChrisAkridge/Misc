using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Data
{
    public interface IStrengths
    {
        double RunningOffenseStrength { get; set; }
        double RunningDefenseStrength { get; set; }
        double PassingOffenseStrength { get; set; }
        double PassingDefenseStrength { get; set; }
        double OffensiveLineStrength { get; set; }
        double DefensiveLineStrength { get; set; }
        double KickingStrength { get; set; }
        double FieldGoalStrength { get; set; }
        double KickReturnStrength { get; set; }
        double KickDefenseStrength { get; set; }
        double ClockManagementStrength { get; set; }
    }
}
