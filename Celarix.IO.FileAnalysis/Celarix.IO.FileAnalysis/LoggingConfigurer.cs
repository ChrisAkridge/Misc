using System.Collections.Generic;
using System.Text;
using NLog;
using NLog.Conditions;
using NLog.Config;
using NLog.Filters;
using NLog.Targets;
using LongPath = Pri.LongPath.Path;

namespace Celarix.IO.FileAnalysis
{
	public static class LoggingConfigurer
    {
        private const string LoggingFolderPath = "job\\log";
		private const string Layout = "${longdate}|${level:uppercase=true}|${message}";

		public static void ConfigureLogging(string jobOutputFolderPath)
        {
            var config = new LoggingConfiguration();
            var coloredConsoleTarget = new ColoredConsoleTarget
            {
                Name = "coloredConsole",
                Layout = Layout,
                RowHighlightingRules =
                {
                    new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("contains(\'${message}\', \'estimated\')"),
                        ConsoleOutputColor.DarkGreen,
                        ConsoleOutputColor.Black)
                }
			};
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, coloredConsoleTarget);
            
            var fileTarget = new FileTarget
            {
                Name = "fileTarget",
                Layout = Layout,
                FileName = LongPath.Combine(jobOutputFolderPath, LoggingFolderPath, "${date:format=yyyy-MM-dd}.log"),
                Encoding = Encoding.UTF8,
                LineEnding = LineEndingMode.LF,
                ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
                ArchiveAboveSize = 1048576L,
                ArchiveFileName = LongPath.Combine(jobOutputFolderPath, LoggingFolderPath, "${date:format=yyyy-MM-dd}_${#}.log")
            };
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, fileTarget);

            LogManager.Configuration = config;
        }
    }
}
