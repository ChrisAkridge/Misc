using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FileTransform.cs
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length != 2)
			{
				Console.WriteLine("FileTransform\r\nUsage: FileTransform path op\r\nAvailable operations: inv, bwnot, fact, not, add, sub, mult, div, mod, eq, ineq, lt, lteq, gt, gteq, and, or, bwand, bwor, bwxor, lshift, rshift");
				Console.ReadKey();
			}

			string filePath = args[0];
			string operation = args[1];

			switch (operation.ToLowerInvariant())
			{
				case "inv":
					Inverse(filePath);
					break;
				case "bwnot":
					BitwiseNot(filePath);
					break;
				case "fact":
					Factorial(filePath);
					break;
				case "not":
					LogicalNot(filePath);
					break;
				case "add":
					Add(filePath);
					break;
				case "sub":
					Subtract(filePath);
					break;
				default:
					break;
			}
		}

		private static string GenerateNewFilename(string filePath, string operation)
		{
			return Path.Combine(new FileInfo(filePath).Directory.FullName, string.Concat(Path.GetFileNameWithoutExtension(filePath), "_", operation, Path.GetExtension(filePath)));
		}

		private static void Inverse(string filePath)
		{
			byte[] input = File.ReadAllBytes(filePath);
			string newFileName = GenerateNewFilename(filePath, "inv");
			byte[] result = Transforms.Inverse(input).ToArray();
			File.WriteAllBytes(newFileName, result);

			Console.WriteLine("Operation succeeded.");
			Console.ReadKey();
		}

		private static void BitwiseNot(string filePath)
		{
			byte[] input = File.ReadAllBytes(filePath);
			string newFileName = GenerateNewFilename(filePath, "bwnot");
			byte[] result = Transforms.BitwiseNot(input).ToArray();
			File.WriteAllBytes(newFileName, result);

			Console.WriteLine("Operation succeeded.");
			Console.ReadKey();
		}

		private static void Factorial(string filePath)
		{
			byte[] input = File.ReadAllBytes(filePath);
			string newFileName = GenerateNewFilename(filePath, "fact");
			byte[] result = Transforms.Factorial(input).ToArray();
			File.WriteAllBytes(newFileName, result);

			Console.WriteLine("Operation succeeded.");
			Console.ReadKey();
		}

		private static void LogicalNot(string filePath)
		{
			byte[] input = File.ReadAllBytes(filePath);
			string newFileName = GenerateNewFilename(filePath, "not");
			byte[] result = Transforms.LogicalNot(input).ToArray();
			File.WriteAllBytes(newFileName, result);

			Console.WriteLine("Operation succeeded.");
			Console.ReadKey();
		}

		private static void Add(string filePath)
		{
			byte[] input = File.ReadAllBytes(filePath);
			string newFileName = GenerateNewFilename(filePath, "add");
			byte[] result = Transforms.Add(input).ToArray();
			File.WriteAllBytes(newFileName, result);

			Console.WriteLine("Operation succeeded.");
			Console.ReadKey();
		}

		private static void Subtract(string filePath)
		{
			byte[] input = File.ReadAllBytes(filePath);
			string newFileName = GenerateNewFilename(filePath, "sub");
			byte[] result = Transforms.Subtract(input).ToArray();
			File.WriteAllBytes(newFileName, result);

			Console.WriteLine("Operation succeeded.");
			Console.ReadKey();
		}
	}
}
