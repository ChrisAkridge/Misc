using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Celarix.Imaging;
using Celarix.IO.FileAnalysis.Analysis;
using Celarix.IO.FileAnalysis.PostProcessing;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Celarix.IO.FileAnalysis.Console
{
    internal sealed class Program
    {
        private static void Main(string[] args)
        {
            // TODO: add CommandLine library
            Celarix.Imaging.LibraryConfiguration.Instance = new LibraryConfiguration
            {
                BinaryDrawingReportsProgressEveryNPixels = 1048576,
                ZoomableCanvasTileEdgeLength = 1024
            };

            if (args[0].Equals("-postprocess", StringComparison.InvariantCultureIgnoreCase))
            {
                var folderPath = args[1];

                PostProcessor.PostProcess(folderPath, args.Length == 3 && args[2].Equals("-deleteBinaryDrawingFiles", StringComparison.InvariantCultureIgnoreCase));
            }
            else if (args[0].Equals("-draw", StringComparison.InvariantCultureIgnoreCase))
            {
                var folderPath = args[1];
                var outputFolderPath = args[2];
                
                BinaryFrameDrawer.DrawFramesForFolder(folderPath, outputFolderPath);
            }
            else if (args[0].Equals("-analyzelogs", StringComparison.InvariantCultureIgnoreCase))
            {
                var folderPath = args[1];
                
                LogAnalyzer.AnalyzeLogsToCSVs(folderPath);
            }
            else if (args[0].Equals("-drawtextmapcanvas", StringComparison.InvariantCultureIgnoreCase))
            {
                var filePath = args[1];
                var outputFolderPath = args[2];
                
                TextMapCanvasGenerator.GenerateTextMapCanvasForFile(filePath, outputFolderPath);
            }
            else
            {
                var inputFolderPath = args[0];
                var outputFolderPath = args[1];

                var job = AnalysisJob.CreateOrLoad(inputFolderPath, outputFolderPath);
                job.StartOrResume();
            }
        }
    }
}
