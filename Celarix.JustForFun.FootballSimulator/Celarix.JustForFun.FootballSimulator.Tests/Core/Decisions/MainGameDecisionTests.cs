using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Tests.Core.Decisions
{
    public class MainGameDecisionTests
    {
        // MainGameDecision_Run_UltraInsane_AlwaysHailMary

        // Consistent flags:
        //  Is fake play
        //  Adjusted passing desirability multiplier applied

        // In two-minute drill clock disposition
        //  Clock running
        //      Next play is fourth down (RunCore)
        //      Too close to own endzone to spike ball (RunCore)
        //      Offense has any timeouts remaining
        //      Offense has no timeouts remaining
        //  Clock stopped (RunCore with higher pass desirability)
        // Next play is fourth down
        //  Distance to line to gain is null (exception)
        //  Set up desirability adjustments on going for it:
        //      (In field goal range/Out of field goal range,
        //       Score difference is greater/smaller than threshold &&
        //       Game time remaining is greater/smaller than threshold,
        //       Distance to line to gain is greater/smaller than threshold,
        //       Fourth down conversion rate during game is greater/smaller than threshold)
        //      Go for it
        //          Try QB sneak if distance to line to gain is very short
        //          Try RunCore otherwise
        //  
    }
}
