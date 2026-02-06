using Celarix.JustForFun.FootballSimulator.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    internal sealed class DecisionArguments(PlayContext priorState,
        GameDecisionParameters parameters,
        IReadOnlyDictionary<string, PhysicsParam> physicsParams)
    {
        public PlayContext PriorState { get; } = priorState;
        public GameDecisionParameters Parameters { get; } = parameters;
        public IReadOnlyDictionary<string, PhysicsParam> PhysicsParams { get; } = physicsParams;

        public void Deconstruct(out PlayContext priorState,
            out GameDecisionParameters parameters,
            out IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            priorState = PriorState;
            parameters = Parameters;
            physicsParams = PhysicsParams;
        }
    }
}
