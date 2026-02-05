using Celarix.JustForFun.FootballSimulator.Core.Game;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
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
            var gameContext = context.Environment.CurrentGameContext ?? throw new InvalidOperationException("Game state machine is not initialized!");
            context.Environment.CurrentGameContext = gameContext.NextState switch
            {
                GameState.AdjustClock => AdjustClockStep.Run(gameContext),
                GameState.AdjustStrengths => AdjustStrengthStep.Run(gameContext),
                GameState.DeterminePlayersOnPlay => DeterminePlayersOnPlayStep.Run(gameContext),
                GameState.EndGame => EndGameStep.Run(gameContext),
                GameState.EvaluatingPlay => EvaluatingPlayStep.Run(gameContext, out evaluatingPlaySignal),
                GameState.InjuryCheck => InjuryCheckStep.Run(gameContext),
                GameState.PostPlayCheck => PostPlayCheck.Run(gameContext),
                GameState.StartNextPeriod => StartNextPeriodStep.Run(gameContext),
                GameState.Start => Game.StartStep.Run(gameContext),
                _ => throw new InvalidOperationException("Invalid game state encountered.")
            };

            context.Environment.DebugContextWriter.WriteContext(context.Environment.CurrentGameContext, context.Environment);

            var nextGameState = context.Environment.CurrentGameContext.NextState;
            if (nextGameState == GameState.EvaluatingPlay && evaluatingPlaySignal == EvaluatingPlaySignal.InProgress)
            {
                inGameSignal = InGameSignal.PlayEvaluationStep;
                Log.Information("InGameStep: Game state machine moved to {GameState}.", context.Environment.CurrentGameContext!.NextState);
                return context.WithNextState(SystemState.InGame);
            }

            if (nextGameState == GameState.EndGame)
            {
                inGameSignal = InGameSignal.GameCompleted;
                Log.Information("InGameStep: Game state machine moved to {GameState}.", context.Environment.CurrentGameContext!.NextState);
                return context.WithNextState(SystemState.PostGame);
            }

            inGameSignal = InGameSignal.GameStateAdvanced;
            Log.Information("InGameStep: Game state machine moved to {GameState}.", context.Environment.CurrentGameContext!.NextState);
            return context.WithNextState(SystemState.InGame);
        }
    }
}
