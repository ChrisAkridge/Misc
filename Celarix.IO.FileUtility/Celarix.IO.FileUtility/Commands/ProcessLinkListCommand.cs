using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace Celarix.IO.FileUtility.Commands
{
	[Verb("ProcessLinkList", HelpText = "Processes a list of links, often lists containing many i.redd.it URLs, v.redd.it URLs, and others.")]
	internal sealed class ProcessLinkListCommand
	{
		[Option('i', "input", Required = true, HelpText = "The path to the file containing the list of links.")]
		public string? LinkListPath { get; set; }

		public bool Validate()
		{
			if (!File.Exists(LinkListPath))
			{
				Console.WriteLine($"The link list file '{LinkListPath}' does not exist.");

				return false;
			}

			return true;
		}
	}
}
