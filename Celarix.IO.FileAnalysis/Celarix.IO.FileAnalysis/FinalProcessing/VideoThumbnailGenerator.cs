using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using LongDirectory = Pri.LongPath.Directory;
using LongPath = Pri.LongPath.Path;
using LongFile = Pri.LongPath.File;

namespace Celarix.IO.FileAnalysis.FinalProcessing
{
    public static class VideoThumbnailGenerator
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void GenerateThumbnailsForVideos(string videoListPath, string ezThumbBinaryPath,
            string thumbnailOutputPath)
        {
            LoggingConfigurer.ConfigurePostProcessingLogging();

            LongDirectory.CreateDirectory(thumbnailOutputPath);
            var tempChildFolderPath = LongPath.Combine(thumbnailOutputPath, "temp");
            LongDirectory.CreateDirectory(tempChildFolderPath);
            
            var filePaths = LongFile.ReadAllLines(videoListPath);
            logger.Info($"Generating thumbnails using EZThumb for {filePaths.Length} videos...");

            foreach (var filePath in filePaths)
            {
                logger.Info($"Generating thumbnail for {filePath}...");
                
                if (!LongFile.Exists(filePath))
                {
                    logger.Error($"File at {filePath} does not exist.");
                    continue;
                }
                
                var processStartInfo = new ProcessStartInfo(ezThumbBinaryPath, $"\"{filePath}\" --outdir \"{tempChildFolderPath}\" --format png")
                {
                    UseShellExecute = false
                };
                
                var process = Process.Start(processStartInfo);
                if (!process.WaitForExit(TimeSpan.FromMinutes(5)))
                {
                    process.Kill();
                }
                
                var filesInTempFolder = LongDirectory.GetFiles(tempChildFolderPath, "*.png");

                if (filesInTempFolder.Length > 1)
                {
                    logger.Warn("Expected one file in temp folder, found multiple.");
                }
                else if (filesInTempFolder.Length == 0)
                {
                    logger.Error($"Failed to generate thumbnail for {filePath}.");

                    continue;
                }

                foreach (var tempFile in filesInTempFolder)
                {
                    var fileNameGuid = Guid.NewGuid() + ".png";
                    LongFile.Move(tempFile, LongPath.Combine(thumbnailOutputPath, fileNameGuid));
                }
                
                logger.Info($"Generated thumbnail for {filePath}.");
            }
        }
    }
}
