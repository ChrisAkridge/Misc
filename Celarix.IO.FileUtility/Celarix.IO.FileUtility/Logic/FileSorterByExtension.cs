using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.IO.FileUtility.Commands;

namespace Celarix.IO.FileUtility.Logic
{
	internal static class FileSorterByExtension
	{
		public static void SortFilesIntoFoldersByExtension(SortIntoFoldersByExtensionCommand options)
		{
			var files = Directory.GetFiles(options.FolderPath!);

			foreach (var file in files)
			{
				var extension = Path.GetExtension(file).TrimStart('.').ToUpperInvariant();
				var extensionFolder = Path.Combine(options.FolderPath!, extension);

				if (!Directory.Exists(extensionFolder)) { Directory.CreateDirectory(extensionFolder); }

				var fileName = Path.GetFileName(file);
				var newFilePath = Path.Combine(extensionFolder, fileName);

				Console.WriteLine($"Moving file {file} to {newFilePath}...");
				File.Move(file, newFilePath);
			}
		}
	}
}
