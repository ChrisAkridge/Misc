using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Celarix.IO.FileAnalysis.PostProcessing
{
    public static class PostProcessor
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        public static void PostProcess(string folderPath, bool deleteBinaryDrawingFiles)
        {
            LoggingConfigurer.ConfigurePostProcessingLogging();
            logger.Info($"Performing post-processing on {folderPath}...");

            var filePaths = FileListGenerator.GenerateFileList(folderPath);
            
            if (deleteBinaryDrawingFiles)
            {
                BinaryDrawingFileRemover.RemoveAllBinaryDrawingFiles(filePaths);
            }
            TextMapMover.MoveAllTextMaps(folderPath, filePaths);
            CSharpMemberFileConcatenator.ConcatenateCSharpMemberFiles(folderPath, filePaths);

            EmptyFolderRemover.RemoveAllEmptyFolders(folderPath);
            FolderTreePrinter.PrintFolderTreeForFolder(folderPath);
        }

        public static void PartialPostProcess(string folderPath)
        {
            LoggingConfigurer.ConfigurePostProcessingLogging();
            logger.Info($"Perfoming post-processing on non-fully-analyzed folder {folderPath}...");

            var filePaths = FileListGenerator.GenerateFileList(folderPath);

            ImageFinder.FindAllImages(folderPath);

            TextMapMover.MoveAllTextMaps(folderPath, filePaths);
            CSharpMemberFileConcatenator.ConcatenateCSharpMemberFiles(folderPath, filePaths);

            EmptyFolderRemover.RemoveAllEmptyFolders(folderPath);
            FolderTreePrinter.PrintFolderTreeForFolder(folderPath);
        }
    }
}
