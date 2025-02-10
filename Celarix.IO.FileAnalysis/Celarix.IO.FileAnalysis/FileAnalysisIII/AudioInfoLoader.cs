using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII
{
	public sealed class AudioInfoLoader
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		
		public string FFProbeExecutablePath
		{
			get
			{
				return Environment.MachineName switch
				{
					"PAVILION-CORE" =>
						@"F:\Documents\Files\Software\ffmpeg-2024-03-28-git-5d71f97e0e-full_build\bin\ffprobe.exe",
					"AKRIDGE-PC" => null,
					"Bluebell01" => null,
					_ => throw new ArgumentOutOfRangeException(nameof(Environment.MachineName),
						$"Unrecognized machine {Environment.MachineName}.")
				};
			}
		}

		public bool TryGetInfo(string filePath, out AudioInfo info)
		{
			try
			{
				var process = new Process
				{
					StartInfo = new()
					{
						FileName = FFProbeExecutablePath,
						Arguments = $"\"{filePath}\"",
						UseShellExecute = false,
						RedirectStandardError = true,
						CreateNoWindow = true
					}
				};
				process.Start();
				var output = process.StandardError.ReadToEnd();
				process.WaitForExit();
				
				// Split the output into lines
				var outputLines = output.Split(new[]
				{
					'\r', '\n'
				}, StringSplitOptions.RemoveEmptyEntries)
				.Select(l => l.Trim())
				.ToArray();
				
				// Find the line starting with "Duration: "
				var durationLine = outputLines.FirstOrDefault(l => l.StartsWith("Duration: ", StringComparison.Ordinal));
				if (durationLine == null)
				{
					info = null;
					return false;
				}

				var durationText = durationLine.Substring("Duration: ".Length).Split(',')[0];
				var duration = TimeSpan.Parse(durationText);
				
				// Find the line starting with "Stream #0:0"
				var streamLine = outputLines.FirstOrDefault(l => l.StartsWith("Stream #0:0", StringComparison.Ordinal));
				if (streamLine == null)
				{
					info = null;
					return false;
				}
				var streamParts = streamLine.Split(',', StringSplitOptions.RemoveEmptyEntries);
				var sampleRatePart = streamParts.FirstOrDefault(p => p.Contains("Hz"))?.Trim();
				if (sampleRatePart == null)
				{
					info = null;
					return false;
				}
				var sampleRateText = sampleRatePart.Split(' ')[0];
				var sampleRate = int.Parse(sampleRateText);

				info = new AudioInfo
				{
					SampleRate = sampleRate,
					Duration = duration,
					Channels = streamParts.Count(p => p.Contains("stereo")) == 1 ? 2 : 1
				};
				logger.Info($"{filePath} (audio): {info.SampleRate} Hz, {info.Channels}-channel audio, {info.Duration}");

				return true;
			}
			catch
			{
				info = null;
				return false;
			}
		}
	}
}
