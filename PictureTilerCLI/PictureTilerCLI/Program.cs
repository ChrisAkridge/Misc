using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using PictureTilerCLI.Packer;

namespace PictureTilerCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments(args, typeof(TileOptions), typeof(PackOptions))
                .WithParsed<TileOptions>(opt => Tiler.Tile(opt))
                .WithParsed<PackOptions>(opt => PicturePacker.Pack(opt))
                .WithNotParsed(errors =>
                {
                    Console.WriteLine("Errors encountered when parsing arguments:");

                    foreach (var error in errors) { Console.WriteLine("\t" + error.Tag); }
                });
        }
    }

    [Verb("Tile", HelpText = "Tiles pictures into a canvas, cropping them to the specified width and height.")]
    internal class TileOptions
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
                if (ext.EndsWith("gif", StringComparison.Ordinal)) { return OutputFileTypes.Gif; }
                else if (ext.EndsWith("jpg", StringComparison.Ordinal) || ext.EndsWith("jpeg", StringComparison.Ordinal)) { return OutputFileTypes.Jpeg; }
                else if (ext.EndsWith("png", StringComparison.Ordinal)) { return OutputFileTypes.Png; }
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
    }

    [Verb("Pack", false, HelpText = "Packs pictures without cropping onto an expanding canvas with an optionally specified aspect ratio.")]
    internal class PackOptions
    {
        [Option('i', "input", Required = true, HelpText = "Path to the folder of pictures to pack.")]
        public string InputFolderPath { get; set; }
        
        [Option('o', "output", Required = true, HelpText = "Path to the image to pack the input pictures into.")]
        public string OutputFilePath { get; set; }
        
        [Option('w', "width", Required = false, HelpText = "The desired final aspect ratio width (i.e. 16:9 has a height of 16).")]
        public int AspectRatioWidth { get; set; }
        
        [Option('h', "height", Required = false, HelpText = "The desired final aspect ratio height (i.e. 16:9 has a height of 9).")]
        public int AspectRatioHeight { get; set; }
        
        [Option('r', "recursive", Required = false, HelpText = "Load pictures recursively.")]
        public bool Recursive { get; set; }
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
