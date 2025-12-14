using Celarix.JustForFun.FootballSimulator.Core.Decisions;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Outcomes
{
    internal static class ReturnableKickOutcome
    {
        public static GameState Run(GameState priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            if (priorState.AdditionalParameters is null ||
                !priorState.HasAdditionalParameter("KickActualYard"))
            {
                throw new ArgumentException("ReturnableKickOutcome requires 'KickActualYard' in AdditionalParameters.");
            }

            var kickActualYard = priorState.GetAdditionalParameterOrDefault<int>("KickActualYard");
            var receivingTeam = priorState.TeamWithPossession.Opponent();
            var receivingTeamKickReturnStrength = parameters.GetActualStrengthsForTeam(receivingTeam).KickReturnStrength;
            var alpha = Math.Log10(receivingTeamKickReturnStrength);
            var beta = -alpha;
            var gamma = Math.Pow(10, beta);
            var kickRecoveryChance = 1 - gamma;

            if (parameters.Random.Chance(kickRecoveryChance))
            {
                Log.Verbose("ReturnableKickOutcome: Kick recovered cleanly by receiving team.");
                return priorState.WithNextState(GameplayNextState.SignalFairCatchDecision) with
                {
                    TeamWithPossession = receivingTeam,
                    LineOfScrimmage = kickActualYard,
                    ClockRunning = false,
                    LastPlayDescriptionTemplate = "{DefAbbr} signals for a fair catch on the kick return."
                };
            }
            else
            {
                Log.Verbose("ReturnableKickOutcome: Kick not recovered cleanly by receiving team; live ball.");
                return priorState.WithNextState(GameplayNextState.FumbledLiveBallOutcome);
            }
        }
    }
}
