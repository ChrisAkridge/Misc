using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Celarix.Imaging.BinaryDrawing;
using Celarix.Imaging.ZoomableCanvas;
using Celarix.IO.FileAnalysis.Extensions;
using NLog;
using SixLabors.ImageSharp;
using LongFileInfo = Pri.LongPath.FileInfo;
using LongPath = Pri.LongPath.Path;
using LongFile = Pri.LongPath.File;
using LongDirectoryInfo = Pri.LongPath.DirectoryInfo;

namespace Celarix.IO.FileAnalysis.Analysis
{
    internal static class BinaryDrawer
    {
        private const int MakeZoomableCanvasIfWiderThan = 8192;
        private const string SingleImageFileName = "bytes.png";
        private const string ZoomableCanvasImagePath = "bytes\\";

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void CreateBinaryImage(string filePath)
        {
            logger.Trace($"Writing binary images for {filePath}");
            
            var imageWidth = GetImageWidthForFile(filePath);

            if (imageWidth <= MakeZoomableCanvasIfWiderThan)
            {
                logger.Trace($"{filePath} will have an image of {imageWidth} pixels wide, drawing as single image...");
                CreateSingleImage(filePath);
            }
            else
            {
                logger.Trace($"{filePath} will have an image of {imageWidth} pixels wide, drawing as zoomable canvas...");
                CreateZoomableCanvas(filePath);
            }
        }

        private static int GetImageWidthForFile(string filePath)
        {
            var fileInfo = new LongFileInfo(filePath);
            var fileSize = fileInfo.Length;
            var pixelCountAt24Bpp = (long)Math.Ceiling(fileSize / 3d);

            return (int)Math.Ceiling(Math.Sqrt(pixelCountAt24Bpp));
        }
        
        private static string GetImageFolderPath(string filePath)
        {
            var fileFolderName = LongPath.GetFileNameWithoutExtension(filePath);

            if (string.IsNullOrEmpty(fileFolderName))
            {
                // no extension - files like .reloc or .rsrc
                fileFolderName = LongPath.GetExtension(filePath).TrimStart('.');
            }

            return LongPath.Combine(LongPath.GetDirectoryName(filePath),
                fileFolderName + SharedConstants.ExtractedFileFolderSuffix);
        }

        private static void CreateSingleImage(string filePath)
        {
            var imageFilePath = LongPath.Combine(GetImageFolderPath(filePath), SingleImageFileName);
            
            try
            {
                new LongDirectoryInfo(LongPath.GetDirectoryName(imageFilePath)).Create();
                using var fileStream = LongFile.OpenRead(filePath);
                using var image = Drawer.Draw(fileStream, 24, null, CancellationToken.None, new DrawingProgressToLogger());
                image.SaveAsPng(imageFilePath);
            }
            catch (Exception ex)
            {
                logger.LogException(ex);
            }
        }

        private static void CreateZoomableCanvas(string filePath)
        {
            var outputPath = LongPath.Combine(GetImageFolderPath(filePath), ZoomableCanvasImagePath);
            new LongDirectoryInfo(outputPath).Create();
            try
            {
                using var fileStream = LongFile.OpenRead(filePath);

                Drawer.DrawCanvas(fileStream, outputPath, 24, null, CancellationToken.None, new DrawingProgressToLogger());

                var currentZoomLevelFolderPath =
                    LongPath.Combine(GetImageFolderPath(filePath), ZoomableCanvasImagePath, "0");

                Utilities.Utilities.DrawZoomLevelsForLevel0CanvasTiles(currentZoomLevelFolderPath);
            }
            catch (Exception ex)
            {
                logger.LogException(ex);
            }
        }
    }
}
