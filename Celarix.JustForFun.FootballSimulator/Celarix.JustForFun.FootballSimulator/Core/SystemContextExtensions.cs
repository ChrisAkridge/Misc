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

            public void AddTag(string tag)
            {
                context.Environment!.AddTag(tag);
            }

            public IEnumerable<string> CollectTags()
            {
                var gameContext = context.Environment.CurrentGameContext!;
                var playContext = gameContext.Environment.CurrentPlayContext!;

                var playTags = playContext.Environment!.GetTagsAndReset();
                var gameTags = gameContext.Environment!.GetTagsAndReset();
                var systemTags = context.Environment!.GetTagsAndReset();

                return playTags
                    .Concat(gameTags)
                    .Concat(systemTags)
                    .Distinct();
            }

            public IEnumerable<string> GetTagsWithoutReset()
            {
                var gameContext = context.Environment.CurrentGameContext!;
                var playContext = gameContext.Environment.CurrentPlayContext!;
                var playTags = playContext.Environment!.GetTags();
                var gameTags = gameContext.Environment!.GetTags();
                var systemTags = context.Environment!.GetTags();
                return playTags
                    .Concat(gameTags)
                    .Concat(systemTags)
                    .Distinct();
            }
        }
    }
}
