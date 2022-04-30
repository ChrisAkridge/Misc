using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Celarix.IO.FileAnalysis.Analysis.Clients;
using NLog;
using LongPath = Pri.LongPath.Path;
using LongFile = Pri.LongPath.File;
using LongDirectory = Pri.LongPath.Directory;
using LongDirectoryInfo = Pri.LongPath.DirectoryInfo;

namespace Celarix.IO.FileAnalysis.Analysis
{
    internal sealed class ClientAnalyzer
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly byte[] executableFirstBytes = new byte[128];

        public List<string> TryAnalyzeWithClients(string filePath)
        {
            var generatedFilePaths = new List<string>();
            
            var isExecutable = IsExecutable(filePath);
            var isArchive = isExecutable || SevenZipClient.IsArchive(filePath);
            var isAssembly = IsAssembly(filePath);

            if (!isExecutable && !isArchive && !isAssembly)
            {
                return new List<string>();
            }

            if (isArchive)
            {
                SevenZipClient.TryExtract(filePath);

                generatedFilePaths.AddRange(LongDirectory.GetFiles(SevenZipClient.GetExtractionPathForFile(filePath),
                        "*",
                        SearchOption.AllDirectories));
            }

            if (isExecutable && !isAssembly)
            {
                var success = ObjConvClient.TryDisassemble(filePath);
                logger.Info($"{(success ? "Disassembled" : "Failed to disassemble")} {filePath}");
                
                generatedFilePaths.AddRange(LongDirectory.GetFiles(ObjConvClient.GetDisassemblyFolderPathForFile(filePath), "*", SearchOption.AllDirectories));
            }

            if (isAssembly)
            {
                var success = ILSpyClient.TryDecompile(filePath);
                logger.Info($"{(success ? "Decompiled" : "Failed to decompile")} {filePath}");
                
                generatedFilePaths.AddRange(LongDirectory.GetFiles(ILSpyClient.GetOutputFolderPath(filePath), "*", SearchOption.AllDirectories));
            }

            return generatedFilePaths;
        }

        private bool IsExecutable(string filePath)
        {
            var extension = LongPath.GetExtension(filePath).ToLowerInvariant();

            if (extension == ".exe" || extension == ".dll")
            {
                logger.Info($"{filePath} is a Windows PE file");

                return true;
            }

            using var stream = File.OpenRead(filePath);

            stream.Read(executableFirstBytes, 0, 128);
            string firstChunkAsASCII = Encoding.ASCII.GetString(executableFirstBytes);

            var isExecutable = firstChunkAsASCII.StartsWith("MZ", StringComparison.Ordinal)
                && firstChunkAsASCII.Contains("This program cannot be run in DOS mode.");
            logger.Info($"{filePath} {(isExecutable ? "is" : "is not")} a Windows PE file");

            return isExecutable;
        }

        private bool IsAssembly(string filePath)
        {
            var isAssembly = true;

            try { AssemblyName.GetAssemblyName(filePath); }
            catch (BadImageFormatException) { isAssembly = false; }
            catch (FileLoadException) { isAssembly = false; }
            catch (OutOfMemoryException) { isAssembly = false; }

            logger.Info($"{filePath} {(isAssembly ? "is" : "is not")} a .NET assembly");

            return isAssembly;
        }
    }
}
