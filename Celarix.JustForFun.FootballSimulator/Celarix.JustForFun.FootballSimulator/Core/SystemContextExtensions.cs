using Celarix.JustForFun.FootballSimulator.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core
{
    internal static class SystemContextExtensions
    {
        extension(SystemContext context)
        {
            public SystemContext WithNextState(SystemState nextState) =>
                context with
                {
                    Version = context.Version + 1,
                    NextState = nextState
                };
        }
    }
}
