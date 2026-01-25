using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Game
{
    internal static class StartStep
    {
        public static GameContext Run(GameContext context)
        {
            // LoadGameStep or ResumePartialGameStep have already set up the initial environment,
            // so there's nothing to do here.
            return context.WithNextState(GameState.EvaluatingPlay);
        }
    }
}
