using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.ReceiptPrinter.Logic
{
    internal static class AdvancedCountdownFunctions
    {
        public static (int Month, int Day) GetSpringEquinox(int year)
        {
            const int SpringEquinoxMonth = 3;
            var day = year switch
            {
                2022 => 20,
                2023 => 20,
                2024 => 19,
                2025 => 20,
                2026 => 20,
                2027 => 20,
                2028 => 19,
                2029 => 20,
                2030 => 20,
                2031 => 20,
                2032 => 19,
                2033 => 20,
                2034 => 20,
                2035 => 20,
                2036 => 19,
                2037 => 20,
                2038 => 20,
                2039 => 20,
                2040 => 19,
                2041 => 20,
                2042 => 20,
                _ => throw new ArgumentOutOfRangeException(nameof(year))
            };
            return (SpringEquinoxMonth, day);
        }

        public static (int Month, int Day) GetSummerSolstice(int year)
        {
            const int SummerSolsticeMonth = 6;
            var day = year switch
            {
                2022 => 21,
                2023 => 21,
                2024 => 20,
                2025 => 20,
                2026 => 21,
                2027 => 21,
                2028 => 20,
                2029 => 20,
                2030 => 21,
                2031 => 21,
                2032 => 20,
                2033 => 20,
                2034 => 21,
                2035 => 21,
                2036 => 20,
                2037 => 20,
                2038 => 21,
                2039 => 21,
                2040 => 20,
                2041 => 20,
                2042 => 21,
                _ => throw new ArgumentOutOfRangeException(nameof(year))
            };
            return (SummerSolsticeMonth, day);
        }

        public static (int Month, int Day) GetAutumnEquinox(int year)
        {
            const int FallEquinoxMonth = 9;
            var day = year switch
            {
                2022 => 22,
                2023 => 23,
                2024 => 22,
                2025 => 22,
                2026 => 22,
                2027 => 23,
                2028 => 22,
                2029 => 22,
                2030 => 22,
                2031 => 22,
                2032 => 22,
                2033 => 22,
                2034 => 22,
                2035 => 22,
                2036 => 22,
                2037 => 22,
                2038 => 22,
                2039 => 22,
                2040 => 22,
                2041 => 22,
                2042 => 22,
                _ => throw new ArgumentOutOfRangeException(nameof(year))
            };
            return (FallEquinoxMonth, day);
        }

        public static (int Month, int Day) GetWinterSolstice(int year)
        {
            const int WinterSolsticeMonth = 12;
            var day = year switch
            {
                >= 2022 and <= 2042 => 21,
                _ => throw new ArgumentOutOfRangeException(nameof(year))
            };
            return (WinterSolsticeMonth, day);
        }

        public static (int Month, int Day) GetEasterMonthAndDay(int year) =>
            year switch
            {
                2022 => (4, 17),
                2023 => (4, 9),
                2024 => (3, 31),
                2025 => (4, 20),
                2026 => (4, 6),
                2027 => (3, 27),
                2028 => (4, 16),
                2029 => (4, 1),
                2030 => (4, 21),
                2031 => (4, 13),
                2032 => (3, 28),
                2033 => (4, 17),
                2034 => (4, 9),
                2035 => (3, 25),
                2036 => (4, 13),
                2037 => (4, 5),
                2038 => (4, 25),
                2039 => (4, 10),
                2040 => (4, 1),
                2041 => (4, 21),
                2042 => (4, 6),
                _ => throw new ArgumentOutOfRangeException(nameof(year))
            };
    }
}
