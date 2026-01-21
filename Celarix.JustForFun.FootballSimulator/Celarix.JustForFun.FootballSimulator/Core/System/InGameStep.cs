using Celarix.JustForFun.FootballSimulator.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.System
{
    internal static class InGameStep
    {
        public static SystemContext Run(SystemContext context, out InGameSignal inGameSignal)
        {
            EvaluatingPlaySignal evaluatingPlaySignal = EvaluatingPlaySignal.None;

            if (context.Environment.CurrentGameContext == null)
            {
                throw new InvalidOperationException("Game state machine is not initialized!");
            }

            context.Environment.CurrentGameContext = context.Environment.CurrentGameContext.NextState switch
            {

            };

            var nextGameState = context.Environment.CurrentGameContext.NextState;
            if (nextGameState == GameState.EvaluatingPlay && evaluatingPlaySignal == EvaluatingPlaySignal.InProgress)
            {
                inGameSignal = InGameSignal.PlayEvaluationStep;
                return context.WithNextState(SystemState.InGame);
            }

            if (nextGameState == GameState.EndGame)
            {
                inGameSignal = InGameSignal.GameCompleted;
                return context.WithNextState(SystemState.PostGame);
            }

            inGameSignal = InGameSignal.GameStateAdvanced;
            return context.WithNextState(SystemState.InGame);
        }
    }
}
