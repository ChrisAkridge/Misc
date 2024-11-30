using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.IO.FileUtility.Commands;

namespace Celarix.IO.FileUtility.Logic
{
	internal static class MultifolderSequentialRenamer
	{
		public static void SequentiallyRename(MultifolderSequentialRenameCommand options)
		{
			var topLevelFolders = Directory.GetDirectories(options.FolderPath!, "*", SearchOption.TopDirectoryOnly);
			string[]? filesToRename;

			if (topLevelFolders.Length > 0)
			{
				filesToRename = topLevelFolders
					.Select(f =>
					{
						var filesInFolder = Directory.GetFiles(f, "*", SearchOption.TopDirectoryOnly)
							.OrderBy(p => p)
							.ToArray();

						return options.SeriesType switch
						{
							SeriesType.Picture when filesInFolder.Length > 500 => throw new ArgumentOutOfRangeException(
								nameof(f),
								$"The number of files ({filesInFolder.Length}) in folder '{f}' exceeds the maximum number of files that can be renamed in a picture-like series, which is 500."),
							SeriesType.Screenshot when filesInFolder.Length > 2000 => throw new
								ArgumentOutOfRangeException(
									nameof(f),
									$"The number of files in folder ({filesInFolder.Length}) '{f}' exceeds the maximum number of files that can be renamed in a screenshot-like series, which is 2000."),
							_ => filesInFolder
						};
					})
					.SelectMany(p => p)
					.ToArray();
			}
			else
			{
				Console.WriteLine("No child folders were found in the specified folder, using pictures from folder directly.");
				
				filesToRename = [.. Directory.GetFiles(options.FolderPath!, "*", SearchOption.TopDirectoryOnly).OrderBy(p => p)];
			}

			if (options.SeriesType == SeriesType.Picture)
			{
				var remainingPossibleFilesAfterStart = (26 * 500) - options.StartFrom;

				if (filesToRename.Length > remainingPossibleFilesAfterStart)
				{
					Console.WriteLine("The number of files exceeds the maximum number of files that can be renamed across 26 series. Excess files will be renamed by a single incrementing integer.");
				}
			}

			// First, prepend the desired number to the existing filename to avoid collisions in the second step.
			var currentFileNameNumber = options.StartFrom;
			for (var i = 0; i < filesToRename.Length; i++)
			{
				var fileToRename = filesToRename[i];
				if (Utilities.FileIsHiddenOrSystem(fileToRename)) { continue; }
				
				string newFileName;

				//if (options.SeriesType == SeriesType.Picture && cur)
				//{
				//	newFileName = options.SeriesType switch
				//	{
				//		SeriesType.Picture => $"{GetPictureSeriesFileName(currentFileNameNumber)}_{Path.GetFileName(fileToRename)}",
				//		SeriesType.Screenshot => $"{GetScreenshotSeriesFileNumber(currentFileNameNumber)}_{Path.GetFileName(fileToRename)}",
				//		_ => throw new InvalidOperationException("Invalid series type.")
				//	};
				//}
				if (options.SeriesType == SeriesType.Picture)
				{
					newFileName = currentFileNameNumber < 26 * 500
						? $"{GetPictureSeriesFileName(currentFileNameNumber)}_{Path.GetFileName(fileToRename)}"
						: $"{currentFileNameNumber}_{Path.GetFileName(fileToRename)}";
				}
				else if (options.SeriesType == SeriesType.Screenshot)
				{
					newFileName =
						$"{GetScreenshotSeriesFileNumber(currentFileNameNumber)}_{Path.GetFileName(fileToRename)}";
				}
				else { throw new InvalidOperationException("Invalid series type."); }
				var newFilePath = Path.Combine(Path.GetDirectoryName(fileToRename)!, newFileName);
				File.Move(fileToRename, newFilePath);
				filesToRename[i] = newFilePath;
				currentFileNameNumber += 1;
			}

			// Then, remove the original portion of the filename, completing the rename.
			currentFileNameNumber = options.StartFrom;
			foreach (var fileToRename in filesToRename)
			{
				if (Utilities.FileIsHiddenOrSystem(fileToRename)) { continue; }

				var substringLength = options.SeriesType switch
				{
					SeriesType.Picture => 1 + 3,
					SeriesType.Screenshot => 2 + 6,
					_ => throw new InvalidOperationException("Invalid series type.")
				};
				var newFileName = Path.GetFileName(fileToRename)[..substringLength];

				var newFilePath = Path.Combine(Path.GetDirectoryName(fileToRename)!,
					$"{newFileName}{Path.GetExtension(fileToRename)}");
				File.Move(fileToRename, newFilePath);
				currentFileNameNumber += 1;
			}
		}

		private static string GetPictureSeriesFileName(int fileNameNumber)
		{
			var seriesNumber = fileNameNumber / 500;
			var fileNumber = fileNameNumber % 500;
			
			var seriesLetter = (char)('a' + seriesNumber);
			return $"{seriesLetter}{fileNumber + 1:D3}";
		}

		private static string GetScreenshotSeriesFileNumber(int fileNameNumber)
		{
			var seriesNumber = fileNameNumber / 2000;
			var fileNumber = fileNameNumber % 2000;

			return $"{seriesNumber}s{fileNumber:D6}";
		}
	}
}
