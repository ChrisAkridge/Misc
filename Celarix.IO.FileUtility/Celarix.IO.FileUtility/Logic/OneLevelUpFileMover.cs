using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.IO.FileUtility.Commands;

namespace Celarix.IO.FileUtility.Logic
{
	internal static class OneLevelUpFileMover
	{
		public static void MoveFilesAndFoldersOneLevelUp(MoveFilesUpOneLevelCommand options)
		{
			var topLevelFolders = Directory
				.GetDirectories(options.FolderPath!, "*", SearchOption.TopDirectoryOnly)
				.OrderByAlphaNumeric(f => f)
				.ToArray();

			if (topLevelFolders.Length == 0)
			{
				Console.WriteLine($"The folder {options.FolderPath} has no child folders, so there is nothing to move up.");

				return;
			}
			
			// Check if any existing top-level folders start with 8-digit numbers followed by an underscore.
			// If so, find the highest number and start incrementing from there.
			var existingNumberedFolders = topLevelFolders
				.Where(f => f.Length >= 9 && f[8] == '_' && f.Substring(0, 8).All(char.IsDigit))
				.Select(f => int.Parse(f.Substring(0, 8)))
				.ToArray();
			
			// Start by moving folders-inside-folders up first, prepending overall numbers to avoid conflicts and preserve order.
			// (i.e. Root/Child1/Child2 -> Root/Child2)
			var movedFolderIndex = existingNumberedFolders.Any()
				? existingNumberedFolders.Max() + 1
				: 0;

			foreach (var topLevelFolder in topLevelFolders)
			{
				var secondLevelFolders = Directory.GetDirectories(topLevelFolder, "*", SearchOption.TopDirectoryOnly);

				foreach (var secondLevelFolder in secondLevelFolders)
				{
					var movedFolderName = $"{movedFolderIndex:D8}_{Path.GetFileName(secondLevelFolder)}";
					
					var destinationPath = Path.Combine(options.FolderPath!, movedFolderName);
					Console.WriteLine($"Moving {secondLevelFolder} to {movedFolderName}");
					Directory.Move(secondLevelFolder, destinationPath);
					movedFolderIndex += 1;
				}
			}
			
			// Check if any existing top-level files start with 8-digit numbers followed by an underscore.
			// If so, find the highest number and start incrementing from there.
			var existingNumberedFiles = Directory
				.GetFiles(options.FolderPath!, "*", SearchOption.TopDirectoryOnly)
				.Where(f => f.Length >= 9 && f[8] == '_' && f.Substring(0, 8).All(char.IsDigit))
				.Select(f => int.Parse(f.Substring(0, 8)))
				.ToArray();
			
			// Then, move files-inside-folders up, handling duplicate names.
			// (i.e. Root/Child1/File1.ext -> Root/File1.ext)
			var movedFileIndex = existingNumberedFiles.Any()
				? existingNumberedFiles.Max() + 1
				: 0;

			foreach (var topLevelFolder in topLevelFolders)
			{
				var files = Directory.GetFiles(topLevelFolder, "*", SearchOption.TopDirectoryOnly);

				foreach (var file in files)
				{
					var movedFileName = $"{movedFileIndex:D8}_{Path.GetFileName(file)}";
					
					var destinationPath = Path.Combine(options.FolderPath!, movedFileName);
					Console.WriteLine($"Moving {file} to {movedFileName}");
					File.Move(file, destinationPath);
					movedFileIndex += 1;
				}
			}
			
			// Finally, delete the now-empty top-level folders.
			foreach (var topLevelFolder in topLevelFolders)
			{
				var fileSystemEntries = Directory.GetFileSystemEntries(topLevelFolder).Length == 0;

				if (fileSystemEntries)
				{
					Console.WriteLine($"Deleting empty folder {topLevelFolder}.");
					Directory.Delete(topLevelFolder);
				}
				else
				{
					Console.WriteLine($"Failed to move all files out of {topLevelFolder}! {fileSystemEntries} remain! Aborting.");

					return;
				}
			}
		}
	}
}
