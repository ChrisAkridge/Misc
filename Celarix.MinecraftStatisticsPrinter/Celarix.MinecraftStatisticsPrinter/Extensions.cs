using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.MinecraftStatisticsPrinter
{
    internal static class Extensions
    {
        private static readonly string[] PowersOfThousands =
        {
            "trillion",
            "quadrillion",
            "quintillion",
            "sextillion",
            "octillion",
            "nonillion",
            "decillion",
            "undecillion",
            "duodecillion"
        };

        private static readonly BigInteger OneTrillion = BigInteger.Parse("1000000000000");
        
        private static string PrettifyNumber(BigInteger number)
        {
            if (number <= OneTrillion)
            {
                return number.ToString("#,###");
            }

            var integerMantissaPart = number / OneTrillion;
            var decimalMantissaPart = (number * 1000) / OneTrillion;
            var powerOfThousandIndex = 0;

            while (integerMantissaPart > 1000)
            {
                integerMantissaPart /= 1000;
                decimalMantissaPart /= 1000;
                powerOfThousandIndex++;
            }

            decimalMantissaPart %= 1000;
            return $"{integerMantissaPart}.{decimalMantissaPart} {PowersOfThousands[powerOfThousandIndex]}";
        }

        public static string PrintNumber(this BigInteger number) =>
            number <= OneTrillion
                ? number.ToString("#,###")
                : $"{number:#,###} ({PrettifyNumber(number)})";

        public static BigInteger CeilingDivide(this BigInteger a, BigInteger b) =>
            a % b == 0
                ? a / b
                : (a / b) + 1;

        // https://stackoverflow.com/a/6084813
        public static BigInteger Sqrt(this BigInteger n)
        {
            if (n == 0)
            {
                return 0;
            }
            
            if (n > 0)
            {
                int bitLength = Convert.ToInt32(Math.Ceiling(BigInteger.Log(n, 2)));
                var root = BigInteger.One << (bitLength / 2);

                while (!IsSqrt(n, root))
                {
                    root += n / root;
                    root >>= 1;
                }

                return root;
            }

            throw new ArithmeticException("NaN");
        }

        private static bool IsSqrt(BigInteger n, BigInteger root)
        {
            var lowerBound = root * root;
            var upperBound = (root + 1) * (root + 1);

            return n >= lowerBound && n < upperBound;
        }

        public static BigInteger Cbrt(this BigInteger n)
        {
            if (n == 0)
            {
                return 0;
            }

            if (n > 0)
            {
                int tritLength = Convert.ToInt32(Math.Ceiling(BigInteger.Log(n, 3)));
                var root = BigInteger.Pow(3, tritLength / 3);

                while (!IsCbrt(n, root))
                {
                    root += n / root;
                    root /= 3;
                }

                return root;
            }

            throw new ArithmeticException("NaN");
        }

        private static bool IsCbrt(BigInteger n, BigInteger root)
        {
            var lowerBound = root * root * root;
            var upperBound = (root + 1) * (root + 1) * (root + 1);

            return n >= lowerBound && n <= upperBound;
        }
    }
}
