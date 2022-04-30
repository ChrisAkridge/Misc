﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Celarix.Imaging;
using Celarix.IO.FileAnalysis.Analysis;
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
            
            var inputFolderPath = args[0];
            var outputFolderPath = args[1];

            var job = AnalysisJob.CreateOrLoad(inputFolderPath, outputFolderPath);
            job.StartOrResume();
        }
    }
}