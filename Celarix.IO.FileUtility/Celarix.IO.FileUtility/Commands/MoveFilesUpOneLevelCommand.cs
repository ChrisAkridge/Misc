using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace Celarix.IO.FileUtility.Commands
{
	[Verb("MoveFilesUpOneLevel", HelpText = "Moves all folders and files within a folder up by one level. All files get an incrementing integer prepended to them to preserve overall order.")]
	internal sealed class MoveFilesUpOneLevelCommand
	{
		[Option('i', "input", Required = true, HelpText = "The path to the folder containing the files to move up.")]
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
