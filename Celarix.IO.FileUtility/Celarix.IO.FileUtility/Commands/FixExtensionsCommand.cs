using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace Celarix.IO.FileUtility.Commands
{
	[Verb("FixExtensions", HelpText = "Fixes the extensions of files with the UNKNOWN extension in a folder.")]
	internal sealed class FixExtensionsCommand
	{
		[Option('i', "input", Required = true, HelpText = "The path to the folder containing the files to fix.")]
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
