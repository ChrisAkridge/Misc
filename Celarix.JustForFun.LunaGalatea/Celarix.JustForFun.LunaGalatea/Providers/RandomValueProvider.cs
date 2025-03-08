using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Unicode;

namespace Celarix.JustForFun.LunaGalatea.Providers
{
    public class RandomValueProvider : IProvider<string>
    {
        private readonly Random random;
        private readonly byte[] buffer;

        public bool UseMonospaceFont => false;

        public RandomValueProvider()
        {
            random = new Random();
            buffer = new byte[16];
        }

        public string GetDisplayObject()
        {
            var randomValues = new List<string>();

            for (int i = 1; i <= 20; i++)
            {
                var randomNumber = RandomNumberOfNDigits(i);
                if (randomNumber.All(c => c == '0') && i >= 3)
                {
                    return $"(Special!) Random number less than 10^{i}: {new string(randomNumber)}";
                }
                randomValues.Add($"Random number less than 10^{i}: {new string(randomNumber)}");
            }
            
            randomValues.Add($"Random Boolean: {random.NextDouble() < 0.5d}");
            randomValues.Add($"Random Byte: {random.Next(0, 256)}");
            randomValues.Add($"Random SByte: {random.Next(-128, 128)}");
            randomValues.Add($"Random Int16: {random.Next(-32768, 32768)}");
            randomValues.Add($"Random UInt16: {random.Next(0, 65536)}");
            randomValues.Add($"Random Int32: {RandomInt32()}");
            randomValues.Add($"Random UInt32: {RandomUInt32()}");
            randomValues.Add($"Random Int64: {RandomInt64()}");
            randomValues.Add($"Random UInt64: {RandomUInt64()}");
            randomValues.Add($"Random Int128: {RandomInt128()}");
            randomValues.Add($"Random UInt128: {RandomUInt128()}");
            randomValues.Add($"Random Single: {RandomSingle()}");
            randomValues.Add($"Random Double: {RandomDouble()}");
            randomValues.Add($"Random Decimal: {RandomDecimal()}");
            randomValues.Add($"Random DateTimeOffset: {RandomDateTimeOffset()}");
            randomValues.Add($"Random Code Point: {RandomCodePoint()}");
            randomValues.Add($"Random String: {RandomString()}");

            return randomValues[random.Next(0, randomValues.Count)];
        }

        private char[] RandomNumberOfNDigits(int n)
        {
            if (n <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(n));
            }
            
            var digits = new char[n];
            for (var i = 0; i < n; i++)
            {
                digits[i] = RandomDigitChar();
            }
            
            return digits;
        }

        private char RandomDigitChar()
        {
            var digit = random.Next(0, 10);
            return (char)(digit + 0x30);
        }

        private int RandomInt32() => unchecked((int)RandomUInt32());

        private uint RandomUInt32()
        {
            random.NextBytes(buffer);
            return BitConverter.ToUInt32(buffer, 0);
        }

        private long RandomInt64() => unchecked((long)RandomUInt64());

        // https://stackoverflow.com/a/13095144
        private long RandomInt64(long min, long max)
        {
            //Working with ulong so that modulo works correctly with values > long.MaxValue
            ulong uRange = (ulong)(max - min);

            //Prevent a modolo bias; see https://stackoverflow.com/a/10984975/238419
            //for more information.
            //In the worst case, the expected number of calls is 2 (though usually it's
            //much closer to 1) so this loop doesn't really hurt performance at all.
            ulong ulongRand;
            do
            {
                random.NextBytes(buffer);
                ulongRand = (ulong)BitConverter.ToInt64(buffer, 0);
            } while (ulongRand > ulong.MaxValue - (((ulong.MaxValue % uRange) + 1) % uRange));

            return (long)(ulongRand % uRange) + min;
        }
        
        private ulong RandomUInt64()
        {
            random.NextBytes(buffer);
            return BitConverter.ToUInt64(buffer, 0);
        }

        private BigInteger RandomInt128()
        {
            random.NextBytes(buffer);
            return new BigInteger(buffer);
        }

        private BigInteger RandomUInt128()
        {
            random.NextBytes(buffer);
            return new BigInteger(buffer, isUnsigned: true, isBigEndian: false);
        }

        private float RandomSingle()
        {
            random.NextBytes(buffer);
            return BitConverter.ToSingle(buffer);
        }

        private double RandomDouble()
        {
            random.NextBytes(buffer);
            return BitConverter.ToDouble(buffer);
        }

        // https://stackoverflow.com/a/610228
        private decimal RandomDecimal() =>
            new decimal(RandomInt32(),
                RandomInt32(),
                random.Next(0x204FCE5E),
                false,
                0);

        private DateTimeOffset RandomDateTimeOffset() => new DateTimeOffset(RandomInt64(DateTimeOffset.MinValue.Ticks, DateTimeOffset.MaxValue.Ticks), TimeSpan.FromHours(random.Next(-12, 15)));

        private string RandomCodePoint()
        {
            try
            {
                var codePoint = random.Next(0, 0x10FFFF + 1);
                return $"{char.ConvertFromUtf32(codePoint)} (U+{codePoint:X})";
            }
            catch
            {
                return $"Failed to select a valid codepoint.";
            }
        }

        private string RandomString()
        {
            var length = random.Next(0, 31);
            var builder = new StringBuilder();
            for (var i = 0; i < length; i++)
            {
                int codePoint;
                do
                {
                    codePoint = random.Next(0, 0x10FFFF + 1);
                } while (codePoint is >= 0xD800 and <= 0xDFFF);
                builder.Append(char.ConvertFromUtf32(codePoint));
            }
            return builder.ToString();
        }
    }
}
