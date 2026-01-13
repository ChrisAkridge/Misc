using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.System
{
    internal static class StartStep
    {
        public static SystemContext Run(SystemContext context)
        {
            var footballContext = context.Environment.FootballContext;
            footballContext.Database.EnsureCreated();
            var settings = footballContext.SimulatorSettings.SingleOrDefault();

            if (settings?.SeedDataInitialized != true)
            {
                Log.Information("StartStep: Database not initialized. Moving to InitializeDatabase state.");
                return context.WithNextState(SystemState.InitializeDatabase);
            }

            Log.Information("StartStep: Database already initialized. Moving to PrepareForGame state.");
            return context.WithNextState(SystemState.PrepareForGame);
        }
    }
}
