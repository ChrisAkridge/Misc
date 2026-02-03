using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Output
{
    public sealed class CallbackSink : ILogEventSink
    {
        private readonly Action callback;

        public CallbackSink(Action callback)
        {
            this.callback = callback;
        }

        public void Emit(LogEvent logEvent)
        {
            callback();
        }
    }
}
