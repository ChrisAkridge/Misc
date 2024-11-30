using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Celarix.IO.FileAnalysis.FinalProcessing
{
    public static class TextFileConcatenator
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void ConcatenateTextFilesInFolder(string folderPath, string outputFilePath)
        {
            LoggingConfigurer.ConfigurePostProcessingLogging();
            logger.Info($"Concatenating files in {folderPath}, outputting to {outputFilePath}...");

            using var outputWriter = new StreamWriter(outputFilePath);
            int linesWritten = 0;

            foreach (var filePath in Directory.EnumerateFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly).OrderBy(f => f))
            {
                logger.Info($"Processing {filePath}...");
                using var inputReader = new StreamReader(filePath);

                while (inputReader.ReadLine() is { } line)
                {
                    outputWriter.WriteLine(line);
                    linesWritten += 1;

                    if (linesWritten % 100_000 == 0)
                    {
                        logger.Info($"Wrote {linesWritten} lines...");
                    }
                }
                
                // Output a blank line between files.
                outputWriter.WriteLine();
                linesWritten += 1;
            }
        }
    }
}
