using Celarix.JustForFun.FootballSimulator.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Output.Models
{
    public sealed class GameEvent
    {
        private string[] tags = [];

        public required string EventType { get; init; }
        public required SystemContext SystemContext { get; init; }
        public GameContext? GameContext => SystemContext.Environment?.CurrentGameContext;
        public PlayContext? PlayContext => GameContext?.Environment?.CurrentPlayContext;
        public IReadOnlyList<string> Tags => tags;

        public GameEvent WithTags(params IEnumerable<string> tags)
        {
            this.tags = [ ..this.tags, ..tags];
            return this;
        }
    }

    public static class GameEventTypes
    {
        // Const strings go here
        public const string SystemStateMachineStep = nameof(SystemStateMachineStep);
    }
}
