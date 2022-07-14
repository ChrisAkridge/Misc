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
        
        public static void PostProcess(string folderPath)
        {
            LoggingConfigurer.ConfigurePostProcessingLogging();
            logger.Info($"Performing post-processing on {folderPath}...");
            
            BinaryDrawingFileRemover.RemoveAllBinaryDrawingFiles(folderPath);
            TextMapMover.MoveAllTextMaps(folderPath);
            CSharpMemberFileConcatenator.ConcatenateCSharpMemberFiles(folderPath);
        }
    }
}
