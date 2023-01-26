using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Celarix.Imaging;
using Celarix.IO.FileAnalysis.Analysis;
using Celarix.IO.FileAnalysis.PostProcessing;
using NLog;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using File = Pri.LongPath.File;

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
            // add TabInserter option
            else
            {
                var inputFolderPath = args[0];
                var outputFolderPath = args[1];

                var job = AnalysisJob.CreateOrLoad(inputFolderPath, outputFolderPath);
                job.StartOrResume();
            }
        }

        private static void SplitEnWikiIntoSmallerLines()
        {
            LoggingConfigurer.ConfigurePostProcessingLogging();

            var logger = LogManager.GetCurrentClassLogger();

            using var reader = new StreamReader(File.OpenRead(
                @"I:\fa\PAVILION-CORE\04 CELARIX_4TB_A\19\files\enwiki-20190101-pages-articles-multistream.xml_ext\enwiki-20190101-pages-articles-multistream.xml.tar"));
            using var writer =
                new StreamWriter(
                    @"I:\fa\PAVILION-CORE\04 CELARIX_4TB_A\19\files\enwiki-20190101-pages-articles-multistream.xml_ext\enwiki-20190101-pages-articles-multistream-splitline.xml.tar");
            var readLines = 0;
            var writtenLines = 0;
            
            while (reader.ReadLine() is { } currentLine)
            {
                var lineAsASCII = LatinToAscii(currentLine);
                readLines += 1;

                if (readLines % 10000 == 0)
                {
                    logger.Info($"Read {readLines} lines, wrote {writtenLines} lines");
                }
                
                if (lineAsASCII.Length < 120)
                {
                    writer.WriteLine(lineAsASCII);
                    writtenLines += 1;

                    continue;
                }

                var i = 0;

                while (i + 120 < lineAsASCII.Length)
                {
                    var span = lineAsASCII.AsSpan(i, 120);
                    writer.WriteLine(span);

                    i += 120;
                }

                var remainingLength = lineAsASCII.Length - i;
                var remainingSpan = lineAsASCII.AsSpan(i, remainingLength);
                writer.WriteLine(remainingSpan);

                writtenLines += 1;
            }
        }

        // https://stackoverflow.com/a/10036907/2709212
        // Based on http://www.codeproject.com/Articles/13503/Stripping-Accents-from-Latin-Characters-A-Foray-in
        private static string LatinToAscii(string inString)
        {
            var newStringBuilder = new StringBuilder();

            newStringBuilder.Append(inString.Normalize(NormalizationForm.FormKD)
                .Where(x => x < 128)
                .ToArray());

            return newStringBuilder.ToString();
        }
    }
}
