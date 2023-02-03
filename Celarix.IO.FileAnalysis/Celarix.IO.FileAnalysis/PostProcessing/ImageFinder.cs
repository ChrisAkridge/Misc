using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.IO.FileAnalysis.Analysis;
using NLog;
using LongPath = Pri.LongPath.Path;
using LongFile = Pri.LongPath.File;
using LongDirectory = Pri.LongPath.Directory;

namespace Celarix.IO.FileAnalysis.PostProcessing
{
    internal static class ImageFinder
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        public static void FindAllImages(string folderPath)
        {
            logger.Info($"Searching for all images in {folderPath}...");

            var imagePathsFilePath = LongPath.Combine(folderPath, "imagePaths.txt");
            var foundImagePaths = FindAllImagesInFolder(folderPath);
            
            LongFile.WriteAllLines(imagePathsFilePath, foundImagePaths);
            logger.Info($"Wrote paths for {foundImagePaths.Count} images.");
        }

        private static List<string> FindAllImagesInFolder(string folderPath)
        {
            var imagePaths = new List<string>();

            var filesInFolder = LongDirectory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly);
            var foldersInFolder = LongDirectory.GetDirectories(folderPath, "*", SearchOption.TopDirectoryOnly);
            
            imagePaths.AddRange(filesInFolder.Where(f => ImageIdentifier.IsValidImageFile(f)));

            foreach (var childFolderPath in foldersInFolder)
            {
                imagePaths.AddRange(FindAllImagesInFolder(childFolderPath));
            }
            
            logger.Info($"Folder {folderPath} has {imagePaths.Count} images.");

            return imagePaths;
        }
    }
}
