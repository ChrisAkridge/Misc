using Celarix.JustForFun.FootballSimulator.Core.Decisions;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Outcomes
{
    internal static class TwoPointConversionAttemptOutcome
    {
        public static GameState Run(GameState priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            var possessingTeamScore = priorState.GetScoreForTeam(priorState.TeamWithPossession);
            var opposingTeamScore = priorState.GetScoreForTeam(priorState.TeamWithPossession.Opponent());

            var mainGameDecisionResult = MainGameDecision.Run(priorState,
                parameters,
                physicsParams);

            var newPossessingTeamScore = mainGameDecisionResult.GetScoreForTeam(priorState.TeamWithPossession);
            var newOpposingTeamScore = mainGameDecisionResult.GetScoreForTeam(priorState.TeamWithPossession.Opponent());

            if (newPossessingTeamScore - possessingTeamScore == 6)
            {
                // Play was a touchdown, rewrite history and make it a two-point conversion
                Log.Verbose("TwoPointConversionAttemptOutcome: Successful two-point conversion.");
                return priorState.WithScoreChange(priorState.TeamWithPossession, 2)
                    .WithNextState(GameplayNextState.PlayEvaluationComplete)    
                with
                {
                    NextPlay = NextPlayKind.Kickoff,
                    LineOfScrimmage = priorState.TeamYardToInternalYard(priorState.TeamWithPossession, 35),
                    LineToGain = null,
                    ClockRunning = false,
                    LastPlayDescriptionTemplate = "{OffAbbr} {OffPlayer0} successful two-point conversion."
                };
            }
            else if (newPossessingTeamScore - possessingTeamScore == 2)
            {
                // Play was a defensive safety, rewrite history and make it a one-point defensive safety
                Log.Verbose("TwoPointConversionAttemptOutcome: Defensive safety on two-point conversion attempt.");
                return priorState.WithScoreChange(priorState.TeamWithPossession, 1)
                    .WithNextState(GameplayNextState.PlayEvaluationComplete)
                with
                {
                    NextPlay = NextPlayKind.Kickoff,
                    LineOfScrimmage = priorState.TeamYardToInternalYard(priorState.TeamWithPossession, 35),
                    LineToGain = null,
                    ClockRunning = false,
                    LastPlayDescriptionTemplate = "{OffAbbr} {OffPlayer0} fumbled and was recovered by defense, but {DefAbbr} downed in own endzone for a one-point defensive safety."
                };
            }
            else if (newOpposingTeamScore - opposingTeamScore == 2)
            {
                // Play was an offensive safety, rewrite history and make it a one-point offensive safety!
                Log.Verbose("TwoPointConversionAttemptOutcome: Offensive safety on two-point conversion attempt.");
                return priorState.WithScoreChange(priorState.TeamWithPossession.Opponent(), 1)
                    .WithNextState(GameplayNextState.PlayEvaluationComplete)
                with
                {
                    NextPlay = NextPlayKind.Kickoff,
                    LineOfScrimmage = priorState.TeamYardToInternalYard(priorState.TeamWithPossession.Opponent(), 35),
                    LineToGain = null,
                    ClockRunning = false,
                    LastPlayDescriptionTemplate = "{OffAbbr} {OffPlayer0} was tackled in own endzone for a one-point offensive safety!"
                };
            }
            else if (newOpposingTeamScore != opposingTeamScore)
            {
                // Play was a defensive score, rewrite history and make it a two-point defensive score
                Log.Verbose("TwoPointConversionAttemptOutcome: Defensive score on two-point conversion attempt.");
                return priorState.WithScoreChange(priorState.TeamWithPossession.Opponent(), 2)
                    .WithNextState(GameplayNextState.PlayEvaluationComplete)
                with
                {
                    NextPlay = NextPlayKind.Kickoff,
                    LineOfScrimmage = priorState.TeamYardToInternalYard(priorState.TeamWithPossession.Opponent(), 35),
                    LineToGain = null,
                    ClockRunning = false,
                    LastPlayDescriptionTemplate = "{OffAbbr} {OffPlayer0} fumbled and was recovered by defense for a two-point defensive score!"
                };
            }

            // Play was an unsuccessful two-point conversion attempt
            Log.Verbose("TwoPointConversionAttemptOutcome: Unsuccessful two-point conversion attempt.");
            return priorState.WithNextState(GameplayNextState.PlayEvaluationComplete) with
            {
                NextPlay = NextPlayKind.Kickoff,
                LineOfScrimmage = priorState.TeamYardToInternalYard(priorState.TeamWithPossession.Opponent(), 35),
                LineToGain = null,
                ClockRunning = false,
                LastPlayDescriptionTemplate = "{OffAbbr} {OffPlayer0} unsuccessful two-point conversion attempt."
            };
        }
    }
}
