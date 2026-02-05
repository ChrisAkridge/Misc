using Celarix.JustForFun.FootballSimulator.Core.Debugging;
using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Output;
using Celarix.JustForFun.FootballSimulator.Random;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    public sealed class GameEnvironment : ITagListable
    {
        private readonly List<string> tags = [];

        public required IFootballRepository FootballRepository { get; init; }
        public required IReadOnlyDictionary<string, PhysicsParam> PhysicsParams { get; set; }
        public required IDebugContextWriter DebugContextWriter { get; init; }
        public PlayContext? CurrentPlayContext { get; set; }
        public required GameRecord CurrentGameRecord { get; init; }
        public required IRandomFactory RandomFactory { get; init; }
        public required IReadOnlyList<PlayerRosterPosition> AwayActiveRoster { get; set; }
        public required IReadOnlyList<PlayerRosterPosition> HomeActiveRoster { get; set; }
        public required IEventBus EventBus { get; init; }

        public void AddTag(string tag)
        {
            tags.Add(tag);
        }

        public IEnumerable<string> GetTagsAndReset()
        {
            var tagsToReturn = tags.ToArray();
            tags.Clear();
            return tagsToReturn;
        }

        public IEnumerable<string> GetTags() => tags.AsReadOnly();
    }
}
