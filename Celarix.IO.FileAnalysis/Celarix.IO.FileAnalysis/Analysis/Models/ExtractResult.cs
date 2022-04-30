namespace Celarix.IO.FileAnalysis.Analysis.Models
{
    public sealed class ExtractResult
    {
        public bool FileWasArchive { get; }
        public int ExtractedFileCount { get; }

        public ExtractResult(bool fileWasArchive, int extractedFileCount)
        {
            FileWasArchive = fileWasArchive;
            ExtractedFileCount = extractedFileCount;
        }
    }
}
