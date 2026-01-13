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
            var footballContext = context.Environment.FootballContext;
            var physicsParams = footballContext.PhysicsParams
                .ToDictionary(p => p.Name, p => p);
            var random = context.Environment.RandomFactory.Create();
            var gameRecord = footballContext.GameRecords
                .Include(g => g.Stadium)
                .Include(g => g.TeamDriveRecords)
                .OrderBy(g => g.KickoffTime)
                .FirstOrDefault(g => !g.GameComplete);

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

            // TODO: check for injury recoveries, apply if any
            // TODO: do coin toss to determine initial possession
        }
    }
}
