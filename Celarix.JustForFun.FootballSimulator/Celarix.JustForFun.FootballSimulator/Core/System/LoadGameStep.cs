using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.System
{
    internal static class LoadGameStep
    {
        public static SystemContext Run(SystemContext context)
        {
            var repository = context.Environment.FootballRepository;
            var physicsParams = repository.GetPhysicsParams()
                .ToDictionary(p => p.Name, p => p);
            var random = context.Environment.RandomFactory.Create();
            var gameRecord = repository.GetNextUnplayedGame();

            if (gameRecord == null)
            {
                Log.Information("LoadGameStep: No incomplete games found to load.");
                return context.WithNextState(SystemState.PrepareForGame);
            }

            var airTemperature = Helpers.GetTemperatureForGame(gameRecord, physicsParams, random);
            var newPlayContext = Helpers.CreateInitialPlayContext(random,
                gameRecord,
                physicsParams["StartWindSpeedStdDev"].Value,
                airTemperature);
            Log.Information("LoadGameStep: Initialized base wind direction to {WindDirection} degrees (0 = toward home, 180 = toward away).", newPlayContext.BaseWindDirection);
            Log.Information("LoadGameStep: Initialized base wind speed to {WindSpeed} mph.", newPlayContext.BaseWindSpeed);

            CheckForInjuryRecoveries(repository, gameRecord);
            SetStrengthsAtKickoff(repository, gameRecord);

            var coinTossWinner = random.Chance(0.5)
                ? GameTeam.Home
                : GameTeam.Away;
            var kickoffLineOfScrimmage = coinTossWinner.Opponent() == GameTeam.Home
                ? 35
                : 65;

            newPlayContext = newPlayContext with
            {
                TeamWithPossession = coinTossWinner.Opponent(),
                LineOfScrimmage = kickoffLineOfScrimmage,
                Environment = new PlayEnvironment
                {
                    DecisionParameters = new GameDecisionParameters
                    {
                        Random = random,
                        AwayTeam = gameRecord.AwayTeam,
                        HomeTeam = gameRecord.HomeTeam,
                        GameType = gameRecord.GameType,
                        AwayTeamActualStrengths = TeamStrengthSet.FromTeamDirectly(gameRecord.AwayTeam, GameTeam.Away),
                        HomeTeamActualStrengths = TeamStrengthSet.FromTeamDirectly(gameRecord.HomeTeam, GameTeam.Home),
                        AwayTeamEstimateOfAway = Helpers.EstimateStrengthSetForTeam(gameRecord.AwayTeam, gameRecord.AwayTeam, gameRecord, random, physicsParams),
                        AwayTeamEstimateOfHome = Helpers.EstimateStrengthSetForTeam(gameRecord.HomeTeam, gameRecord.AwayTeam, gameRecord, random, physicsParams),
                        HomeTeamEstimateOfAway = Helpers.EstimateStrengthSetForTeam(gameRecord.AwayTeam, gameRecord.HomeTeam, gameRecord, random, physicsParams),
                        HomeTeamEstimateOfHome = Helpers.EstimateStrengthSetForTeam(gameRecord.HomeTeam, gameRecord.HomeTeam, gameRecord, random, physicsParams),
                    },
                    PhysicsParams = physicsParams
                }
            };

            context.Environment.CurrentGameRecord = gameRecord;
            context.Environment.CurrentGameContext = new GameContext(
                Version: 0L,
                NextState: GameState.Start,
                Environment: new GameEnvironment
                {
                    PhysicsParams = physicsParams,
                    CurrentPlayContext = newPlayContext,
                });

            Log.Information("LoadGameStep: Loaded incomplete game between {AwayTeam} and {HomeTeam} scheduled for {KickoffTime}.",
                gameRecord.AwayTeam.TeamName,
                gameRecord.HomeTeam.TeamName,
                gameRecord.KickoffTime);
            return context.WithNextState(SystemState.InGame);
        }

        internal static void CheckForInjuryRecoveries(IFootballRepository repository,
            GameRecord gameRecord)
        {
            var claimableRecoveries = repository.GetInjuryRecoveriesForGame(gameRecord.AwayTeamID,
                gameRecord.HomeTeamID,
                gameRecord.KickoffTime);

            foreach (var recovery in claimableRecoveries)
            {
                // Are we reflecting? You better believe we are.
                Log.Verbose("LoadGameStep: Applying injury recovery for TeamID {TeamID} on {Strength} (+{StrengthDelta}).",
                    recovery.TeamID,
                    recovery.Strength,
                    -recovery.StrengthDelta);
                var team = recovery.TeamID == gameRecord.HomeTeamID
                    ? gameRecord.HomeTeam
                    : gameRecord.AwayTeam;
                var property = team.GetType().GetProperty(recovery.Strength);
                if (property != null)
                {
                    var currentValue = (double)property.GetValue(team!)!;
                    property.SetValue(team, currentValue + -recovery.StrengthDelta);
                    recovery.Recovered = true;
                }
            }

            repository.SaveChanges();
        }

        internal static void SetStrengthsAtKickoff(IFootballRepository repository,
            GameRecord gameRecord)
        {
            var awayTeamStrengths = TeamStrengthSet.FromTeamDirectly(gameRecord.AwayTeam, GameTeam.Away);
            var homeTeamStrengths = TeamStrengthSet.FromTeamDirectly(gameRecord.HomeTeam, GameTeam.Home);
            var awayTeamStrengthsJson = global::System.Text.Json.JsonSerializer.Serialize(awayTeamStrengths);
            var homeTeamStrengthsJson = global::System.Text.Json.JsonSerializer.Serialize(homeTeamStrengths);
            gameRecord.AwayTeamStrengthsAtKickoffJSON = awayTeamStrengthsJson;
            gameRecord.HomeTeamStrengthsAtKickoffJSON = homeTeamStrengthsJson;
            repository.SaveChanges();
        }
    }
}
