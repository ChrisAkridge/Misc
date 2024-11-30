using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Celarix.Imaging;
using Celarix.IO.FileAnalysis.Analysis;
using Celarix.IO.FileAnalysis.FileAnalysisIII;
using Celarix.IO.FileAnalysis.FinalProcessing;
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
	        LoggingConfigurer.ConfigurePostProcessingLogging();
	        var distributionsGenerator = new FileDistributionGenerator();

	        var distributions = distributionsGenerator.GenerateFileDistributions(
		        @"F:\Documents\Files\Music\Recordings\Miscellaneous\(2024-11-05) KIRO Seattle - Election Day.mp3");
	        
	        using var distributionStream = new BinaryWriter(File.OpenWrite(@"F:\Documents\Files\Unclassified Files\kiro_seattle.bin"));
	        foreach (var distribution in distributions)
	        {
		        distribution.Write(distributionStream);
	        }
	        
	        return;
            // TODO: add CommandLine library
            Celarix.Imaging.LibraryConfiguration.Instance = new LibraryConfiguration
            {
                BinaryDrawingReportsProgressEveryNPixels = 1048576,
                ZoomableCanvasTileEdgeLength = 1024
            };

            if (args[0].Equals(" - postprocess", StringComparison.InvariantCultureIgnoreCase))
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
            else if (args[0].Equals("-partialpostprocess", StringComparison.InvariantCultureIgnoreCase))
            {
                // dammit you're supposed to delete the INPUT folder, not the output folder

                var folderPath = args[1];
                
                PostProcessor.PartialPostProcess(folderPath);
            }
            else if (args[0].Equals("-inserttabs", StringComparison.InvariantCultureIgnoreCase))
            {
                var inputPath = args[1];
                var outputPath = args[2];
                var tabsToInsert = int.Parse(args[3]);
                var lineToPrepend = args[4];
                
                TabInserter.InsertTabsForFile(inputPath, outputPath, tabsToInsert, lineToPrepend);
            }
            else if (args[0].Equals("-generatethinfiletree", StringComparison.InvariantCultureIgnoreCase))
            {
                var inputPath = args[1];
                var outputPath = args[2];
                
                ThinFileTreeGenerator.GenerateThinFileTree(inputPath, outputPath);
            }
            else if (args[0].Equals("-movenumberedtextmaps", StringComparison.InvariantCultureIgnoreCase))
            {
                var textMapsPath = args[1];
                
                NumberedTextMapMover.MoveNumberedTextMaps(textMapsPath);
            }
            else if (args[0].Equals("-combinecanvases", StringComparison.InvariantCultureIgnoreCase))
            {
                var canvasPathsFilePath = args[1];
                var outputPath = args[2];
                
                CanvasCombiner.CombineCanvases(File.ReadAllLines(canvasPathsFilePath), outputPath);
            }
            else if (args[0].Equals("-findvideos", StringComparison.InvariantCultureIgnoreCase))
            {
                var searchFolderPath = args[1];
                var listOutputPaths = args[2];
                
                VideoFinder.FindAllVideosInFolder(searchFolderPath, listOutputPaths);
            }
            else if (args[0].Equals("-generatethumbnails", StringComparison.InvariantCultureIgnoreCase))
            {
                var videoListPath = args[1];
                var ezThumbBinaryPath = args[2];
                var outputFolderPath = args[3];
                
                VideoThumbnailGenerator.GenerateThumbnailsForVideos(videoListPath, ezThumbBinaryPath, outputFolderPath);
            }
            else if (args[0].Equals("-concatenatetextfiles", StringComparison.InvariantCultureIgnoreCase))
            {
                var inputFolderPath = args[1];
                var outputPath = args[2];
                
                TextFileConcatenator.ConcatenateTextFilesInFolder(inputFolderPath, outputPath);
            }
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

        private static void MultiPrependAndIndent(string filePath, IReadOnlyList<string> prependStrings)
        {
            var currentInputFilePath = filePath;
            
            for (int i = 0; i < prependStrings.Count; i++)
            {
                var outputFilePath = (i < prependStrings.Count - 1)
                    ? Path.Combine(Path.GetDirectoryName(currentInputFilePath),
                        $"{Path.GetFileNameWithoutExtension(currentInputFilePath)}_prepended{i}{Path.GetExtension(currentInputFilePath)}")
                    : Path.Combine(Path.GetDirectoryName(currentInputFilePath),
                        $"{Path.GetFileNameWithoutExtension(currentInputFilePath)}_final{Path.GetExtension(currentInputFilePath)}");
                TabInserter.InsertTabsForFile(currentInputFilePath, outputFilePath, 1, prependStrings[i]);
                currentInputFilePath = outputFilePath;
            }
        }

        private static string[] GetPrependStringsForTreeFiles(string filePath) =>
            new[]
            {
                Path.GetFileNameWithoutExtension(filePath)
            };

        private static void ConcatenateAllLogsInFolder(string folderPath)
        {
            LoggingConfigurer.ConfigurePostProcessingLogging();
            var logger = LogManager.GetCurrentClassLogger();
            
            var annexFolders = Directory.GetDirectories(folderPath, "*", SearchOption.TopDirectoryOnly);
            var comparer = new LogFileNameComparer();
            var linesWritten = 0;

            foreach (var annexFolder in annexFolders)
            {
                logger.Info($"Concatenating all logs in {annexFolder}...");
                
                var logFiles = Directory.GetFiles(annexFolder, "*.log", SearchOption.TopDirectoryOnly)
                    .OrderBy(f => Path.GetFileName(f), comparer);
                var annexFolderName = Path.GetFileName(annexFolder);
                var outputPath = Path.Combine(folderPath, $"{annexFolderName}.log");
                using var outputStream = new StreamWriter(File.OpenWrite(outputPath));
                
                foreach (var logFile in logFiles)
                {
                    logger.Info($"Concatenating {logFile}...");
                    using var inputStream = new StreamReader(File.OpenRead(logFile));

                    while (inputStream.ReadLine() is { } line)
                    {
                        outputStream.WriteLine(line);
                        linesWritten += 1;

                        if (linesWritten % 100000 == 0) { logger.Info($"Wrote {linesWritten} lines"); }
                    }
                }
            }
        }
    }

    internal sealed class LogFileNameComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            var (xDate, xFileNumber) = GetParts(x);
            var (yDate, yFileNumber) = GetParts(y);
            
            var dateComparison = xDate.CompareTo(yDate);

            return dateComparison != 0 ? dateComparison : xFileNumber.CompareTo(yFileNumber);
        }

        private static (DateOnly date, int fileNumber) GetParts(string fileName)
        {
            var parts = fileName.Split('.');
            var date = DateOnly.Parse(parts[0].TrimEnd('_'));

            return parts.Length == 3
                ? (date, int.Parse(parts[1]))
                : parts[0].EndsWith('_')
                    ? (date, 0)
                    : (date, int.MaxValue);
        }
    }
}
