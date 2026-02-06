using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Output
{
    public sealed class CallbackSink(Action callback) : ILogEventSink
    {
        private readonly Action callback = callback;

        public void Emit(LogEvent logEvent)
        {
            callback();
        }
    }
}
