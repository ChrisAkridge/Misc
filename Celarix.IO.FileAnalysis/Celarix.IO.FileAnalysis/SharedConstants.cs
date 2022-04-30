using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis
{
    internal static class SharedConstants
    {
        public const int DefaultBufferCapacity = 10000;
        public const string HashFilePath = "job\\deduplicate\\hashes.txt";
        public const string ListFileFolderPath = "job\\copy\\";
        public const string OutputFileFolderPath = "files";
        public const string PathFileFolderPath = "job\\convert\\";
        public const string ImagePathFileFolderPath = "job\\imagePaths";
        public const string ImagePathsFilePath = "job\\imagePaths.txt";
        public const string TextFileFolderPath = "pages\\";
        public const string ExtractedFileFolderSuffix = "_ext";
    }
}
