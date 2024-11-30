using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.IO.FileUtility.Commands;

namespace Celarix.IO.FileUtility.Logic
{
	internal static class FilesIntoSeriesSorter
	{
		public static void SortFilesIntoSeries(SortFilesIntoSeriesCommand options)
		{
			var filesToSort = Directory.GetFiles(options.FolderPath!, "*", SearchOption.TopDirectoryOnly)
				.OrderBy(f => f)
				.ToArray();

			if (options.SeriesType == SeriesType.Picture)
			{
				var remainingPossibleFileNumbersAfterStartFrom = (26 * 500) - options.StartFrom;

				if (filesToSort.Length > remainingPossibleFileNumbersAfterStartFrom)
				{
					Console.WriteLine("Given the file count and the starting filename, there are not enough possible filenames to cover all files.");
					Console.WriteLine("Excess files will be assigned a single incrementing integer.");
				}
				
				var currentFileNumber = options.StartFrom;

				foreach (var file in filesToSort)
				{
					if (currentFileNumber < 26 * 500)
					{
						var seriesNumber = currentFileNumber / 500;
						var seriesName = (char)('a' + seriesNumber);
						var seriesFolderName = $"{char.ToUpper(seriesName)} Series";
						var fileNumberInSeries = (currentFileNumber % 500) + 1;
					
						var destinationFolderPath = Path.Combine(options.FolderPath!, seriesFolderName);
						var newFileName = $"{seriesName}{fileNumberInSeries:D3}{Path.GetExtension(file)}";
						Console.WriteLine($"Moving {file} to {seriesFolderName}/{newFileName}.");
						CreateDirectoryAndMoveFile(file, newFileName, destinationFolderPath);
					}
					else
					{
						var fileNumberInSeries = (currentFileNumber - (26 * 500)) + 1;
						
						var destinationFolderPath = Path.Combine(options.FolderPath!, "Overflow");
						var newFileName = $"{fileNumberInSeries:D8}{Path.GetExtension(file)}";
						Console.WriteLine($"Moving {file} to Overflow/{newFileName}.");
						CreateDirectoryAndMoveFile(file, newFileName, destinationFolderPath);
					}
					
					currentFileNumber += 1;
				}
			}
			else if (options.SeriesType == SeriesType.Screenshot)
			{
				var currentFileNumber = options.StartFrom;

				foreach (var file in filesToSort)
				{
					var seriesNumber = currentFileNumber / 2000;
					var seriesFolderName = $"{seriesNumber + 1}s Series";
					var fileNumberInSeries = (currentFileNumber % 2000) + 1;
					
					var destinationFolderPath = Path.Combine(options.FolderPath!, seriesFolderName);
					var newFileName = $"{seriesNumber + 1}s{fileNumberInSeries:D6}{Path.GetExtension(file)}";
					Console.WriteLine($"Moving {file} to {seriesFolderName}/{newFileName}.");
					CreateDirectoryAndMoveFile(file, newFileName, destinationFolderPath);
					
					currentFileNumber += 1;
				}
			}
			else { Console.WriteLine("Invalid series type."); }
		}

		private static void CreateDirectoryAndMoveFile(string sourceFilePath, string newFileName, string destinationFolderPath)
		{
			Directory.CreateDirectory(destinationFolderPath);
			File.Move(sourceFilePath, Path.Combine(destinationFolderPath, newFileName));
		}
	}
}
