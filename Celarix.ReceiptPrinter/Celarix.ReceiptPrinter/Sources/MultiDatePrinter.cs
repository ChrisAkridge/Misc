using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.ReceiptPrinter.Sources
{
    public static class MultiDatePrinter
    {
        private const int MaxColumns = 48;

        public static string GetMultiDates(DateOnly date)
        {
            var multiDateBuilder = new StringBuilder();
            // Long format: Friday, October 13, 2023
            multiDateBuilder.AppendLine(date.ToString("dddd, MMMM d, yyyy"));
            // Short format (ISO 8601): 2023-10-13
            multiDateBuilder.AppendLine(date.ToString("yyyy-MM-dd"));
            // Julian date: 286
            multiDateBuilder.AppendLine(date.ToString("D3"));
            // ISO week and day: 2023-W41-5
            multiDateBuilder.AppendLine(date.ToString("yyyy-'W'ww-e"));
            // Roman numerals: MMXXIII-X-XIII
            multiDateBuilder.AppendLine(ToRomanNumerals(date.Year) + "-" + ToRomanNumerals(date.Month) + "-" + ToRomanNumerals(date.Day));
            // Unix timestamp range: 1697155200 - 1697241599
            var unixStart = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();
            var unixEnd = new DateTimeOffset(date.Year, date.Month, date.Day, 23, 59, 59, TimeSpan.Zero).ToUnixTimeSeconds();
            multiDateBuilder.AppendLine($"{unixStart} - {unixEnd}");

            multiDateBuilder.AppendLine();

            // Binary: 011111100111 - 1010 - 01101
            var binaryYear = Convert.ToString(date.Year, 2).PadLeft(12, '0');
            var binaryMonth = Convert.ToString(date.Month, 2).PadLeft(4, '0');
            var binaryDay = Convert.ToString(date.Day, 2).PadLeft(5, '0');
            multiDateBuilder.AppendLine($"2: {binaryYear} - {binaryMonth} - {binaryDay}");

            // Octal: 3747 - 12 - 15
            var octalYear = Convert.ToString(date.Year, 8).PadLeft(4, '0');
            var octalMonth = Convert.ToString(date.Month, 8).PadLeft(2, '0');
            var octalDay = Convert.ToString(date.Day, 8).PadLeft(3, '0');
            multiDateBuilder.AppendLine($"8: {octalYear} - {octalMonth} - {octalDay}");

            // Hexadecimal: 7E7 - A - D
            var hexYear = Convert.ToString(date.Year, 16).PadLeft(3, '0').ToUpper();
            var hexMonth = Convert.ToString(date.Month, 16).PadLeft(1, '0').ToUpper();
            var hexDay = Convert.ToString(date.Day, 16).PadLeft(1, '0').ToUpper();
            multiDateBuilder.AppendLine($"16: {hexYear} - {hexMonth} - {hexDay}");

            // Base 64: 7w - C - D
            var base64Year = NumberToBase64(date.Year);
            var base64Month = NumberToBase64(date.Month);
            var base64Day = NumberToBase64(date.Day);
            multiDateBuilder.AppendLine($"64: {base64Year} - {base64Month} - {base64Day}");

            multiDateBuilder.AppendLine();

            // Unix timestamp range in binary
            var unixStartBinary = Convert.ToString(unixStart, 2).PadLeft(32, '0');
            var unixEndBinary = Convert.ToString(unixEnd, 2).PadLeft(32, '0');
            multiDateBuilder.AppendLine($"Unix Binary: {unixStartBinary} - {unixEndBinary}");

            // Unix timestamp range in octal
            var unixStartOctal = Convert.ToString(unixStart, 8).PadLeft(11, '0');
            var unixEndOctal = Convert.ToString(unixEnd, 8).PadLeft(11, '0');
            multiDateBuilder.AppendLine($"Unix Octal: {unixStartOctal} - {unixEndOctal}");

            // Unix timestamp range in hexadecimal
            var unixStartHex = Convert.ToString(unixStart, 16).PadLeft(8, '0').ToUpper();
            var unixEndHex = Convert.ToString(unixEnd, 16).PadLeft(8, '0').ToUpper();
            multiDateBuilder.AppendLine($"Unix Hex: {unixStartHex} - {unixEndHex}");

            // Unix timestamp range in base 64
            var unixStartBase64 = NumberToBase64(unixStart);
            var unixEndBase64 = NumberToBase64(unixEnd);
            multiDateBuilder.AppendLine($"Unix Base64: {unixStartBase64} - {unixEndBase64}");

            multiDateBuilder.AppendLine();

            // Planck times since the start of the universe, in hexadecimal
            var planckTimesPerSecond = BigInteger.Parse("18500000000000000000000000000000000000000000");
            var yearsSinceBigBang = BigInteger.Parse("13700000000") + date.Year;
            var secondsSinceStartOfYear = date.DayOfYear * 24 * 60 * 60;
            var secondsSinceBigBang = (BigInteger.Parse("31557600") * yearsSinceBigBang) + secondsSinceStartOfYear;
            var planckTimesSinceBigBang = planckTimesPerSecond * secondsSinceBigBang;
            var planckTimesHex = planckTimesSinceBigBang.ToString("X");

            // Split on every MaxColumns characters
            var planckTimesHexLines = new List<string>();
            for (int i = 0; i < planckTimesHex.Length; i += MaxColumns)
            {
                planckTimesHexLines.Add(planckTimesHex.Substring(i, Math.Min(MaxColumns, planckTimesHex.Length - i)));
            }
            multiDateBuilder.AppendLine($"Planck times since Big Bang:");
            foreach (var line in planckTimesHexLines)
            {
                multiDateBuilder.AppendLine(line);
            }

            // Planck times since the start of the universe, in base 64
            var planckTimesBase64 = BigIntegerToBase64(planckTimesSinceBigBang);
            var planckTimesBase64Lines = new List<string>();
            for (int i = 0; i < planckTimesBase64.Length; i += MaxColumns)
            {
                planckTimesBase64Lines.Add(planckTimesBase64.Substring(i, Math.Min(MaxColumns, planckTimesBase64.Length - i)));
            }
            multiDateBuilder.AppendLine($"Planck times since Big Bang (Base 64):");
            foreach (var line in planckTimesBase64Lines)
            {
                multiDateBuilder.AppendLine(line);
            }

            return multiDateBuilder.ToString();
        }

        private static string ToRomanNumerals(int number)
        {
            var romanNumeralBuilder = new StringBuilder();
            var romanNumerals = new Dictionary<int, string>
            {
                { 1000, "M" },
                { 900, "CM" },
                { 500, "D" },
                { 400, "CD" },
                { 100, "C" },
                { 90, "XC" },
                { 50, "L" },
                { 40, "XL" },
                { 10, "X" },
                { 9, "IX" },
                { 5, "V" },
                { 4, "IV" },
                { 1, "I" }
            };
            foreach (var kvp in romanNumerals)
            {
                while (number >= kvp.Key)
                {
                    romanNumeralBuilder.Append(kvp.Value);
                    number -= kvp.Key;
                }
            }
            return romanNumeralBuilder.ToString();
        }

        private static string NumberToBase64(long number)
        {
            const string base64Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
            var result = new StringBuilder();
            while (number > 0)
            {
                result.Insert(0, base64Chars[(int)(number & 0b111111)]);
                number >>= 6;
            }
            return result.ToString();
        }

        private static string BigIntegerToBase64(BigInteger number)
        {
            const string base64Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
            var result = new StringBuilder();
            while (number > 0)
            {
                result.Insert(0, base64Chars[(int)(number & 0b111111)]);
                number >>= 6;
            }
            return result.ToString();
        }
    }
}
