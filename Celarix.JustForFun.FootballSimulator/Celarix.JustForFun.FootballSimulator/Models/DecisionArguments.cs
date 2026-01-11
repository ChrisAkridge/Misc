using Celarix.JustForFun.FootballSimulator.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    internal sealed class DecisionArguments
    {
        public GameState PriorState { get; }
        public GameDecisionParameters Parameters { get; }
        public IReadOnlyDictionary<string, PhysicsParam> PhysicsParams { get; }
        
        public DecisionArguments(GameState priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            PriorState = priorState;
            Parameters = parameters;
            PhysicsParams = physicsParams;
        }

        public void Deconstruct(out GameState priorState,
            out GameDecisionParameters parameters,
            out IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            priorState = PriorState;
            parameters = Parameters;
            physicsParams = PhysicsParams;
        }
    }
}
