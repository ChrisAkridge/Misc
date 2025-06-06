﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace Celarix.IO.FileUtility.Commands
{
	[Verb("SortFilesIntoSeries", HelpText = "Sorts top-level files into series based on their filenames.")]
	internal sealed class SortFilesIntoSeriesCommand
	{
		[Option('i', "input", Required = true, HelpText = "The path to the folder containing the files to sort into series.")]
		public string? FolderPath { get; set; }
		
		[Option('t', "type", Required = true, HelpText = "Whether to sort into picture-like series or screenshot-like series.")]
		public string? SeriesTypeText { get; set; }
		
		[Option('s', "startfrom", Required = false, HelpText = "The series-qualified number to start the renaming at.")]
		public string? StartFromText { get; set; }

		public int StartFrom =>
			Utilities.GetIndexFromSeriesFileName(
				StartFromText ?? throw new ArgumentNullException(nameof(StartFromText)), SeriesType);

		public SeriesType SeriesType =>
			Enum.TryParse(typeof(SeriesType),
				SeriesTypeText ?? throw new ArgumentNullException(nameof(SeriesTypeText)), true, out var result)
				? (SeriesType)result
				: SeriesType.Invalid;

		public bool Validate()
		{
			if (!Directory.Exists(FolderPath))
			{
				Console.WriteLine($"The folder path '{FolderPath}' does not exist.");

				return false;
			}

			if (SeriesType == SeriesType.Invalid)
			{
				Console.WriteLine("The series type must be either 'Picture' or 'Screenshot'.");

				return false;
			}

			if (StartFrom == -1)
			{
				Console.WriteLine("The starting filename is invalid.");

				return false;
			}

			return true;
		}
	}
}
