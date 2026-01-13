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
            var footballContext = context.Environment.FootballContext;
            var settings = footballContext.SimulatorSettings.SingleOrDefault();

            if (settings == null)
            {
                footballContext.SimulatorSettings.Add(new SimulatorSettings
                {
                    SeedDataInitialized = true
                });
                footballContext.SaveChanges();
            }
            else
            {
                settings.SeedDataInitialized = true;
            }

            var teamsWithStadiums = SeedData.TeamSeedData();
            Log.Verbose($"InitializeDatabaseStep: Seeding team data for {teamsWithStadiums.Count} teams into database.");
            foreach (var team in teamsWithStadiums)
            {
                footballContext.Teams.Add(team);
            }
            footballContext.SaveChanges();

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
                    footballContext.Players.Add(player);
                }
            }
            footballContext.SaveChanges();

            Log.Verbose("InitializeDatabaseStep: Adding physics parameters.");
            var physicsParams = SeedData.ParamSeemData();
            foreach (var param in physicsParams)
            {
                footballContext.PhysicsParams.Add(param);
            }
            footballContext.SaveChanges();

            Log.Information("InitializeDatabaseStep: Initialized database.");
            return context.WithNextState(SystemState.InitializeNextSeason);
        }
    }
}
