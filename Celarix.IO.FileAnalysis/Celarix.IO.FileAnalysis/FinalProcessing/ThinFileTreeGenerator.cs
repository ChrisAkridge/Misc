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
    public static class ThinFileTreeGenerator
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void GenerateThinFileTree(string fileTreeInputPath, string thinTreeOutputPath)
        {
            LoggingConfigurer.ConfigurePostProcessingLogging();
            logger.Info($"Generating thin version of file tree at {fileTreeInputPath}, saving to {thinTreeOutputPath}");

            using var inputStream = new StreamReader(LongFile.OpenRead(fileTreeInputPath));
            using var outputStream = LongFile.CreateText(thinTreeOutputPath);
            var linesWritten = 0;

            while (inputStream.ReadLine() is { } currentLine)
            {
                var spaceCount = 0;

                foreach (var c in currentLine)
                {
                    if (c == ' ') { spaceCount += 1; }
                    else { break; }
                }

                var tabCount = spaceCount / 2;
                outputStream.WriteLine($"{new string(' ', tabCount)}*");

                linesWritten += 1;

                if (linesWritten % 10000 == 0)
                {
                    logger.Info($"Wrote {linesWritten} lines");
                }
            }
        }
    }
}
