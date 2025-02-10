using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace Celarix.IO.FileUtility.Commands
{
	[Verb("SortIntoFoldersByExtension", HelpText = "Sorts files in a folder into subfolders based on their extensions.")]
	internal sealed class SortIntoFoldersByExtensionCommand
	{
		[Option('i', "input", Required = true, HelpText = "The path to the folder containing the files to sort.")]
		public string? FolderPath { get; set; }

		public bool Validate()
		{
			if (!Directory.Exists(FolderPath))
			{
				Console.WriteLine($"The folder path '{FolderPath}' does not exist.");

				return false;
			}

			return true;
		}
	}
}
