using Celarix.JustForFun.FootballSimulator.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Data
{
    public static class Extensions
    {
        public static bool IsTouchdown(this DriveResult result)
        {
            return result switch
            {
                DriveResult.TouchdownNoXP => true,
                DriveResult.TouchdownWithXP => true,
                DriveResult.TouchdownWithTwoPointConversion => true,
                DriveResult.TouchdownWithOffensiveSafety => true,
                DriveResult.TouchdownWithDefensiveScore => true,
                _ => false
            };
        }
    }
}
