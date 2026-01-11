using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using Directory = System.IO.Directory;

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

            string today = DateTime.Today.ToString("yyyyMMdd");
            string inputFileLogPath = $@"E:\Documents\Files\Unclassified Files\SortFilesByDate\{today}_in.txt";
            string outputFileLogPath = $@"E:\Documents\Files\Unclassified Files\SortFilesByDate\{today}_out.txt";

            var inputWriter = new StreamWriter(File.OpenWrite(inputFileLogPath));
            var outputWriter = new StreamWriter(File.OpenWrite(outputFileLogPath));

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
                inputWriter.WriteLine($"Found {filePath}");
			}

			Console.WriteLine("\tLoaded {0} files.", filesInfo.Count);
            inputWriter.WriteLine();

			// Sort the files by date taken.
			Console.WriteLine("Sorting by date taken...");

			Dictionary<DateTime, List<string>> sortedFiles = new Dictionary<DateTime, List<string>>();

			foreach (FileInfo fileInfo in filesInfo.Values)
            {
                DateTime dateTaken;
				
                try
                {
                    // https://stackoverflow.com/a/39839380/2709212
                    var metadataDirectories = ImageMetadataReader.ReadMetadata(fileInfo.FullName);
                    var subIfdDirectory = metadataDirectories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                    dateTaken = subIfdDirectory?.GetDateTime(ExifDirectoryBase.TagDateTimeOriginal)
                        ?? fileInfo.CreationTime;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning! File at {fileInfo.FullName} could not have its \"date taken\" read!");
                    Console.WriteLine(ex);
                    dateTaken = fileInfo.CreationTime;
                }

                if (!sortedFiles.ContainsKey(dateTaken))
				{
					sortedFiles.Add(dateTaken, new List<string>());
				}

				sortedFiles[dateTaken].Add(fileInfo.FullName);
                inputWriter.WriteLine($"Added {fileInfo.FullName} to {dateTaken.ToShortDateString()}");
			}

			 Console.WriteLine("\tSorted into {0} days", sortedFiles.Count);

			// Copy the files
			Console.WriteLine("Copying files...");

            int i = 0;
            foreach (var kvp in sortedFiles) { i += kvp.Value.Count; }

			string tempFolderName = Path.Combine(parentFolder, $"{folderName}_tmp");
			Directory.CreateDirectory(tempFolderName);

            int copied = 0;

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
                        copied++;
                        outputWriter.WriteLine($"Copied {file} to {newPath}");
					}
					catch (IOException exception)
					{
						if (exception.Message.Contains("already exists"))
						{
                            Console.WriteLine($"File {file} already exists.");
                            continue;
						}
						else
						{
							Console.WriteLine($"\tException: {exception.Message}");
                            outputWriter.WriteLine($"Exception: {exception.Message}");

                            inputWriter.Close();
                            outputWriter.Close();

                            inputWriter.Dispose();
                            outputWriter.Dispose();

                            Environment.Exit(1);
						}
					}
				}

				Console.WriteLine($"\tCopied {sortedFiles[date].Count} files for {GetDayName(date)}");
			}

			Console.WriteLine("Operation completed.");
            Console.WriteLine($"Copied {copied} files.");

            inputWriter.Close();
            outputWriter.Close();

            inputWriter.Dispose();
            outputWriter.Dispose();
		}

		private static void ValidateFolders(string v1, string v2)
		{
			string[] aFiles = Directory.GetFiles(v1, "*", SearchOption.AllDirectories);
			string[] bFiles = Directory.GetFiles(v2, "*", SearchOption.AllDirectories);

            IEnumerable<FileInfo> aInfos = aFiles.Select(f => new FileInfo(f));
            IEnumerable<FileInfo> bInfos = bFiles.Select(f => new FileInfo(f));

            var aGroups = aInfos.GroupBy(f => f.Length);
            var bGroups = bInfos.GroupBy(f => f.Length);

            var aFlattened = aGroups.Select(f => f.First());
            var bFlattened = bGroups.Select(f => f.First());

			var except = aFlattened.Except(bFlattened);

			if (!except.Any())
			{
				Console.WriteLine("Validated.");
				Console.ReadKey(intercept: true);
			}
			else
			{
				Console.WriteLine("Not valid. Some files were not successfully copied.");
				string tempPath = @"G:\Documents\Files\Unclassified Files\SortFilesByDate Validation\";
				Directory.CreateDirectory(tempPath);

				foreach (var intersect in except)
				{
					Console.WriteLine($"\t{intersect}");
                    try
                    {
                        File.Copy(aFiles.First(f => f.Contains(intersect.Name)), Path.Combine(tempPath, intersect.Name), false);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("already exists")) { continue; }
                        else
                        {
                            Console.WriteLine(ex.Message);
                            break;
                        }
                    }
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
