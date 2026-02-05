using Celarix.JustForFun.FootballSimulator.Output.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Output
{
    public sealed class EventBus : IEventBus
    {
        private readonly List<IGameEventListener> listeners = [];

        public void Publish(GameEvent gameEvent)
        {
            foreach (var listener in listeners)
            {
                listener.Handle(gameEvent);
            }
        }

        public void Subscribe(IGameEventListener listener)
        {
            listeners.Add(listener);
        }
    }
}
