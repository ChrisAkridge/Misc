using Celarix.JustForFun.FootballSimulator.Core.Decisions;
using Celarix.JustForFun.FootballSimulator.Core.Outcomes;
using Celarix.JustForFun.FootballSimulator.Core.PostPlay;
using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Core
{
    internal sealed partial class GameLoop
    {
        private IRandom random;
        private FootballContext footballContext;
        private GameRecord gameRecord;
        private readonly IReadOnlyDictionary<string, PhysicsParam> physicsParams;

        private GameState currentState;
        private GameDecisionParameters currentParameters;
        private GameTeam firstPossessingTeam;

        public GameLoop(FootballContext footballContext, IRandom random, GameRecord gameRecord)
        {
            // TODO: Log GameState before and after single step
            this.footballContext = footballContext;
            this.random = random;
            this.gameRecord = gameRecord;
            physicsParams = footballContext.GetAllPhysicsParams();

            foreach (var param in physicsParams)
            {
                Log.Debug("Loaded physics parameter {ParamName} with value {ParamValue} {ParamUnit}.", param.Key, param.Value.Value, param.Value.Unit);
            }
        }

        // Main Loop
        public void MoveNext()
        {
            do
            {
                currentState = currentState.NextState switch
                {
                    GameplayNextState.Start => throw new InvalidOperationException("uh... shouldn't be here yet, I think?"),
                    GameplayNextState.KickoffDecision => KickoffDecision.Run(currentState, currentParameters, physicsParams),
                    GameplayNextState.FreeKickDecision => FreeKickDecision.Run(currentState, currentParameters, physicsParams),
                    GameplayNextState.SignalFairCatchDecision => SignalFairCatchDecision.Run(currentState, currentParameters, physicsParams),
                    GameplayNextState.TouchdownDecision => TouchdownDecision.Run(currentState, currentParameters, physicsParams),
                    GameplayNextState.MainGameDecision => MainGameDecision.Run(currentState, currentParameters, physicsParams),
                    GameplayNextState.ReturnFumbledOrInterceptedBallDecision => ReturnFumbledOrInterceptedBallDecision.Run(currentState, currentParameters, physicsParams),
                    GameplayNextState.NormalKickoffOutcome => NormalKickoffOutcome.Run(currentState, currentParameters, physicsParams),
                    GameplayNextState.ReturnableKickOutcome => ReturnableKickOutcome.Run(currentState, currentParameters, physicsParams),
                    GameplayNextState.FumbledLiveBallOutcome => FumbledLiveBallOutcome.Run(currentState, currentParameters, physicsParams),
                    GameplayNextState.PuntOutcome => PuntOutcome.Run(currentState, currentParameters, physicsParams),
                    GameplayNextState.ReturnablePuntOutcome => ReturnablePuntOutcome.Run(currentState, currentParameters, physicsParams),
                    GameplayNextState.KickOrPuntReturnOutcome => KickOrPuntReturnOutcome.Run(currentState, currentParameters, physicsParams),
                    GameplayNextState.OnsideKickAttemptOutcome => OnsideKickAttemptOutcome.Run(currentState, currentParameters, physicsParams),
                    GameplayNextState.FieldGoalsAndExtraPointAttemptOutcome => FieldGoalAttemptOutcome.Run(currentState, currentParameters, physicsParams),
                    GameplayNextState.TwoPointConversionAttemptOutcome => TwoPointConversionAttemptOutcome.Run(currentState, currentParameters, physicsParams),
                    GameplayNextState.StandardRushingPlayOutcome => StandardRushingPlayOutcome.Run(currentState, currentParameters, physicsParams),
                    GameplayNextState.StandardShortPassingPlayOutcome => StandardPassingPlayOutcome.Run(currentState, currentParameters, physicsParams, PassAttemptDistance.Short),
                    GameplayNextState.StandardMediumPassingPlayOutcome => StandardPassingPlayOutcome.Run(currentState, currentParameters, physicsParams, PassAttemptDistance.Medium),
                    GameplayNextState.StandardLongPassingPlayOutcome => StandardPassingPlayOutcome.Run(currentState, currentParameters, physicsParams, PassAttemptDistance.Long),
                    GameplayNextState.HailMaryOutcome => HailMaryOutcome.Run(currentState, currentParameters, physicsParams),
                    GameplayNextState.QBSneakOutcome => QBSneakOutcome.Run(currentState, currentParameters, physicsParams),
                    GameplayNextState.FakePuntOutcome => FakePuntOrFieldGoalOutcome.Run(currentState, currentParameters, physicsParams),
                    GameplayNextState.FakeFieldGoalOutcome => FakePuntOrFieldGoalOutcome.Run(currentState, currentParameters, physicsParams),
                    GameplayNextState.VictoryFormationOutcome => VictoryFormationOutcome.Run(currentState, currentParameters, physicsParams),
                    _ => throw new InvalidOperationException($"Unhandled gameplay next state {currentState.NextState}.")
                };

                currentState = currentState with
                {
                    Version = currentState.Version + 1
                };

                Log.Verbose("Advanced to next game state: {GameState}.", currentState.NextState);
            } while (currentState.NextState != GameplayNextState.PlayEvaluationComplete);

            Log.Verbose("Play evaluation complete, running post-play processing.");

            // TODO: compute update to game clock
            currentState = GameClockAdjuster.Adjust(currentState, currentParameters, physicsParams);
            if (currentState.NextState == GameplayNextState.EndOfHalf)
            {
                if (currentState.PeriodNumber % 4 == 3)
                {
                    // At the start of the 3rd quarter and every 4 quarters after that, switch possession and set up a kickoff
                    currentState = currentState with
                    {
                        // The first possessing team gets to kick off to the other team for the 2nd half
                        TeamWithPossession = firstPossessingTeam,
                        NextPlay = NextPlayKind.Kickoff,
                        ClockRunning = false,
                        LineOfScrimmage = currentState.TeamYardToInternalYard(firstPossessingTeam, 35),
                        LastPlayDescriptionTemplate = "Start of {Period}. {OffAbbr} kickoff to {DefAbbr}.",
                        PossessionOnPlay = firstPossessingTeam.ToPossessionOnPlay()
                    };
                }
                else if (currentState.PeriodNumber % 4 == 1)
                {
                    // At the start of the 1st quarter and every 4 quarters after that, do another coinflip.
                    firstPossessingTeam = random.Chance(0.5)
                        ? GameTeam.Away
                        : GameTeam.Home;
                    currentState = currentState with
                    {
                        // The first possessing team gets to kick off to the other team for the 2nd half
                        TeamWithPossession = firstPossessingTeam.Opponent(),
                        NextPlay = NextPlayKind.Kickoff,
                        ClockRunning = false,
                        LineOfScrimmage = currentState.TeamYardToInternalYard(firstPossessingTeam.Opponent(), 35),
                        LastPlayDescriptionTemplate = "Start of {Period}, {DefAbbr} wins coin toss and takes possession. {OffAbbr} kickoff to {DefAbbr}.",
                        PossessionOnPlay = firstPossessingTeam.ToPossessionOnPlay()
                    };
                }
            }

            // TODO: compute injury chances and UDFA drafting
            // TODO: increase strengths based on play results
            // TODO: on change of possession or score, build and save drive record
            // TODO: update outgoing status messages
        }
    }
}
