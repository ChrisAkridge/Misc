var move = args[0].Equals("-m", StringComparison.InvariantCultureIgnoreCase);
var correctArgumentCount = (move && args.Length == 3) || args.Length == 2;
var delete = args[0].Equals("-d", StringComparison.InvariantCultureIgnoreCase);
var fileListExists = File.Exists(args[1]);
var outputDirectoryPathIsValid = delete || Uri.TryCreate(args[2], UriKind.Absolute, out _);

if (!correctArgumentCount
    || !(move || delete)
    || !fileListExists
    || !(delete || outputDirectoryPathIsValid))
{
    Console.WriteLine("Usage: [-m|-d] <file list> <output directory>");
    Console.WriteLine("  -m: Move files to output directory");
    Console.WriteLine("  -d: Delete files in list");

    return;
}

if (move)
{
    if (!Directory.Exists(args[2]))
    {
        Directory.CreateDirectory(args[2]);
    }
    
    var fileList = File.ReadAllLines(args[1]);
    var digitsInOutputFileNames = (int)Math.Ceiling(Math.Log10(fileList.Length));
    int currentFileIndex = 0;
    
    foreach (var filePath in fileList)
    {
        Console.WriteLine($"Moving {filePath}...");
        
        var fileExtension = Path.GetExtension(filePath);
        var outputFileName = $"{currentFileIndex.ToString().PadLeft(digitsInOutputFileNames, '0')}{fileExtension}";
        var outputFilePath = Path.Combine(args[2], outputFileName);
        File.Move(filePath, outputFilePath);

        currentFileIndex += 1;
    }
}
else
{
    Console.WriteLine("Warning! Will delete files in the file list! Is this okay? [Y/n]");
    var key = Console.ReadKey();
    
    if (key.Key != ConsoleKey.Y) { return; }
    
    var fileList = File.ReadAllLines(args[1]);

    foreach (var filePath in fileList)
    {
        Console.WriteLine($"Deleting {filePath}!");
        File.Delete(filePath);
    }
}