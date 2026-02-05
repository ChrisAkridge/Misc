using Celarix.JustForFun.FootballSimulator.Output.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Output
{
    public interface IGameEventListener
    {
        void Handle(GameEvent gameEvent);
    }
}
