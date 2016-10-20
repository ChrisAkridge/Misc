using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace PictureTilerCLI
{
	class Program
	{
		static void Main(string[] args)
		{
			var options = new Options();
			if (Parser.Default.ParseArguments(args, options))
			{
				// do work
				Tiler.Tile(options);
			}
			else
			{
				Console.WriteLine(options.GetUsage());
#if DEBUG
				Console.ReadKey(intercept: true);
#endif
			}
		}
	}

	internal class Options
	{
		[Option('i', "input", Required = true, HelpText = "Path to the input folder.")]
		public string InputFolderPath { get; set; }

		[Option('o', "output", Required = true, HelpText = "Path to the output file. Extensions determines type; valid extensions are gif, jpg, jpeg, and png.")]
		public string OutputFilePath { get; set; }

		[Option('w', "width", Required = true, HelpText = "Width of each picture in pixels.")]
		public int Width { get; set; }

		[Option('h', "height", Required = true, HelpText = "Height of each picture in pixels.")]
		public int Height { get; set; }

		[Option('s', "sortby", Required = false, HelpText = "How the picture are sorted on the canvas (name, date_created, size). Defaults to name.")]
		private string OrderByInput { get; set; } = "name";

		public OutputFileTypes OutputFileType
		{
			get
			{
				string ext = Path.GetExtension(OutputFilePath).ToLowerInvariant();
				if (ext.EndsWith("gif")) { return OutputFileTypes.Gif; }
				else if (ext.EndsWith("jpg") || ext.EndsWith("jpeg")) { return OutputFileTypes.Jpeg; }
				else if (ext.EndsWith("png")) { return OutputFileTypes.Png; }
				return OutputFileTypes.Default;
			}
		}

		public OrderByTypes OrderBy
		{
			get
			{
				if (OrderByInput.Equals("name", StringComparison.InvariantCultureIgnoreCase)) { return OrderByTypes.Name; }
				else if (OrderByInput.Equals("date_created", StringComparison.InvariantCultureIgnoreCase)) { return OrderByTypes.DateCreated; }
				else if (OrderByInput.Equals("size", StringComparison.InvariantCultureIgnoreCase)) { return OrderByTypes.Size; }
				return OrderByTypes.Default;
			}
		}

		[HelpOption]
		public string GetUsage()
		{
			return HelpText.AutoBuild(this);
		}
	}

	internal enum OutputFileTypes
	{
		Default,
		Gif,
		Jpeg,
		Png
	}

	internal enum OrderByTypes
	{
		Default,
		Name,
		DateCreated,
		Size
	}
}
