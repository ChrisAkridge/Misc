using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.IO.FileUtility.Commands;

namespace Celarix.IO.FileUtility.Logic
{
	internal static class ExtensionFixer
	{
		public static void FixExtensions(FixExtensionsCommand options)
		{
			var files = Directory.GetFiles(options.FolderPath!, "*.unknown", SearchOption.AllDirectories);

			foreach (var filePath in files)
			{
				// Most commonly, we have JPEG files with the wrong extension.
				// Check if it's a JPEG file by its bytes first.
				var fileCheckBuffer = new byte[10];
				var fileStream = new BinaryReader(File.OpenRead(filePath));
				var readBytes = fileStream.Read(fileCheckBuffer, 0, fileCheckBuffer.Length);
				fileStream.Close();
				fileStream.Dispose();

				if (readBytes < 10)
				{
					continue;
				}
				
				var fileIsJpeg = fileCheckBuffer[0] == 0xFF
					&& fileCheckBuffer[1] == 0xD8
					&& fileCheckBuffer[2] == 0xFF
					&& fileCheckBuffer[3] == 0xE0
					&& fileCheckBuffer[6] == 0x4A
					&& fileCheckBuffer[7] == 0x46
					&& fileCheckBuffer[8] == 0x49
					&& fileCheckBuffer[9] == 0x46;

				if (!fileIsJpeg) { continue; }
				
				Console.WriteLine($"Fixing extension for JPEG file {filePath}...");
				var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
				var newFileName = $"{fileNameWithoutExtension}.jpg";
				var newFilePath = Path.Combine(Path.GetDirectoryName(filePath)!, newFileName);
				File.Move(filePath, newFilePath);
			}
		}
	}
}
