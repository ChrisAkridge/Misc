using Celarix.JustForFun.FootballSimulator.Output.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Output
{
    public interface IEventBus
    {
        void Subscribe(IGameEventListener listener);
        void Publish(GameEvent gameEvent);
    }
}
