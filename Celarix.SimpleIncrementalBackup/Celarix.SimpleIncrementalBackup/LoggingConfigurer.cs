using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using NLog.Config;

namespace Celarix.SimpleIncrementalBackup
{
    internal static class LoggingConfigurer
    {
        private const string Layout = "${longdate}|${level:uppercase=true}|${message}";

        public static void ConfigureConsoleLogging()
        {
            var config = new LoggingConfiguration();
            var consoleTarget = new NLog.Targets.ConsoleTarget("console")
            {
                Layout = Layout
            };
            config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, consoleTarget);
            LogManager.Configuration = config;
        }
    }
}
