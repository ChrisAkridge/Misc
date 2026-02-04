using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Functions
{
    internal static class ClockDispositionFunction
    {
        /// <summary>
        /// Determines the appropriate clock management disposition (for example, hurry-up
        /// or clock-chewing) for the team in possession based on the current game state,
        /// team dispositions and tuning parameters.
        /// </summary>
        /// <param name="priorState">The game state used to evaluate score, possession and time.</param>
        /// <param name="parameters">Decision parameters that include team dispositions and team strength estimates.</param>
        /// <param name="physicsParams">
        /// Dictionary of physics/tuning parameters. Expected keys used by this method:
        /// "LeadingClockDispositionInStandardZoneOpponentStrengthMultiple",
        /// "LeadingClockDispositionInEndOfHalfZoneOpponentStrengthMultiple",
        /// "LeadingClockDispositionInEndOfHalfZoneOpponentStrengthMultipleForAggressivePlay".
        /// </param>
        /// <returns>The chosen <see cref="ClockDisposition"/> for the possessing team.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when an unexpected <see cref="ClockZone"/> value is encountered.
        /// </exception>
        public static ClockDisposition Get(PlayContext priorState)
        {
            var parameters = priorState.Environment!.DecisionParameters;
            var physicsParams = priorState.Environment.PhysicsParams;

            GameTeam self = priorState.TeamWithPossession;
            GameTeam opponent = self.Opponent();
            var clockZone = GetClockZone(priorState, physicsParams);

            var possessingTeamDisposition = parameters.GetDispositionForTeam(self);
            if (possessingTeamDisposition is TeamDisposition.UltraInsane)
            {
                return ClockDisposition.TwoMinuteDrill;
            }

            ClockDisposition selectedClockDisposition;
            int selfScore = priorState.GetScoreForTeam(self);
            int opponentScore = priorState.GetScoreForTeam(opponent);
            if (selfScore >= opponentScore)
            {
                var selfEstimateOfSelf = parameters.GetEstimateOfTeamByTeam(self, self);
                var selfEstimateOfOpponent = parameters.GetEstimateOfTeamByTeam(self, opponent);
                var averageRatio = selfEstimateOfSelf.OverallAverageStrength / selfEstimateOfOpponent.OverallAverageStrength;
                var highThreshold = physicsParams["LeadingClockDispositionInStandardZoneOpponentStrengthMultiple"].Value;
                var mediumThreshold = physicsParams["LeadingClockDispositionInEndOfHalfZoneOpponentStrengthMultiple"].Value;
                var lowThreshold = physicsParams["LeadingClockDispositionInEndOfHalfZoneOpponentStrengthMultipleForAggressivePlay"].Value;

                if (clockZone == ClockZone.Standard)
                {
                    if (averageRatio > highThreshold)
                    {
                        selectedClockDisposition = ClockDisposition.ClockChewing;
                    }
                    else
                    {
                        selectedClockDisposition = ClockDisposition.Relaxed;
                    }
                }
                else if (averageRatio > mediumThreshold)
                {
                    if (averageRatio > lowThreshold)
                    {
                        selectedClockDisposition = ClockDisposition.TwoMinuteDrill;
                    }
                    else
                    {
                        selectedClockDisposition = ClockDisposition.HurryUp;
                    }
                }
                else
                {
                    selectedClockDisposition = ClockDisposition.Relaxed;
                }
            }
            else
            {
                selectedClockDisposition = clockZone switch
                {
                    ClockZone.Standard => ClockDisposition.HurryUp,
                    ClockZone.EndOfHalf => ClockDisposition.TwoMinuteDrill,
                    _ => throw new InvalidOperationException($"Unexpected clock zone {clockZone}.")
                };
            }

            if (possessingTeamDisposition == TeamDisposition.Insane &&
                    selectedClockDisposition is ClockDisposition.Relaxed or ClockDisposition.ClockChewing)
            {
                return ClockDisposition.HurryUp;
            }
            return selectedClockDisposition;
        }

        /// <summary>
        /// Classifies the current game time into a <see cref="ClockZone"/>. Uses the current
        /// period and the configured low-time threshold to determine if the game is in the
        /// standard time zone or the end-of-half time zone.
        /// </summary>
        /// <param name="priorState">The current game state used to obtain period and remaining time.</param>
        /// <param name="physicsParams">
        /// Dictionary of physics/tuning parameters. Expected key used by this method:
        /// "LowTimeInHalfThreshold".
        /// </param>
        /// <returns>The <see cref="ClockZone"/> representing whether the game is in standard play or end-of-half time.</returns>
        private static ClockZone GetClockZone(PlayContext priorState,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            if (priorState.PeriodNumber is 1 or 3)
            {
                return ClockZone.Standard;
            }

            var threshold = physicsParams["LowTimeInHalfThreshold"].Value.Round();
            var secondsLeftInGame = priorState.TotalSecondsLeftInGame();
            if (secondsLeftInGame <= threshold)
            {
                return ClockZone.EndOfHalf;
            }
            return ClockZone.Standard;
        }
    }
}
