using Celarix.JustForFun.FootballSimulator.Core.Decisions;
using Celarix.JustForFun.FootballSimulator.Core.Outcomes;
using Celarix.JustForFun.FootballSimulator.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Game
{
    internal static class EvaluatingPlayStep
    {
        public static GameContext Run(GameContext context, out EvaluatingPlaySignal evaluatingPlaySignal)
        {
            PlayContext playContext = context.Environment.CurrentPlayContext!;

            if (playContext.NextState == PlayEvaluationState.PlayEvaluationComplete)
            {
                // We're coming back to start a new play evaluation. Reset the play involvement.
                playContext = playContext with
                {
                    PlayInvolvement = Helpers.CreateInitialPlayInvolvement()
                };
            }

            context.Environment.CurrentPlayContext = playContext.NextState switch
            {
                PlayEvaluationState.FreeKickDecision => FreeKickDecision.Run(playContext),
                PlayEvaluationState.KickoffDecision => KickoffDecision.Run(playContext),
                PlayEvaluationState.MainGameDecision => MainGameDecision.Run(playContext),
                PlayEvaluationState.ReturnFumbledOrInterceptedBallDecision => ReturnFumbledOrInterceptedBallDecision.Run(playContext),
                PlayEvaluationState.SignalFairCatchDecision => SignalFairCatchDecision.Run(playContext),
                PlayEvaluationState.TouchdownDecision => TouchdownDecision.Run(playContext),
                PlayEvaluationState.FakeFieldGoalOutcome => FakePuntOrFieldGoalOutcome.Run(playContext),
                PlayEvaluationState.FakePuntOutcome => FakePuntOrFieldGoalOutcome.Run(playContext),
                PlayEvaluationState.FumbledLiveBallOutcome => FumbledLiveBallOutcome.Run(playContext),
                PlayEvaluationState.FumbleOrInterceptionReturnOutcome => FumbleOrInterceptionReturnOutcome.Run(playContext),
                PlayEvaluationState.HailMaryOutcome => HailMaryOutcome.Run(playContext),
                PlayEvaluationState.KickOrPuntReturnOutcome => KickOrPuntReturnOutcome.Run(playContext),
                PlayEvaluationState.NormalKickoffOutcome => NormalKickoffOutcome.Run(playContext),
                PlayEvaluationState.OnsideKickAttemptOutcome => OnsideKickAttemptOutcome.Run(playContext),
                PlayEvaluationState.PuntOutcome => PuntOutcome.Run(playContext),
                PlayEvaluationState.QBSneakOutcome => QBSneakOutcome.Run(playContext),
                PlayEvaluationState.ReturnableKickOutcome => ReturnableKickOutcome.Run(playContext),
                PlayEvaluationState.ReturnablePuntOutcome => ReturnablePuntOutcome.Run(playContext),
                PlayEvaluationState.StandardShortPassingPlayOutcome => StandardPassingPlayOutcome.Run(playContext, PassAttemptDistance.Short),
                PlayEvaluationState.StandardMediumPassingPlayOutcome => StandardPassingPlayOutcome.Run(playContext, PassAttemptDistance.Medium),
                PlayEvaluationState.StandardLongPassingPlayOutcome => StandardPassingPlayOutcome.Run(playContext, PassAttemptDistance.Long),
                PlayEvaluationState.StandardRushingPlayOutcome => StandardRushingPlayOutcome.Run(playContext),
                PlayEvaluationState.TwoPointConversionAttemptOutcome => TwoPointConversionAttemptOutcome.Run(playContext),
                PlayEvaluationState.VictoryFormationOutcome => VictoryFormationOutcome.Run(playContext),
                _ => throw new InvalidOperationException("Invalid play evaluation state encountered.")
            };

            evaluatingPlaySignal = playContext.NextState switch
            {
                PlayEvaluationState.PlayEvaluationComplete => EvaluatingPlaySignal.PlayEvaluationComplete,
                _ => EvaluatingPlaySignal.InProgress
            };

            return playContext.NextState == PlayEvaluationState.PlayEvaluationComplete
                ? context.WithNextState(GameState.AdjustStrengths)
                : context.WithNextState(GameState.EvaluatingPlay);
        }
    }
}
