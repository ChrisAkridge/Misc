using Celarix.JustForFun.FootballSimulator.Models;
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
            return context;
        }
    }
}
