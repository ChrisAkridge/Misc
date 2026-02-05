using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.System
{
    internal static class ErrorStep
    {
        public static SystemContext Run(SystemContext context)
        {
            // Don't do anything, just return the context as-is
            Log.Information("ErrorStep: System state machine in error, cannot proceed.");
            context.AddTag("error");
            return context;
        }
    }
}
