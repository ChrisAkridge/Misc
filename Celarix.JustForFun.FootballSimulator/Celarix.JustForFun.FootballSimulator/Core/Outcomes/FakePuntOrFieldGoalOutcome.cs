using Celarix.JustForFun.FootballSimulator.Core.Decisions;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Outcomes
{
    internal static class FakePuntOrFieldGoalOutcome
    {
        public static PlayContext Run(PlayContext priorState)
        {
            var parameters = priorState.Environment!.DecisionParameters;
            var physicsParams = priorState.Environment.PhysicsParams;

            var priorStateWithSimulatedLineOfScrimmage = priorState with
            {
                LineOfScrimmage = priorState.AddYardsForPossessingTeam(priorState.LineOfScrimmage, -15).Round()
            };

            // This is one of the few times we call a decision directly instead of letting the GameLoop handle it.
            // This is because we want to rerun the main game decision with a flag that prevents another fake punt/FG.
            return MainGameDecision.Run(priorStateWithSimulatedLineOfScrimmage)
                .WithAdditionalParameter<bool?>("IsFakePlay", true);
        }
    }
}
