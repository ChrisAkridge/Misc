using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleArc
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("SimpleArc - A minimal instruction set virtual processor");
                Console.WriteLine("Copyright (c) 2014 Chris Akridge. Licensed under the MIT license.");
                Console.WriteLine();
                Console.WriteLine("Arguments: MEM_SIZE PROGRAM_PATH");
                Console.WriteLine("\tMEM_SIZE: The size in bytes of system memory.");
                Console.WriteLine("\tPROGRAM_PATH: The path to the program file to execute.");
                Console.ReadKey();
                return;
            }

            int memorySize = int.Parse(args[0]);
            string programPath = args[1];

            if (!File.Exists(programPath))
            {
                Console.WriteLine("The file at {0} could not be found.", programPath);
                Console.ReadKey();
                return;
            }
        }
    }
}
