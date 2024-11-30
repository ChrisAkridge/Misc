using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.IO.FileUtility.Commands;

namespace Celarix.IO.FileUtility.Logic
{
	internal static class SequentialRenamer
	{
		public static void SequentialRename(SequentialRenameCommand options)
		{
			var filePaths = Directory
				.GetFiles(options.FolderPath!, "*", SearchOption.TopDirectoryOnly)
				.OrderBy(p => p)
				.ToArray();

			var digitCount = options.DigitCount;
			var minRequiredDigitCount = (int)Math.Ceiling(Math.Log10(filePaths.Length));

			if (minRequiredDigitCount > options.DigitCount)
			{
				Console.WriteLine($"The specified digit count of {options.DigitCount} is too low for the number of files in the folder. Automatically increasing to {minRequiredDigitCount}.");
				digitCount = minRequiredDigitCount;
			}

			// First, prepend the desired number to the existing filename to avoid collisions in the second step.
			for (var i = 0; i < filePaths.Length; i++)
			{
				var fileToRename = filePaths[i];

				if (Utilities.FileIsHiddenOrSystem(fileToRename)) { continue; }

				var fileNumber = i + options.StartNumber;
				var fileNumberText = options.Prefix + fileNumber.ToString().PadLeft(digitCount, '0');
				var newFileName = $"{fileNumberText}_{Path.GetFileName(fileToRename)}";
				var newFilePath = Path.Combine(Path.GetDirectoryName(fileToRename)!, newFileName);
				Console.WriteLine($"Renaming {fileToRename} to {newFileName}");
				File.Move(fileToRename, newFilePath);
				filePaths[i] = newFilePath;
			}
			
			// Then, remove the original portion of the filename, completing the rename.
			foreach (var fileToRename in filePaths)
			{
				if (Utilities.FileIsHiddenOrSystem(fileToRename)) { continue; }

				var newFileName = Path.GetFileName(fileToRename)[..(digitCount + options.Prefix.Length)];
				var newFilePath = Path.Combine(Path.GetDirectoryName(fileToRename)!, $"{newFileName}{Path.GetExtension(fileToRename)}");
				Console.WriteLine($"Renaming {fileToRename} to {newFileName}");
				File.Move(fileToRename, newFilePath);
			}
		}
	}
}
