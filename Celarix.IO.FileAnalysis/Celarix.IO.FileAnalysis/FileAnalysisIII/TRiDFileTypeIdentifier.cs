using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII
{
	public sealed class TRiDFileTypeIdentifier
	{
		public string TRiDNetExecutablePath
		{
			get
			{
				return Environment.MachineName switch
				{
					"PAVILION-CORE" => @"F:\Documents\Files\Software\TRiD\trid.exe",
					"AKRIDGE-PC" => null,
					"Bluebell01" => null,
					_ => throw new ArgumentOutOfRangeException(nameof(Environment.MachineName), $"Unrecognized machine {Environment.MachineName}.")
				};
			}
		}
		
		public string IdentifyFileType(string filePath)
		{
			if (TRiDNetExecutablePath == null)
			{
				throw new ArgumentOutOfRangeException(nameof(TRiDNetExecutablePath),
					"TRiD executable file path not specified.");
			}
			
			var process = new System.Diagnostics.Process
			{
				StartInfo = new System.Diagnostics.ProcessStartInfo
				{
					FileName = TRiDNetExecutablePath,
					Arguments = $"\"{filePath}\"",
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				}
			};
			process.Start();
			var output = process.StandardOutput.ReadToEnd();
			process.WaitForExit();
			
			var outputLines = output.Split(new[]
			{
				 '\r',
				 '\n'
			}, StringSplitOptions.RemoveEmptyEntries);
			
			if (outputLines.Length < 5)
			{
				return "Unknown file type";
			}
			
			var mostLikelyFileType = outputLines.Length > 1 ? outputLines[4] : null;
			var lastLeftParenthesisIndex = mostLikelyFileType?.LastIndexOf('(') ?? -1;
			return mostLikelyFileType?[..lastLeftParenthesisIndex].Trim();
		}
	}
}
