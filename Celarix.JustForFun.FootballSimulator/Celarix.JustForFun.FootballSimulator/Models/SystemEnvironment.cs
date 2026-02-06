using Celarix.JustForFun.FootballSimulator.Core.Debugging;
using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Output;
using Celarix.JustForFun.FootballSimulator.Random;
using Celarix.JustForFun.FootballSimulator.Scheduling;
using Celarix.JustForFun.FootballSimulator.Standings;
using Celarix.JustForFun.FootballSimulator.SummaryWriting;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    public sealed class SystemEnvironment : ITagListable
    {
        private readonly List<string> tags = [];

        public required IFootballRepository FootballRepository { get; init; }
        public required IRandomFactory RandomFactory { get; init; }
        public required PlayerFactory PlayerFactory { get; init; }
        public required ISummaryWriter SummaryWriter { get; init; }
        public required IDebugContextWriter DebugContextWriter { get; init; }
        public required IEventBus EventBus { get; init; }
        public ScheduleGenerator3? ScheduleGenerator { get; set; }
        public TeamRanker? TeamRanker { get; set; }
        public GameRecord? CurrentGameRecord { get; set; }
        public GameContext? CurrentGameContext { get; set; }

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
