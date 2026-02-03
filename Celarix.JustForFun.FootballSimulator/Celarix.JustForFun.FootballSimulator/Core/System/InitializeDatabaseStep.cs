using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.System
{
    internal static class InitializeDatabaseStep
    {
        public static SystemContext Run(SystemContext context)
        {
            var repository = context.Environment.FootballRepository;
            var settings = repository.GetSimulatorSettings();

            if (settings == null)
            {
                repository.AddSimulatorSettings(new SimulatorSettings
                {
                    SeedDataInitialized = true
                });
                repository.SaveChanges();
            }
            else
            {
                settings.SeedDataInitialized = true;
            }

            var teamsWithStadiums = SeedData.TeamSeedData();
            Log.Verbose($"InitializeDatabaseStep: Seeding team data for {teamsWithStadiums.Count} teams into database.");
            foreach (var team in teamsWithStadiums)
            {
                repository.AddTeam(team);
            }
            repository.SaveChanges();

            Log.Verbose("InitializeDatabaseStep: Generating player rosters.");
            var playerFactory = context.Environment.PlayerFactory;
            var standardRosterPositions = PlayerFactory.GetStandardRoster();
            var random = context.Environment.RandomFactory.Create();
            foreach (var team in teamsWithStadiums)
            {
                foreach (var rosterPosition in standardRosterPositions)
                {
                    var player = playerFactory.CreateNewPlayer(random, undraftedFreeAgent: false);
                    PlayerFactory.AssignPlayerToTeam(player, team.TeamID, rosterPosition, random);
                    repository.AddPlayer(player);
                }
            }
            repository.SaveChanges();

            Log.Verbose("InitializeDatabaseStep: Adding physics parameters.");
            var physicsParams = SeedData.ParamSeedData();
            foreach (var param in physicsParams)
            {
                repository.AddPhysicsParam(param);
            }
            repository.SaveChanges();

            Log.Information("InitializeDatabaseStep: Initialized database.");
            return context.WithNextState(SystemState.InitializeNextSeason);
        }
    }
}
