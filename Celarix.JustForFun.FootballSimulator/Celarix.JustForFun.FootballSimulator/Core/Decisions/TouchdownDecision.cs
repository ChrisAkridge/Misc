using Celarix.JustForFun.FootballSimulator.Core.Outcomes;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Decisions
{
    internal static class TouchdownDecision
    {
        public static GameState Run(GameState priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            var possessingTeamDisposition = parameters.GetDispositionForTeam(priorState.TeamWithPossession);
            if (possessingTeamDisposition == TeamDisposition.UltraConservative)
            {
                return AttemptExtraPoint(priorState, parameters, physicsParams);
            }
            else if (possessingTeamDisposition is TeamDisposition.Insane or TeamDisposition.UltraInsane)
            {
                return AttemptTwoPointConversion(priorState, parameters, physicsParams);
            }

            var automaticTwoPointAttemptChance = physicsParams["AutomaticTwoPointAttemptChance"].Value;
            if (parameters.Random.Chance(automaticTwoPointAttemptChance))
            {
                return AttemptTwoPointConversion(priorState, parameters, physicsParams);
            }

            var minutesLeftInGame = priorState.SecondsLeftInPeriod / 60d;
            var scoreDifference = priorState.GetScoreDifferenceForTeam(priorState.TeamWithPossession);
            var attemptThreshold = physicsParams["TwoPointAttemptPointsPerMinuteThreshold"].Value;
            if (minutesLeftInGame < 0 && Math.Abs(scoreDifference) / minutesLeftInGame > attemptThreshold)
            {
                return AttemptTwoPointConversion(priorState, parameters, physicsParams);

            }
            return AttemptExtraPoint(priorState, parameters, physicsParams);
        }

        private static GameState AttemptTwoPointConversion(GameState priorState, GameDecisionParameters parameters, IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            Log.Verbose("TouchdownDecision: Team disposition is Insane or UltraInsane; opting for a two-point conversion attempt.");
            return TwoPointConversionAttemptOutcome.Run(priorState with
            {
                LineOfScrimmage = priorState.TeamYardToInternalYard(priorState.TeamWithPossession, 2),
                NextPlay = NextPlayKind.ConversionAttempt,
                LastPlayDescriptionTemplate = "{OffAbbr} attempts a two-point conversion."
            }, parameters, physicsParams);
        }

        private static GameState AttemptExtraPoint(GameState priorState, GameDecisionParameters parameters, IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            Log.Verbose("TouchdownDecision: Team disposition is UltraConservative; opting for an extra point attempt.");
            return FieldGoalAttemptOutcome.Run(priorState with
            {
                LineOfScrimmage = priorState.TeamYardToInternalYard(priorState.TeamWithPossession.Opponent(), 15),
                NextPlay = NextPlayKind.ConversionAttempt,
                LastPlayDescriptionTemplate = "{OffAbbr} attempts an extra point."
            }, parameters, physicsParams);
        }
    }
}
