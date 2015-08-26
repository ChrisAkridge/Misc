using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTools
{
	internal static class CSharpWriter
	{
		public static string WriteBytesAsBools(byte[] data, string name)
		{
			StringBuilder result = new StringBuilder();

			result.AppendLine("using System;");
			result.AppendLine();
			result.AppendLine($"namespace FileTools.{name}");
			result.AppendLine("{");
			result.AppendLine($"\tpublic static class {name}");
			result.AppendLine("\t{");
			result.AppendLine("\t\tpublic static readonly bool[] file = new bool[] {");

			return null;
		}

		private static string WriteBools(byte data)
		{
			return string.Join(", ", AsBits(data).Select(b => (b) ? "true" : "false").ToArray());
		}

		private static IEnumerable<bool> AsBits(byte data)
		{
			for (int i = 0; i < 7; i++)
			{
				yield return ((data & (1 << i)) != 0) ? true : false;
			}

			yield break;
		}
	}
}
