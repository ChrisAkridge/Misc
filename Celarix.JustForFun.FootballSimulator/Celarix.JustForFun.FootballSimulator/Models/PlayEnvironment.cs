using Celarix.JustForFun.FootballSimulator.Core.Debugging;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Output;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    public sealed class PlayEnvironment : ITagListable
    {
        private readonly List<string> eventTags = [];
        public required GameDecisionParameters DecisionParameters { get; init; }
        public required IReadOnlyDictionary<string, PhysicsParam> PhysicsParams { get; init; }
        public required IEventBus EventBus { get; init; }
        
        public void AddTag(string tag)
        {
            eventTags.Add(tag);
        }

        public IEnumerable<string> GetTagsAndReset()
        {
            var tagsToReturn = eventTags.ToArray();
            eventTags.Clear();
            return tagsToReturn;
        }

        public IEnumerable<string> GetTags() => eventTags.AsReadOnly();
    }
}
