using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace Celarix.IO.FileUtility.Commands
{
	[Verb("SequentialRename", HelpText = "Renames all files in a folder by an incrementing integer, preserving existing order.")]
	internal sealed class SequentialRenameCommand
	{
		[Option('i', "input", Required = true, HelpText = "The path to the folder containing the files to rename.")]
		public string? FolderPath { get; set; }
		
		[Option('p', "prefix", Required = false, HelpText = "A string to prefix before every number. Must be made only of valid filename characters. Defaults to no prefix.")]
		public string? PrefixText { get; set; }
		
		[Option('d', "digits", Required = false, HelpText = "The number of digits to use in the new file names. Will be rounded up if there are more files than filenames.")]
		public string? DigitCountText { get; set; }
		
		[Option('s', "start", Required = false, HelpText = "The number to start the renaming at.")]
		public string? StartNumberText { get; set; }
		
		public int DigitCount => int.TryParse(DigitCountText, out var result) ? result : 4;
		public int StartNumber => int.TryParse(StartNumberText, out var result) ? result : 0;
		public string Prefix => PrefixText ?? string.Empty;

		public bool Validate()
		{
			if (!Directory.Exists(FolderPath))
			{
				Console.WriteLine($"The folder path '{FolderPath}' does not exist.");
				return false;
			}

			if (DigitCount < 1)
			{
				Console.WriteLine("The digit count must be a positive integer.");

				return false;
			}

			if (StartNumber < 0)
			{
				Console.WriteLine("The start number must be a non-negative integer.");

				return false;
			}

			if (Prefix.Any(IsInvalidFileNameCharacter))
			{
				Console.WriteLine("The prefix contains invalid characters.");

				return false;
			}

			return true;
		}
		
		private static bool IsInvalidFileNameCharacter(char c) =>
			c is '\\' or '/' or ':' or '*' or '?' or '"' or '<' or '>' or '|';
	}
}
