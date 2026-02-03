using Celarix.JustForFun.FootballSimulator.Core.Decisions;
using Celarix.JustForFun.FootballSimulator.Core.Outcomes;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
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
                // We're coming back to start a new play evaluation. Reset the play context.
                var playCountResets = playContext.TeamWithPossession != context.TeamWithPossession
                    || playContext.NextPlay is NextPlayKind.Kickoff or NextPlayKind.FreeKick;
                var playNumber = playCountResets ? 1 : context.PlayCountOnDrive + 1;
                Log.Verbose("EvaluatingPlayStep: Starting evaluation of play number {PlayNumber} for {TeamWithPossession}.",
                    playNumber,
                    context.TeamWithPossession);

                playContext = playContext with
                {
                    NextState = playContext.NextPlay switch
                    {
                        NextPlayKind.Kickoff => PlayEvaluationState.KickoffDecision,
                        NextPlayKind.ConversionAttempt => PlayEvaluationState.MainGameDecision,
                        NextPlayKind.FreeKick => PlayEvaluationState.FreeKickDecision,
                        NextPlayKind.FirstDown or
                        NextPlayKind.SecondDown or
                        NextPlayKind.ThirdDown or
                        NextPlayKind.FourthDown => PlayEvaluationState.MainGameDecision,
                        _ => throw new InvalidOperationException("Invalid next play kind encountered.")
                    },
                    PlayInvolvement = Helpers.CreateInitialPlayInvolvement(),
                    AwayScoredThisPlay = false,
                    HomeScoredThisPlay = false,
                    DriveResult = null,
                    PossessionOnPlay = PossessionOnPlay.None,
                    TeamCallingTimeout = null
                };

                context = context with
                {
                    OffensePlayersOnPlay = null,
                    DefensePlayersOnPlay = null,
                    TeamWithPossession = playContext.TeamWithPossession,
                    PlayCountOnDrive = playNumber
                };

                context.Environment.DebugContextWriter.WriteContext(playContext);
                context.Environment.DebugContextWriter.WriteContext(context);
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

            context.Environment.DebugContextWriter.WriteContext(context.Environment.CurrentPlayContext);

            evaluatingPlaySignal = playContext.NextState switch
            {
                PlayEvaluationState.PlayEvaluationComplete => EvaluatingPlaySignal.PlayEvaluationComplete,
                _ => EvaluatingPlaySignal.InProgress
            };

            Log.Information("EvaluatingPlayStep: Play evaluation state machine advanced to {PlayState}.",
                context.Environment.CurrentPlayContext!.NextState);
            return playContext.NextState == PlayEvaluationState.PlayEvaluationComplete
                ? context.WithNextState(GameState.AdjustStrengths)
                : context.WithNextState(GameState.EvaluatingPlay);
        }
    }
}
