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
        private readonly GamePlayerManager gamePlayerManager;

        private GameState currentState;
        private GameDecisionParameters currentParameters;
        private GameTeam firstPossessingTeam;

        public GameLoop(FootballContext footballContext,
            IRandom random,
            GameRecord gameRecord,
            PlayerManager playerManager)
        {
            // TODO: Log GameState before and after single step
            this.footballContext = footballContext;
            this.random = random;
            this.gameRecord = gameRecord;
            physicsParams = footballContext.GetAllPhysicsParams();
            gamePlayerManager = new GamePlayerManager(footballContext,
                gameRecord.AwayTeamID,
                gameRecord.HomeTeamID,
                random,
                playerManager,
                gameRecord.StadiumID);

            foreach (var param in physicsParams)
            {
                Log.Debug("Loaded physics parameter {ParamName} with value {ParamValue} {ParamUnit}.", param.Key, param.Value.Value, param.Value.Unit);
            }
        }

        // Main Loop
        public void MoveNext()
        {
            var initialTeamWithPossession = currentState.TeamWithPossession;
            var nextPlay = currentState.NextPlay;

            do
            {
                Log.Information(currentState.GetDescription(currentParameters));

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
                    GameplayNextState.FumbleOrInterceptionReturnOutcome => FumbleOrInterceptionReturnOutcome.Run(currentState, currentParameters, physicsParams),
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
            } while (currentState.NextState != GameplayNextState.PlayEvaluationComplete);

            Log.Verbose("Play evaluation complete, running post-play processing.");

            if (nextPlay == NextPlayKind.FourthDown)
            {
                var converted = (currentState.NextPlay is NextPlayKind.FirstDown
                    or NextPlayKind.ConversionAttempt)
                    && currentState.TeamWithPossession == initialTeamWithPossession;
                currentParameters.RecordFourthDownAttempt(initialTeamWithPossession, converted);
            }

            // Game clock adjustment
            currentState = GameClockAdjuster.Adjust(currentState, currentParameters, physicsParams, gameRecord);
            if (currentState.NextState == GameplayNextState.EndOfHalf)
            {
                if (currentState.PeriodNumber % 4 == 3)
                {
                    // At the start of the 3rd quarter and every 4 quarters after that, switch possession and set up a kickoff
                    Log.Verbose("Starting {Period}, switching possession and setting up kickoff.", currentState.PeriodNumber.ToPeriodDisplayString());
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
                    Log.Verbose("Starting {Period}, performing coin toss to determine first possession.", currentState.PeriodNumber.ToPeriodDisplayString());
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

                // TODO: build and commit QuarterBoxScore record
            }

            InjuryCheck();
            // TODO: increase strengths based on play results
            // TODO: on change of possession or score, build and save drive record
            // TODO: update outgoing status messages
            // TODO: if EndOfGame, finalize game record and exit loop
        }

        private void InjuryCheck()
        {
            var injuredPlayerCounts = gamePlayerManager.DoInjuryCheck(physicsParams);
            DoInjuryAdjustment(GameTeam.Away, injuredPlayerCounts.AwayTeamInjuredCount);
            DoInjuryAdjustment(GameTeam.Home, injuredPlayerCounts.HomeTeamInjuredCount);
            
            if (!injuredPlayerCounts.HomeTeamInjuredCount.Empty &&
                !injuredPlayerCounts.AwayTeamInjuredCount.Empty)
            {
                footballContext.SaveChanges();
            }
        }

        private void DoInjuryAdjustment(GameTeam team, GamePlayerManager.PlayerCounts injuredCounts)
        {
            // this brilliant code brought to you by: "it's 2:47am"
            // maybe I should someday map it all out like I did the game state tree
            var teamData = team == GameTeam.Home
                ? gameRecord.HomeTeam
                : gameRecord.AwayTeam;
            if (injuredCounts.Empty) { return; }


            Log.Verbose("Injury: Adjusting strengths after injuries for {Team}", teamData.TeamName);

            var propertyGetters = new List<Func<IStrengths, double>>();
            var propertySetters = new List<Action<IStrengths, double>>();

            var teamNoticesOwnInjuryChance = physicsParams["TeamNoticesOwnInjuryChance"].Value;
            var teamNoticesOpponentInjuryChance = physicsParams["TeamNoticesOpponentInjuryChance"].Value;

            if (injuredCounts.OffensiveCount > 0)
            {
                AddPropertyAccessorsForTeamStrengths(teamData, BasicPlayerPosition.Offense, propertyGetters, propertySetters);
                var injuredPortion = (double)(injuredCounts.OffensiveCount) / GamePlayerManager.PlayerCounts.MaxOffensivePlayers;
                AdjustStrengthForInjuries(teamData, propertyGetters, propertySetters, injuredPortion, injuredCounts.OffensiveCount);
                
                if (random.Chance(teamNoticesOwnInjuryChance))
                {
                    Log.Verbose("Injury: Team noticed own injury, adjusting estimate...");
                    var estimate = currentParameters.GetEstimateOfTeamByTeam(team, team);
                    AdjustStrengthForInjuries(estimate, propertyGetters, propertySetters, injuredPortion, injuredCounts.OffensiveCount);
                }

                if (random.Chance(teamNoticesOpponentInjuryChance))
                {
                    Log.Verbose("Injury: Team noticed own injury, adjusting estimate...");
                    var estimate = currentParameters.GetEstimateOfTeamByTeam(team.Opponent(), team);
                    AdjustStrengthForInjuries(estimate, propertyGetters, propertySetters, injuredPortion, injuredCounts.OffensiveCount);
                }
            }

            if (injuredCounts.DefensiveCount > 0)
            {
                propertyGetters.Clear();
                propertySetters.Clear();
                AddPropertyAccessorsForTeamStrengths(teamData, BasicPlayerPosition.Defense, propertyGetters, propertySetters);
                var injuredPortion = (double)(injuredCounts.DefensiveCount) / GamePlayerManager.PlayerCounts.MaxDefensivePlayers;
                AdjustStrengthForInjuries(teamData, propertyGetters, propertySetters, injuredPortion, injuredCounts.DefensiveCount);
                if (random.Chance(teamNoticesOwnInjuryChance))
                {
                    Log.Verbose("Injury: Team noticed own injury, adjusting estimate...");
                    var estimate = currentParameters.GetEstimateOfTeamByTeam(team, team);
                    AdjustStrengthForInjuries(estimate, propertyGetters, propertySetters, injuredPortion, injuredCounts.DefensiveCount);
                }
                if (random.Chance(teamNoticesOpponentInjuryChance))
                {
                    Log.Verbose("Injury: Team noticed own injury, adjusting estimate...");
                    var estimate = currentParameters.GetEstimateOfTeamByTeam(team.Opponent(), team);
                    AdjustStrengthForInjuries(estimate, propertyGetters, propertySetters, injuredPortion, injuredCounts.DefensiveCount);
                }
            }

            if (injuredCounts.Quarterback)
            {
                propertyGetters.Clear();
                propertySetters.Clear();
                AddPropertyAccessorsForTeamStrengths(teamData, BasicPlayerPosition.Quarterback, propertyGetters, propertySetters);
                var injuredPortion = 1.0 / 1.0;
                AdjustStrengthForInjuries(teamData, propertyGetters, propertySetters, injuredPortion, 1);
                if (random.Chance(teamNoticesOwnInjuryChance))
                {
                    var estimate = currentParameters.GetEstimateOfTeamByTeam(team, team);
                    AdjustStrengthForInjuries(estimate, propertyGetters, propertySetters, injuredPortion, 1);
                }
                if (random.Chance(teamNoticesOpponentInjuryChance))
                {
                    var estimate = currentParameters.GetEstimateOfTeamByTeam(team.Opponent(), team);
                    AdjustStrengthForInjuries(estimate, propertyGetters, propertySetters, injuredPortion, 1);
                }
            }

            if (injuredCounts.Kicker)
            {
                propertyGetters.Clear();
                propertySetters.Clear();
                AddPropertyAccessorsForTeamStrengths(teamData, BasicPlayerPosition.Kicker, propertyGetters, propertySetters);
                var injuredPortion = 1.0 / 1.0;
                AdjustStrengthForInjuries(teamData, propertyGetters, propertySetters, injuredPortion, 1);
                if (random.Chance(teamNoticesOwnInjuryChance))
                {
                    var estimate = currentParameters.GetEstimateOfTeamByTeam(team, team);
                    AdjustStrengthForInjuries(estimate, propertyGetters, propertySetters, injuredPortion, 1);
                }
                if (random.Chance(teamNoticesOpponentInjuryChance))
                {
                    var estimate = currentParameters.GetEstimateOfTeamByTeam(team.Opponent(), team);
                    AdjustStrengthForInjuries(estimate, propertyGetters, propertySetters, injuredPortion, 1);
                }
            }
        }

        private void AddPropertyAccessorsForTeamStrengths(IStrengths strengths,
            BasicPlayerPosition position,
            List<Func<IStrengths, double>> propertyGetters,
            List<Action<IStrengths, double>> propertySetters)
        {
            Log.Verbose("Injury: Preparing to adjust strengths for {Position} player(s)...", position);
            if (position == BasicPlayerPosition.Offense)
            {
                propertyGetters.Add(s => s.RunningOffenseStrength);
                propertySetters.Add((s, v) => s.RunningOffenseStrength = v);
                propertyGetters.Add(s => s.PassingOffenseStrength);
                propertySetters.Add((s, v) => s.PassingOffenseStrength = v);
                propertyGetters.Add(s => s.OffensiveLineStrength);
                propertySetters.Add((s, v) => s.OffensiveLineStrength = v);
                propertyGetters.Add(s => s.KickReturnStrength);
            }
            else if (position == BasicPlayerPosition.Defense)
            {
                propertyGetters.Add(s => s.RunningDefenseStrength);
                propertySetters.Add((s, v) => s.RunningDefenseStrength = v);
                propertyGetters.Add(s => s.PassingDefenseStrength);
                propertySetters.Add((s, v) => s.PassingDefenseStrength = v);
                propertyGetters.Add(s => s.DefensiveLineStrength);
                propertySetters.Add((s, v) => s.DefensiveLineStrength = v);
                propertyGetters.Add(s => s.KickDefenseStrength);
            }
            else if (position == BasicPlayerPosition.Quarterback)
            {
                propertyGetters.Add(s => s.PassingOffenseStrength);
                propertySetters.Add((s, v) => s.PassingOffenseStrength = v);
            }
            else if (position == BasicPlayerPosition.Kicker)
            {
                propertyGetters.Add(s => s.KickingStrength);
                propertySetters.Add((s, v) => s.KickingStrength = v);
                propertyGetters.Add(s => s.FieldGoalStrength);
                propertySetters.Add((s, v) => s.FieldGoalStrength = v);
            }
        }

        private void AdjustStrengthForInjuries(IStrengths strengths,
            List<Func<IStrengths, double>> propertyGetters,
            List<Action<IStrengths, double>> propertySetters,
            double injuredPortion,
            double injuredPlayerCount)
        {
            Log.Verbose("Injury: Adjusting {StrengthsType} for injuries", strengths.GetType().Name);
            for (int i = 0; i < propertyGetters.Count; i++)
            {
                var currentValue = propertyGetters[i](strengths);
                var newValue = currentValue;

                // Step 1: Remove all the strength contributed by injured players
                newValue -= currentValue * injuredPortion;

                // Step 2: Figure out the mean of who's left
                var meanOfRemaining = newValue / (1.0 - injuredPortion);

                // Step 3: For each new player added to replace injured players...
                var udfaStrengthMultiplierMean = physicsParams["UDFAStrengthMultiplierMean"].Value;
                var udfaStrengthMultiplierStdDev = physicsParams["UDFAStrengthMultiplierStddev"].Value;
                for (int j = 0; j < injuredPlayerCount; j++)
                {
                    var udfaMultiplier = random.SampleNormalDistribution(udfaStrengthMultiplierMean, udfaStrengthMultiplierStdDev);
                    udfaMultiplier = Math.Clamp(udfaMultiplier, 0, 1);
                    var udfaStrength = meanOfRemaining * udfaMultiplier;
                    newValue += udfaStrength;
                }

                Log.Verbose("Injury: Some strength went from {Old} to {New}", currentValue, newValue);
                propertySetters[i](strengths, newValue);
            }
        }
    }
}
