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
        Insane
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
        Touchdown
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
}
