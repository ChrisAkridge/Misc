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
using Celarix.IO.FileAnalysis.Utilities;
using NLog;
using SixLabors.ImageSharp;
using LongDirectory = Pri.LongPath.Directory;
using LongPath = Pri.LongPath.Path;
using LongFile = Pri.LongPath.File;
using LongFileInfo = Pri.LongPath.FileInfo;

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
            List<FilePathWithSize> filePaths;

            if (!File.Exists(filePathFilePath))
            {
                filePaths = GetAllFilesAndSizesInFolder(folderPath);
                filePaths.Sort((a, b) => string.Compare(a.FilePath, b.FilePath, StringComparison.Ordinal));
                WriteFilePathsAndSizes(filePathFilePath, filePaths);
            }
            else
            {
                filePaths = ReadFilePathsAndSizes(filePathFilePath);
            }

            logger.Info($"Drawing binary frames for {filePaths.Count:N0} files...");
            
            var multiStream = new NamedMultiStream(filePaths);
            var totalMegabytes = multiStream.Length / 1048576d;
            logger.Info($"Total size of all files is {totalMegabytes:N2} megabytes.");

            const int sourceTextHeight = 36;
            const int drawingAreaHeight = 1080 - sourceTextHeight;
            const int drawingAreaWidth = 1920;
            const int bytesPerImage = drawingAreaWidth * drawingAreaHeight * 3;
            var totalImages = (int)Math.Ceiling(multiStream.Length / (decimal)bytesPerImage);
            var imagesProgress = new AdvancedProgress(totalImages, DateTimeOffset.Now);

            logger.Info($"There will be {totalImages:N0} images drawn.");

            var progress = new Progress<DrawingProgress>();
            progress.ProgressChanged += (_, _) => { };

            for (var i = 0; i < totalImages; i++)
            {
                var imageFilePath = LongPath.Combine(outputFolderPath, $"{i:D8}.png");

                if (LongFile.Exists(imageFilePath))
                {
                    logger.Info($"Image {i + 1} already exists. Skipping.");

                    multiStream.Seek(multiStream.Position + bytesPerImage, SeekOrigin.Begin);

                    continue;
                }
                
                var image = Drawer.DrawFixedSizeWithSourceText(new Size(1920, 1080),
                    multiStream,
                    24,
                    null,
                    CancellationToken.None,
                    progress);
                image.SaveAsPng(imageFilePath);
                imagesProgress.CurrentAmount = i + 1;
                logger.Info(
                    $"Drawn image {i + 1} of {totalImages}. Drawn {((i + 1L) * bytesPerImage) / 1048576d:N2} MB of {totalMegabytes:N2} MB. {imagesProgress.AmountPerSecond:F2} FPS. Estimated complete on {imagesProgress.EstimatedCompletionTime:yyyy-MM-dd hh:mm:ss tt}");
            }
            
            logger.Info("Completed binary drawing!");
        }

        private static List<FilePathWithSize> GetAllFilesAndSizesInFolder(string folderPath)
        {
            logger.Info($"Finding all files in {folderPath}");
            var filePaths = new List<FilePathWithSize>();
            var runningTotalSize = 0L;
            var enumerator = LongDirectory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories);

            foreach (var filePath in enumerator)
            {
                var filePathWithSize = new FilePathWithSize
                {
                    FilePath = filePath,
                    Size = new LongFileInfo(filePath).Length
                };
                runningTotalSize += filePathWithSize.Size;
                filePaths.Add(filePathWithSize);

                if (filePaths.Count % 1000 == 0)
                {
                    logger.Info(
                        $"Search found {filePaths.Count} files, totalling {runningTotalSize / 1048576d:N2} MB. Most recent file found is {filePath}.");
                }
            }
            
            logger.Info($"Search complete. {folderPath} has {filePaths.Count:N0} files.");
            return filePaths;
        }

        private static void WriteFilePathsAndSizes(string filePathsFilePath, List<FilePathWithSize> filePaths)
        {
            using var writer = new BinaryWriter(File.OpenWrite(filePathsFilePath));

            foreach (var filePath in filePaths)
            {
                var filePathUTF8Bytes = Encoding.UTF8.GetBytes(filePath.FilePath);
                writer.Write(filePathUTF8Bytes.Length);
                writer.Write(filePathUTF8Bytes);
                writer.Write(filePath.Size);
            }
        }

        private static List<FilePathWithSize> ReadFilePathsAndSizes(string filePathsFilePath)
        {
            using var reader = new BinaryReader(File.OpenRead(filePathsFilePath));
            var filePaths = new List<FilePathWithSize>();

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var filePathBytesLength = reader.ReadInt32();

                if (filePathBytesLength >= 65536)
                {
                    throw new ArgumentOutOfRangeException(
                        $"Whoa! Really long file path, length {filePathBytesLength:N0}!. File position 0x{reader.BaseStream.Position - 4:X8}.");
                }
                
                var filePathBytes = reader.ReadBytes(filePathBytesLength);
                var filePath = Encoding.UTF8.GetString(filePathBytes);
                var fileSize = reader.ReadInt64();
            
                filePaths.Add(new FilePathWithSize
                {
                    FilePath = filePath,
                    Size = fileSize
                });
            }
            
            return filePaths;
        }
    }
}
