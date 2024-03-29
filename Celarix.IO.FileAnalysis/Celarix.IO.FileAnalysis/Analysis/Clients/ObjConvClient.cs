﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Celarix.IO.FileAnalysis.Extensions;
using NLog;
using LongDirectory = Pri.LongPath.Directory;
using LongPath = Pri.LongPath.Path;
using LongFile = Pri.LongPath.File;

namespace Celarix.IO.FileAnalysis.Analysis.Clients
{
    internal static class ObjConvClient
    {
        private const string ObjConvPath = "ClientBinaries\\objconv.exe";
        private const string DisassemblyFolderName = "disasm";
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static bool TryDisassemble(string filePath, out string disassemblyPath)
        {
            logger.Trace($"Attempting to disassemble {filePath}...");

            var disassemblyDirectoryPath = GetDisassemblyFolderPathForFile(filePath);

            LongDirectory.CreateDirectory(disassemblyDirectoryPath);

            disassemblyPath = LongPath.Combine(disassemblyDirectoryPath,
                LongPath.GetFileNameWithoutExtension(filePath) + ".asm");

            if (LongFile.Exists(disassemblyPath))
            {
                logger.Warn($"The file at {filePath} has already been disassembled! Skipping...");
                return true;
            }

            var startInfo = new ProcessStartInfo(ObjConvPath, $"-fnasm \"{filePath}\" \"{disassemblyPath}\"")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                WorkingDirectory = LongPath.GetDirectoryName(filePath)
            };

            var runObjConvTask = Task.Run(() => TryRunObjConv(startInfo));

            return runObjConvTask.Wait(TimeSpan.FromMinutes(10d)) && runObjConvTask.Result;
        }

        private static bool TryRunObjConv(ProcessStartInfo startInfo)
        {
            try
            {
                using var objconv = Process.Start(startInfo);

                var reader = objconv?.StandardOutput;
                var output = reader?.ReadToEnd();

                return output?.Contains("Error") == false;
            }
            catch (Exception ex)
            {
                logger.LogException(ex);

                return false;
            }
        }

        public static string GetDisassemblyFolderPathForFile(string filePath)
        {
            var directoryPath = LongPath.GetDirectoryName(filePath);

            return LongPath.Combine(directoryPath, DisassemblyFolderName);
        }
    }
}
