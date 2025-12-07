using Celarix.JustForFun.FootballSimulator.Core.Decisions;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Outcomes
{
    internal static class ReturnablePuntOutcome
    {
        public static GameState Run(GameState priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            var receivingTeamStrengths = parameters.GetActualStrengthsForTeam(
                priorState.TeamWithPossession.Opponent());
            var kickReturnStrength = receivingTeamStrengths.KickReturnStrength;

            // Compute recovery chance
            var alpha = Math.Log10(kickReturnStrength);
            var beta = -alpha;
            var gamma = Math.Pow(10, beta);
            var nonRecoveryChance = 1 - gamma;
            var recoveryChance = 1 - nonRecoveryChance;
            var receivingTeamRecovers = parameters.Random.Chance(recoveryChance);

            if (receivingTeamRecovers)
            {
                return SignalFairCatchDecision.Run(priorState, parameters, physicsParams);
            }

            var receivingTeamTouchesBallButNotRecoversChance = physicsParams["UnrecoveredPuntTouchesReceivingTeamChance"].Value;
            var receivingTeamTouchesBallButNotRecovers = parameters.Random.Chance(receivingTeamTouchesBallButNotRecoversChance);
            if (receivingTeamTouchesBallButNotRecovers)
            {
                return FumbledLiveBallOutcome.Run(priorState with
                    {
                        PossessionOnPlay = PossessionOnPlay.BothTeams
                    },
                    parameters,
                    physicsParams,
                    priorState.LineOfScrimmage);
            }

            // Ball is rolling, calculate where it ends up
            var puntRollMean = physicsParams["GroundPuntRollDistanceMean"].Value;
            var puntRollStdDev = physicsParams["GroundPuntRollDistanceStddev"].Value;
            var puntRollsBackwardChance = physicsParams["GroundPuntBouncesBackwardChance"].Value;
            var puntRollDistance = parameters.Random.SampleNormalDistribution(puntRollMean, puntRollStdDev);
            var rollsBackward = parameters.Random.Chance(puntRollsBackwardChance);
            if (rollsBackward) { puntRollDistance *= -1; }

            var newLineOfScrimmage = priorState.LineOfScrimmage + puntRollDistance;
            var lineOfScrimmageTeamYard = priorState.InternalYardToTeamYard(newLineOfScrimmage.Round());
            if (lineOfScrimmageTeamYard.TeamYard < 0)
            {
                if (lineOfScrimmageTeamYard.Team == priorState.TeamWithPossession)
                {
                    // Safety! Oh, no!
                    var updatedState = priorState.WithScoreChange(lineOfScrimmageTeamYard.Team.Opponent(), 2) with
                    {
                        LineOfScrimmage = priorState.TeamYardToInternalYard(priorState.TeamWithPossession, 20),
                        NextPlay = NextPlayKind.FreeKick,
                        PossessionOnPlay = priorState.TeamWithPossession.ToPossessionOnPlay()
                    };
                    return FreeKickDecision.Run(updatedState,
                        parameters,
                        physicsParams);
                }
                // Touchback on punt
                return priorState.WithFirstDownLineOfScrimmage(25d, lineOfScrimmageTeamYard.Team.Opponent());
            }

            return priorState.WithFirstDownLineOfScrimmage(newLineOfScrimmage, lineOfScrimmageTeamYard.Team.Opponent());
        }
    }
}
