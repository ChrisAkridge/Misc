using Celarix.JustForFun.FootballSimulator.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core
{
    internal static class GameContextExtensions
    {
        extension(GameContext context)
        {
            public GameContext WithNextState(GameState nextState) =>
                context with
                {
                    Version = context.Version + 1,
                    NextState = nextState
                };

            public void AddTag(string tag)
            {
                context.Environment!.AddTag(tag);
            }
        }
    }
}
