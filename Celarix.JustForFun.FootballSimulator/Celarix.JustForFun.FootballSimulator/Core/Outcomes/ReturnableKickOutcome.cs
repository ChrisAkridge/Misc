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
            IReadOnlyDictionary<string, PhysicsParam> physicsParams,
            double kickActualYard)
        {
            var receivingTeam = priorState.OtherTeam(priorState.TeamWithPossession);
            var receivingTeamKickReturnStrength = parameters.GetActualStrengthsForTeam(receivingTeam).KickReturnStrength;
            var alpha = Math.Log10(receivingTeamKickReturnStrength);
            var beta = -alpha;
            var gamma = Math.Pow(10, beta);
            var kickRecoveryChance = 1 - gamma;

            if (parameters.Random.Chance(kickRecoveryChance))
            {
                Log.Verbose("ReturnableKickOutcome: Kick recovered cleanly by receiving team.");
                return SignalFairCatchDecision.Run(priorState with
                    {
                        TeamWithPossession = receivingTeam,
                        LineOfScrimmage = kickActualYard.Round(),
                        LastPlayDescriptionTemplate = "{DefAbbr} signals for a fair catch on the kick return."
                },
                    parameters,
                    physicsParams);
            }
            else
            {
                Log.Verbose("ReturnableKickOutcome: Kick not recovered cleanly by receiving team; live ball.");
                return FumbledLiveBallOutcome.Run(priorState,
                        parameters,
                        physicsParams,
                        kickActualYard);
            }
        }
    }
}
