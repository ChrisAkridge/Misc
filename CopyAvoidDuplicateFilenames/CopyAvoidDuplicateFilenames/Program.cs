using System;
using System.IO;

namespace CopyAvoidDuplicateFilenames
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2 || args.Length > 3)
            {
                Usage();
                return;
            }

            string inputFolderPath = args[0];
            string outputFolderPath = args[1];
            string extension = (args.Length == 3) ? args[2] : null;

            string[] filesToCopy = (extension != null)
                ? Directory.GetFiles(inputFolderPath, "*." + extension, SearchOption.AllDirectories) 
                : Directory.GetFiles(inputFolderPath, "*", SearchOption.AllDirectories);

            Console.WriteLine($"Found {filesToCopy.Length} files to copy");

            for (int i = 0; i < filesToCopy.Length; i++)
            {
                Console.WriteLine($"Copying {filesToCopy[i]}");
                string fileExtension = Path.GetExtension(filesToCopy[i]);
                string newFileName = $"{i:D5}{fileExtension}";
                string newFilePath = Path.Combine(outputFolderPath, newFileName);
                File.Copy(filesToCopy[i], newFilePath);
            }
        }

        private static void Usage()
        {
            Console.WriteLine("CopyAvoidDuplicateFilenames \"//path//to//input\" \"//path//to/output//\" ext");
            Console.WriteLine("\t\"//path//to//input\": Path to the folder to copy files from");
            Console.WriteLine("\t\"//path//to/output//\": Path to the folder to copy files to");
            Console.WriteLine("\text: Extension of the files to copy. Leave out to copy all files.");
            Console.WriteLine("Note: copies files from folder and all subfolders");
        }
    }
}
