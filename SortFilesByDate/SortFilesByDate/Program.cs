using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace SortFilesByDate
{
	public class Program
	{
		private static readonly string[] monthNames = new string[]
			{ "January", "February", "March", "April", "May", "June",
			  "July", "August", "September", "October", "November", "December" };

		public static void Main(string[] args)
		{
			Console.WriteLine("SortFilesByDate - sorts all files in a directory into folders by date");
			
			if (args.Length != 1 || args.Length != 3)
			{
				Console.WriteLine("\tUsage: SortFilesByDate \"<input directory>\"");
			}

			if (args[0] == "-v")
			{
				ValidateFolders(args[1], args[2]);
				Environment.Exit(0);
			}

			// Load the info for the files.

			Console.WriteLine("Loading files information...");
			string folderPath = args[0];
			string parentFolder = Directory.GetParent(folderPath).FullName;
			string folderName = Path.GetFileName(args[0]);

			string[] filePaths = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
			Dictionary<string, FileInfo> filesInfo = new Dictionary<string, FileInfo>(filePaths.Length);
			
			foreach (string filePath in filePaths)
			{
				filesInfo.Add(filePath, new FileInfo(filePath));
			}

			Console.WriteLine("\tLoaded {0} files.", filesInfo.Count);

			// Sort the files by creation date.
			Console.WriteLine("Sorting by creation date...");

			Dictionary<DateTime, List<string>> sortedFiles = new Dictionary<DateTime, List<string>>();

			foreach (FileInfo fileInfo in filesInfo.Values)
			{
				DateTime lastWriteDateTime = fileInfo.LastWriteTime;
				DateTime lastWriteDate = new DateTime(lastWriteDateTime.Year, lastWriteDateTime.Month, lastWriteDateTime.Day);

				if (!sortedFiles.ContainsKey(lastWriteDate))
				{
					sortedFiles.Add(lastWriteDate, new List<string>());
				}

				sortedFiles[lastWriteDate].Add(fileInfo.FullName);
			}

			Console.WriteLine("\tSorted into {0} days", sortedFiles.Count);

			// Copy the files
			Console.WriteLine("Copying files...");

			string tempFolderName = Path.Combine(parentFolder, $"{folderName}_tmp");
			Directory.CreateDirectory(tempFolderName);

			foreach (var date in sortedFiles.Keys)
			{
				// Create the folders for the years and days
				string dateFolderPath = Path.Combine(tempFolderName, $"{date.Year}", monthNames[date.Month - 1], GetDayName(date));
				Directory.CreateDirectory(dateFolderPath);

				// Copy the files from this date into the folder
				foreach (var file in sortedFiles[date])
				{
					string newPath = string.Concat(dateFolderPath, @"\", Path.GetFileName(file));

					try
					{
						File.Copy(file, newPath);
					}
					catch (IOException exception)
					{
						if (exception.Message.Contains("already exists"))
						{
							continue;
						}
						else
						{
							Console.WriteLine($"\tException: {exception.Message}");
							Environment.Exit(1);
						}
					}
				}

				Console.WriteLine($"\tCopied {sortedFiles[date].Count} files for {GetDayName(date)}");
			}

			Console.WriteLine("Operation completed.");
		}

		private static void ValidateFolders(string v1, string v2)
		{
			string[] aFiles = Directory.GetFiles(v1, "*", SearchOption.AllDirectories);
			string[] bFiles = Directory.GetFiles(v2, "*", SearchOption.AllDirectories);

			HashSet<string> aSet = new HashSet<string>(aFiles.Select(f => Path.GetFileName(f)));
			HashSet<string> bSet = new HashSet<string>(bFiles.Select(f => Path.GetFileName(f)));

			var except = aSet.Except(bSet);

			if (!except.Any())
			{
				Console.WriteLine("Validated.");
				Console.ReadKey(intercept: true);
			}
			else
			{
				Console.WriteLine("Not valid. Some files were not successfully copied.");
				string tempPath = @"C:\Users\Chris\Documents\Files\Unclassified Files\SortFilesByDate Validation\";
				Directory.CreateDirectory(tempPath);

				foreach (var intersect in except)
				{
					Console.WriteLine($"\t{intersect}");
					File.Copy(aFiles.First(f => f.Contains(intersect)), Path.Combine(tempPath, intersect), false);
				}

				Console.ReadKey(intercept: true);
			}
		}

		private static string GetDayName(DateTime date)
		{
			string monthName = monthNames[date.Month - 1];
			return $"{monthName} {date.Day}";
		}
	}
}
