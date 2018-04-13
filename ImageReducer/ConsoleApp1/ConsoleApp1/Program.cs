using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageReducer
{
	class Program
	{
		static void Main(string[] args)
		{
		}

		static void PrintUsage()
		{
			Console.WriteLine("ImageReducer");
			Console.WriteLine("\tResizes all images in a folder that are over a certain width");
			Console.WriteLine();
			Console.WriteLine("Note: If an image is substantially wide, it is considered panoramic");
			Console.WriteLine("Panoramic images are resized to be at a certain height instead of width");
			Console.WriteLine();
			Console.WriteLine("Usage:");
			// ImageReducer /[i|r] \"path\\to\\folder\" /w:[width] /[t|r]
			// \t/i: Inspection mode - loads the dimensions of all images and displays info about
			// \t\tthe pictures that are over the size limit
			// \t/r: Reduction mode - Reduces pictures to the width specified in [width]
			// [width]: The desired maximum width (or height for panoramic images) that all images
			// \t\tshould be reduced to
			// \t/t: Top-level only: Only loads pictures in that folder and not in any subfolders
			// \t/r: Recursive: Loads all pictures in all subfolders

			// args[0]: /[i|r]
			// args[1]: "path\to\folder\"
			// args[2]: /w:[width]
			// args[3]: /[t|r]
		}
	}
}
