using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.IO.FileAnalysis.Utilities;
using NLog;
using LongPath = Pri.LongPath.Path;
using LongFile = Pri.LongPath.File;
using LongDirectory = Pri.LongPath.Directory;

namespace Celarix.IO.FileAnalysis.FinalProcessing
{
    public static class NumberedTextMapMover
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void MoveNumberedTextMaps(string textMapFolder)
        {
            LoggingConfigurer.ConfigurePostProcessingLogging();
            logger.Info("Moving all numbered text maps into GUID-named folders...");

            var guidBuffer = new byte[16];
            var textMaps = LongDirectory
                .GetFiles(textMapFolder, "*.png", SearchOption.TopDirectoryOnly)
                .OrderBy(m => m)
                .ToList();
            logger.Info($"Found {textMaps.Count} numbered text maps");
            var advancedProgress = new AdvancedProgress(textMaps.Count, DateTimeOffset.Now);

            var textMapCount = textMaps.Count;
            for (var i = 0; i < textMapCount; i++)
            {
                var textMapPath = textMaps[i];
                var fileGuid = Guid.NewGuid();
                var destinationFileName = $"{fileGuid}.png";

                fileGuid.TryWriteBytes(guidBuffer);

                var destinationFolderPath = LongPath.Combine(guidBuffer[0].ToString(), guidBuffer[1].ToString());
                var destinationPath = LongPath.Combine(textMapFolder, destinationFolderPath);
                LongDirectory.CreateDirectory(destinationPath);
                var destinationFilePath = LongPath.Combine(destinationPath, destinationFileName);
                
                LongFile.Move(textMapPath, destinationFilePath);
                advancedProgress.CurrentAmount += 1;
                logger.Info(advancedProgress.ToString());
            }
            
            logger.Info("Moved all numbered text maps.");
        }
    }
}
