using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Data.Models
{
    public enum Conference
    {
        AFC,
        NFC
    }

    public enum Division
    {
        North,
        South,
        East,
        West,
        Extra
    }
    
    public enum TeamDisposition
    {
        UltraConservative,
        Conservative,
        Insane,
        UltraInsane
    }

    public enum GameType
    {
        Preseason,
        RegularSeason,
        Postseason
    }

    public enum StadiumWeather
    {
        Sunny,
        Cloudy,
        Raining,
        Snowing
    }

    public enum GameTeam
    {
        Home,
        Away
    }

    public enum DriveResult
    {
        Punt,
        TurnoverOnDowns,
        FumbleLost,
        Interception,
        Safety,
        FieldGoalMiss,
        FieldGoalSuccess,
        TouchdownNoXP,
        // Also represents a touchdown (6 points for the offense) and a defensive safety (1 point for the offense)
        TouchdownWithXP,
        TouchdownWithTwoPointConversion,
        TouchdownWithOffensiveSafety,
        TouchdownWithDefensiveScore,
        EndOfHalf
    }

    public enum GameResult
    {
        Win,
        Loss,
        Tie
    }

    public enum BasicPlayerPosition
    {
	    Quarterback,
	    Offense,
	    Defense,
	    Kicker
    }

    public enum WinningTeam
    {
        Away,
        Home,
        Tie
    }
}
