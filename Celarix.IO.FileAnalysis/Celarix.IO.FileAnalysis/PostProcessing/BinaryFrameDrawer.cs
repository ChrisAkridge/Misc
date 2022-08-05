using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Celarix.Imaging.BinaryDrawing;
using Celarix.Imaging.IO;
using Celarix.Imaging.Progress;
using NLog;
using SixLabors.ImageSharp;
using LongDirectory = Pri.LongPath.Directory;
using LongPath = Pri.LongPath.Path;
using LongFile = Pri.LongPath.File;

namespace Celarix.IO.FileAnalysis.PostProcessing
{
    public static class BinaryFrameDrawer
    {
        private const string FilePathFileName = "files.txt";
        
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void DrawFramesForFolder(string folderPath, string outputFolderPath)
        {
            LoggingConfigurer.ConfigurePostProcessingLogging();
            logger.Info($"Drawing binary frames for {folderPath}, outputting to {outputFolderPath}...");

            if (!LongDirectory.Exists(outputFolderPath))
            {
                LongDirectory.CreateDirectory(outputFolderPath);
            }
            
            var filePathFilePath = LongPath.Combine(outputFolderPath, FilePathFileName);
            var filePaths = !File.Exists(filePathFilePath)
                ? GetAllFilePathsInFolder(folderPath)
                : LongFile.ReadAllLines(filePathFilePath).ToList();
            logger.Info($"Drawing binary frames for {filePaths.Count:N0} files...");

            var multiStream = new NamedMultiStream(filePaths);
            var totalMegabytes = multiStream.Length / 1048576d;
            logger.Info($"Total size of all files is {totalMegabytes:N2} megabytes.");

            const int sourceTextHeight = 36;
            const int drawingAreaHeight = 1080 - sourceTextHeight;
            const int drawingAreaWidth = 1920;
            const int bytesPerImage = drawingAreaWidth * drawingAreaHeight * 3;
            var totalImages = (int)Math.Ceiling(multiStream.Length / (decimal)bytesPerImage);
            
            logger.Info($"There will be {totalImages:N0} images drawn.");

            var progress = new Progress<DrawingProgress>();
            progress.ProgressChanged += (_, _) => { };

            for (var i = 0; i < totalImages; i++)
            {
                var image = Drawer.DrawFixedSizeWithSourceText(new Size(1920, 1080),
                    multiStream,
                    24,
                    null,
                    CancellationToken.None,
                    progress);
                var imageFilePath = LongPath.Combine(outputFolderPath, $"{i:D8}.png");
                image.SaveAsPng(imageFilePath);
                logger.Info($"Drawn image {i + 1} of {totalImages}. Drawn {((i + 1) * bytesPerImage) / 1048576d:N2} MB of {totalMegabytes:N2} MB.");
            }
            
            logger.Info("Completed binary drawing!");
        }

        private static List<string> GetAllFilePathsInFolder(string folderPath)
        {
            logger.Info($"Finding all files in {folderPath}...");
            var filePaths = new List<string>();
            var enumerator = LongDirectory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories);

            foreach (var filePath in enumerator)
            {
                filePaths.Add(filePath);
                if (filePaths.Count % 1000 == 0)
                {
                    logger.Info($"Search found {filePaths.Count:N0} files.");
                }
            }
            
            logger.Info($"Search complete. {folderPath} has {filePaths.Count:N0} files.");
            return filePaths;
        }
    }
}
