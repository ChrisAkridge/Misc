using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Scheduling;
using MathNet.Numerics;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core
{
    public sealed partial class SystemLoop
    {
        private void Initialize()
        {
            footballContext.Database.EnsureCreated();
            var settings = footballContext.SimulatorSettings.SingleOrDefault();

            if (settings?.SeedDataInitialized != true)
            {
                FirstTimeInitialization(settings);
            }
            else
            {
                FindCurrentState();
            }

            CurrentState = CurrentSystemState.GameInitialization;
        }

        private void FirstTimeInitialization(SimulatorSettings? settings)
        {
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
            foreach (var team in teamsWithStadiums) { footballContext.Teams.Add(team); }
            footballContext.SaveChanges();
            Log.Information("Seeded team data into the database.");

            const int BaseYear = 2014;
            GenerateScheduleForYear(BaseYear);
            footballContext.SaveChanges();
            Log.Information("Generated initial schedule for year {Year}.", BaseYear);

            SystemStatus = new SystemStatus
            {
                CurrentState = CurrentSystemState.Initialization,
                StatusMessage = "Initialization complete. Seed data added."
            };
        }

        private void GenerateScheduleForYear(int year)
        {
            var teams = footballContext.Teams.ToList();
            var dataTeams = teams.ToDictionary(t => new BasicTeamInfo(t.TeamName, t.Conference, t.Division), t => t);
            var scheduleGenerator = new ScheduleGenerator3([.. dataTeams.Keys], randomFactory);
            var schedule = scheduleGenerator.GenerateScheduleForYear(year, dataTeams, null, null, out _);
        }

        private void FindCurrentState()
        {
            Log.Information("Seed data already present in the database. Skipping seeding.");

            var currentSeasonId = footballContext.GetCurrentSeasonId();
            var currentGame = footballContext.GetCurrentGameRecord(currentSeasonId);
            currentGameLoop = new GameLoop(footballContext, randomFactory.Create(), currentGame, CreatePlayerManager());
            currentGameLoop.Initialize();
        }

        private PlayerManager CreatePlayerManager()
        {
            // Get a list of first and last names from two files
            // in the build directory: "firstNames.txt" and "lastNames.txt"
            // They're newline-separated lists of names.
            var firstNames = File.ReadAllLines("firstNames.txt");
            var lastNames = File.ReadAllLines("lastNames.txt");
            return new PlayerManager(firstNames, lastNames);
        }
    }
}
