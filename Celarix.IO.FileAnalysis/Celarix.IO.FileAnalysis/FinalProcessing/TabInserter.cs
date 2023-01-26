using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using LongFile = Pri.LongPath.File;

namespace Celarix.IO.FileAnalysis.FinalProcessing
{
    public static class TabInserter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void InsertTabsForFile(string inputFilePath, string outputFilePath, int tabsToInsert,
            string lineToPrepend)
        {
            LoggingConfigurer.ConfigurePostProcessingLogging();
            logger.Info($"Prepending {tabsToInsert} tabs for each line in {inputFilePath}, writing to {outputFilePath}...");

            using var inputStream = new StreamReader(LongFile.OpenRead(inputFilePath));
            using var outputStream = LongFile.CreateText(outputFilePath);
            var tabs = new string(' ', 4 * tabsToInsert);
            
            outputStream.WriteLine(lineToPrepend);
            var linesWritten = 1;

            while (inputStream.ReadLine() is { } lineFromInput)
            {
                outputStream.WriteLine(string.Concat(tabs, lineFromInput));
                linesWritten += 1;

                if (linesWritten % 10_000 == 0)
                {
                    logger.Info($"Wrote {linesWritten} lines...");
                }
            }
        }
    }
}
