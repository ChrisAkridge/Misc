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

            if (deleteBinaryDrawingFiles)
            {
                BinaryDrawingFileRemover.RemoveAllBinaryDrawingFiles(folderPath);
            }
            TextMapMover.MoveAllTextMaps(folderPath);
            CSharpMemberFileConcatenator.ConcatenateCSharpMemberFiles(folderPath);
            
            EmptyFolderRemover.RemoveAllEmptyFolders(folderPath);
            FolderTreePrinter.PrintFolderTreeForFolder(folderPath);
        }
    }
}
