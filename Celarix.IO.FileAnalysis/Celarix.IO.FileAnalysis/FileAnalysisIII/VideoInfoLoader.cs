using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using SixLabors.ImageSharp;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII
{
	public sealed class VideoInfoLoader
	{
		private static readonly Logger logger = LogManager.GetCurrentClassLogger();
		
		public string FFProbeExecutablePath
		{
			get
			{
				return Environment.MachineName switch
				{
					"PAVILION-CORE" => @"F:\Documents\Files\Software\ffmpeg-2024-03-28-git-5d71f97e0e-full_build\bin\ffprobe.exe",
					"AKRIDGE-PC" => null,
					"Bluebell01" => null,
					_ => throw new ArgumentOutOfRangeException(nameof(Environment.MachineName),
						$"Unrecognized machine {Environment.MachineName}.")
				};
			}
		}
		
		public bool TryGetInfo(string filePath, out AnimatedImageOrVideoInfo info)
		{
			if (TryGetAnimatedImageInfo(filePath, out info))
			{
				return true;
			}
			
			if (FFProbeExecutablePath == null)
			{
				throw new ArgumentOutOfRangeException(nameof(FFProbeExecutablePath),
					"FFProbe executable file path not specified.");
			}
			
			var durationProcess = new Process
			{
				StartInfo = GetFFProbeProcessStartInfo(
					$"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{filePath}\"")
			};
			durationProcess.Start();
			var output = durationProcess.StandardOutput.ReadToEnd();
			durationProcess.WaitForExit();
			
			if (!double.TryParse(output, out var durationSeconds))
			{
				info = default;
				return false;
			}
			
			var dimensionsProcess = new Process
			{
				StartInfo = GetFFProbeProcessStartInfo(
					$"-v error -select_streams v -show_entries stream=width,height -of csv=s=x:p=0 \"{filePath}\"")
			};
			dimensionsProcess.Start();
			output = dimensionsProcess.StandardOutput.ReadToEnd();
			var outputParts = output.Split('x');
			dimensionsProcess.WaitForExit();
			
			var frameCountProcess = new Process
			{
				StartInfo = GetFFProbeProcessStartInfo(
					$"-v error -count_frames -select_streams v:0 -show_entries stream=nb_frames -of default=nokey=1:noprint_wrappers=1 \"{filePath}\"")
			};
			frameCountProcess.Start();
			output = frameCountProcess.StandardOutput.ReadToEnd();
			frameCountProcess.WaitForExit();

			info = new AnimatedImageOrVideoInfo
			{
				Width = int.Parse(outputParts[0]),
				Height = int.Parse(outputParts[1]),
				FrameCount = int.Parse(output),
				Duration = TimeSpan.FromSeconds(durationSeconds)
			};
			
			logger.Info($"{filePath} (video): {info.Width}x{info.Height}, {info.FrameCount} frames, {info.Duration}");
			
			return true;
		}

		private ProcessStartInfo GetFFProbeProcessStartInfo(string arguments) =>
			new()
			{
				FileName = FFProbeExecutablePath,
				Arguments = arguments,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				CreateNoWindow = true
			};

		private static bool TryGetAnimatedImageInfo(string filePath, out AnimatedImageOrVideoInfo info)
		{
			try
			{
				using var image = Image.Load(filePath);
				
				if (image.Frames.Count == 1)
				{
					info = null;
					return false;
				}
				
				var totalDurationMilliseconds = 0u;

				foreach (var imageFrame in image.Frames)
				{
					if (imageFrame.Metadata.TryGetWebpFrameMetadata(out var webpMetadata))
					{
						totalDurationMilliseconds += webpMetadata.FrameDelay;
					}
					else if (imageFrame.Metadata.TryGetGifMetadata(out var gifMetadata))
					{
						totalDurationMilliseconds += (uint)gifMetadata.FrameDelay;
					}
				}
				
				info = new AnimatedImageOrVideoInfo
				{
					Width = image.Width,
					Height = image.Height,
					FrameCount = image.Frames.Count,
					Duration = TimeSpan.FromMilliseconds(totalDurationMilliseconds)
				};
				
				logger.Info($"{filePath} (animated image): {info.Width}x{info.Height}, {info.FrameCount} frames, {info.Duration}");
				
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
