using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PictureTilerCLI.Packer
{
    internal static class PicturePacker
    {
        internal static void Pack(PackOptions options)
        {
            var files = GetFiles(options.InputFolderPath, options.Recursive);
            var filesAndSizes = new Dictionary<string, Size>();

            for (int i = 0; i < files.Count; i++)
            {
                if (i > 0 && i % 100 == 0)
                {
                    Console.WriteLine($"Loaded sizes for {i} files");
                }
                
                filesAndSizes.Add(files[i], GetImageSize(files[i]));
            }

            var blocks = filesAndSizes.OrderByDescending(kvp => kvp.Value.Width)
                .Select(kvp => new Block
                {
                    ImageFilePath = kvp.Key,
                    Size = kvp.Value
                })
                .ToList();
            
            var packer = new Packer();
            packer.Fit(blocks);
            DrawImage(blocks, packer.Root.Size, options.OutputFilePath);
        }

        private static IList<string> GetFiles(string inputFolderPath, bool recursive)
        {
            if (!Directory.Exists(inputFolderPath))
            {
                throw new DirectoryNotFoundException($"Invalid path {inputFolderPath}: path does not exist");
            }

            Console.WriteLine("Loading all files in input folder...");
            var files = Directory.GetFiles(inputFolderPath, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            Console.WriteLine($"Found {files.Length} total files...");

            Console.WriteLine("Selecting pictures...");
            var filteredFiles = files.Where(f => f.EndsWith("gif", StringComparison.InvariantCultureIgnoreCase)
                || f.EndsWith("jpg", StringComparison.InvariantCultureIgnoreCase)
                || f.EndsWith("jpeg", StringComparison.InvariantCultureIgnoreCase)
                || f.EndsWith("png", StringComparison.InvariantCultureIgnoreCase));

            return filteredFiles.ToList();
        }

        private static void DrawImage(IList<Block> blocks, Size rootSize, string outputFilePath)
        {
            var canvas = new Image<Rgba32>(rootSize.Width, rootSize.Height, Rgba32.ParseHex("ffffffff"));

            foreach (var block in blocks)
            {
                using (var image = Image.Load(block.ImageFilePath))
                {
                    canvas.Mutate(c => c.DrawImage(image, block.Fit.Location, 1f));
                }
            }

            canvas.SaveAsPng(outputFilePath);
        }

        private static Size GetImageSize(string imageFilePath)
        {
            using (var image = Image.Load(imageFilePath))
            {
                return image.Size();
            }
        }
    }
}
