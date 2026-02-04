using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Debugging
{
    public interface IDebugContextWriter
    {
        void EnterGame(int gameID);
        void ExitGame();
        void WriteContext<T>(T context);
    }
}
