using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII
{
	public sealed class VideoInfoLoader
	{
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
		
		public bool TryGetDuration(string filePath, out TimeSpan duration)
		{
			if (TryGetAnimatedWebPDuration(filePath, out duration))
			{
				return true;
			}
			
			if (FFProbeExecutablePath == null)
			{
				throw new ArgumentOutOfRangeException(nameof(FFProbeExecutablePath),
					"FFProbe executable file path not specified.");
			}
			
			var process = new System.Diagnostics.Process
			{
				StartInfo = new System.Diagnostics.ProcessStartInfo
				{
					FileName = FFProbeExecutablePath,
					Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{filePath}\"",
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				}
			};
			process.Start();
			var output = process.StandardOutput.ReadToEnd();
			process.WaitForExit();
			
			if (!double.TryParse(output, out var durationSeconds))
			{
				duration = default;
				return false;
			}
			
			duration = TimeSpan.FromSeconds(durationSeconds);
			return true;
		}

		private static bool TryGetAnimatedWebPDuration(string filePath, out TimeSpan duration)
		{
			using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			using var reader = new BinaryReader(stream);
			var possibleFourCCHeader = reader.ReadBytes(4);
			if (possibleFourCCHeader[0] != 0x52 || possibleFourCCHeader[1] != 0x49 || possibleFourCCHeader[2] != 0x46 || possibleFourCCHeader[3] != 0x46)
			{
				duration = default;
				return false;
			}

			try
			{
				// Skip the file size (4 bytes).
				reader.BaseStream.Seek(4, SeekOrigin.Current);
				// Skip the "WEBP" header (4 bytes).
				reader.BaseStream.Seek(4, SeekOrigin.Current);
				// Read the next 4 bytes as an ASCII string and compare it to "VP8X".
				var vp8xHeader = reader.ReadBytes(4);
				if (vp8xHeader[0] != 0x56 || vp8xHeader[1] != 0x50 || vp8xHeader[2] != 0x38 || vp8xHeader[3] != 0x58)
				{
					duration = default;
					return false;
				}
				
				// Okay, I've decided I don't care.
				// Use SixLabors.ImageSharp to get the duration.
				using var image = Image.Load(filePath);
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
			}
			catch
			{
				duration = default;
				return false;
			}
		}
	}
}
