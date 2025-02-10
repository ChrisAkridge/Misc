namespace Celarix.SimpleIncrementalBackup.Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            License();
            if (args.Length < 2
                || args.Length > 4
                || !Directory.Exists(args[0])
                || !Directory.Exists(args[1])
                || (args.Length == 3 && !args[2].Equals("-d", StringComparison.InvariantCultureIgnoreCase))
                || (args.Length == 4
	                && !args[2].Equals("-d", StringComparison.InvariantCultureIgnoreCase)
	                && !args[3].Equals("-f", StringComparison.InvariantCultureIgnoreCase)))
            {
                Usage();
                return;
            }

            var sourceFolder = args[0];
            var backupFolder = args[1];
            var deleteImmediately = args.Length is 3 or 4;
            var deleteFirst = args.Length == 4;

            var agent = new BackupAgent(sourceFolder, backupFolder, deleteImmediately, deleteFirst);
            agent.RunBackup();
        }

        private static void License()
        {
            var mitLicense = $"MIT License\r\n\r\nCopyright (c) {DateTimeOffset.Now.Year} Chris Akridge\r\n\r\nPermission is hereby granted, free of charge, to any person obtaining a copy\r\nof this software and associated documentation files (the \"Software\"), to deal\r\nin the Software without restriction, including without limitation the rights\r\nto use, copy, modify, merge, publish, distribute, sublicense, and/or sell\r\ncopies of the Software, and to permit persons to whom the Software is\r\nfurnished to do so, subject to the following conditions:\r\n\r\nThe above copyright notice and this permission notice shall be included in all\r\ncopies or substantial portions of the Software.\r\n\r\nTHE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR\r\nIMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,\r\nFITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE\r\nAUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER\r\nLIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,\r\nOUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE\r\nSOFTWARE.";
            System.Console.WriteLine(mitLicense);
            System.Console.WriteLine();
            System.Console.WriteLine("WARNING! This application is not meant as a replacement for proper backup software!");
            System.Console.WriteLine("This application is written mostly for my personal use! Per the MIT license, this software is provided AS IS!");
            System.Console.WriteLine("I am not liable for any data loss or corruption caused by this software!");
            System.Console.WriteLine("Use this software at your own risk!");
        }

        private static void Usage()
        {
            // cool programmer hax because I named the project Console and that clashes with the
            // Console class
            Action<string> w = System.Console.WriteLine;
            w("");
            w("Celarix.SimpleIncrementalBackup.Console: A command-line utility that performs simple incremental backups");
            w("of a specified folder to a specified backup folder. Files deleted from the source folder are retained in");
            w("the backup folder once, until the next backup, unless a command-line option is specified to immediately");
            w("delete them.");
            w("");
            w("This application is intended for folders where new files are quite common and existing files aren't modified");
            w("too often. This is not recommended for a full-system backup.");
            w("");
            w("Usage: Celarix.SimpleIncrementalBackup.Console.exe <source folder> <backup folder> [-d]");
            w("\t<source folder>: The folder to back up. Can be the root of a drive (i.e. \"C:\"");
            w("\t<backup folder>: The folder to back up to. Can be the root of a drive (i.e. \"D:\"");
            w("\t-d: Immediately delete files from the backup folder that have been deleted from the source folder.");
            w("\t-f: Delete files first before running the backup. Dangerous, but useful if a very large number of files have been moved that would be too large to fit twice in the destination.");
        }
    }
}
