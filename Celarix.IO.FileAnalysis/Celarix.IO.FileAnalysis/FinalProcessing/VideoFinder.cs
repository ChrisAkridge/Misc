using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using LongDirectory = Pri.LongPath.Directory;
using LongPath = Pri.LongPath.Path;

namespace Celarix.IO.FileAnalysis.FinalProcessing
{
    public static class VideoFinder
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly string[] supportedExtensions =
        {
            "3gp", "3g2", "asf", "asx", "avi",
            "avs", "divx", "flv", "f4v", "f4p",
            "f4a", "f4b", "m1v", "m2v", "m4p",
            "m4v", "mjpg", "mkv", "mov", "movie",
            "mp2", "mp4", "mpa", "mpe", "mpeg",
            "mpg", "mpv", "mv", "ogv",
            "qt", "rm", "rmvb", "rv", "ts",
            "viv", "vivo", "vob", "webm", "wmv",
            "yuv"
        };

        public static void FindAllVideosInFolder(string folderPath, string listOutputPath)
        {
            LoggingConfigurer.ConfigurePostProcessingLogging();
            
            var filesSearched = 0;
            var videoPaths = new List<string>();
            foreach (var filePath in LongDirectory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories))
            {
                filesSearched += 1;

                if (supportedExtensions.Contains(LongPath.GetExtension(filePath).TrimStart('.').ToLowerInvariant())
                    && !filePath.Contains("node_modules"))
                {
                    videoPaths.Add(filePath);
                }

                if (filesSearched % 1000 == 0)
                {
                    logger.Info($"Found {videoPaths.Count} videos in {filesSearched} searched files ({(videoPaths.Count * 100f) / filesSearched:F2}%)");
                }
            }
            
            logger.Info($"Found {videoPaths.Count} videos in {filesSearched} searched files ({(videoPaths.Count * 100f) / filesSearched:F2}%)");
            File.WriteAllLines(listOutputPath, videoPaths);
        }
    }
}
