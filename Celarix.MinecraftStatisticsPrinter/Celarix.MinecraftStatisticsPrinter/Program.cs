using System.Numerics;
using System.Text;
using Celarix.MinecraftStatisticsPrinter.Blocks;

namespace Celarix.MinecraftStatisticsPrinter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var builder = new StringBuilder();
            var stone = new Stone();
            var count = BigInteger.Parse("1382400000000000000");
            stone.PrintBasicData(builder, count, 0);
            stone.Print(builder, count, 0);
            Console.WriteLine(builder.ToString());
        }
    }
}