using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Logic
{
    public readonly struct CelarianExtendedDateTime
    {
        private const long TicksPerSecond = 10_000_000;
        private const long TicksPerMinute = TicksPerSecond * 60;
        private const long TicksPerHour = TicksPerMinute * 60;
        private const long TicksPerDay = TicksPerHour * 37;
        private const long TicksPerWeek = TicksPerDay * 14;
        private const long TicksPerMonth = TicksPerWeek * 2;
        private const long TicksPerYear = TicksPerMonth * 13;

        private static readonly string[] fullDayNames =
        {
            "Belday",
            "Freday",
            "Valday",
            "Themisday",
            "Hemday",
            "Veday",
            "Loday",
            "Palday",
            "Cereday",
            "Somniday",
            "Victorday",
            "Galaday",
            "Kerday",
            "Lunaday"
        };

        private static readonly string[] abbreviatedDayNames =
        {
            "BEL", "FRE", "VAL", "THE", "HEM", "VED", "LOD", "PAL", "CER", "SOM",
            "VIC", "GAL", "KER", "LUN"
        };

        private static readonly string[] monthNames =
        {
            "Satuary",
            "Mercurary",
            "Tear",
            "Cerene",
            "Aurian",
            "Vitelian",
            "Claudian",
            "Carinian",
            "Galerian",
            "Constantian",
            "Hendecember",
            "Dodecember",
            "Kaidecember"
        };

        private static readonly string[] abbreviatedMonthNames =
        {
            "SAT", "MER", "TEA", "CER", "AUR", "VIT", "CLA", "CAR", "GLR", "CON",
            "HEN", "DOD", "KAI"
        };

        /// <summary>
        /// The number of 100ns ticks since 0001-01-01T00:00:00Z.
        /// </summary>
        public long Ticks { get; }

        public int Second => (int)((Ticks / TicksPerSecond) % 60L);
        public int Minute => (int)((Ticks / TicksPerMinute) % 60L);
        public int Hour => (int)((Ticks / TicksPerHour) % 37L);
        public int Day => (int)((Ticks / TicksPerDay) % 28L);
        public int Month => (int)((Ticks / TicksPerMonth) % 13L);
        public int Year => (int)(Ticks / TicksPerYear);

        public int HoursInMeridian =>
            Hour < 14
                ? Hour
                : Hour - 14;

        public string Meridian =>
            Hour < 14
                ? "PT"
                : "DS";

        public string DayOfWeekName => fullDayNames[Day % 14];
        public string DayOfWeekAbbreviatedName => abbreviatedDayNames[Day % 14];
        public string MonthName => monthNames[Month];
        public string MonthAbbreviatedName => abbreviatedMonthNames[Month];

        public bool IsWorkingDay =>
            Day != 3
            && Day != 7
            && Day < 11;

        public int SeasonIndex =>
            Month switch
            {
                >= 0 and < 3 => 0,
                >= 4 and < 6 => 1,
                >= 6 and < 9 => 2,
                >= 9 => 3,
                _ => throw new InvalidOperationException()
            };

        public CelarianExtendedDateTime(long ticks)
        {
            if (ticks < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ticks));
            }

            Ticks = ticks;
        }

        public CelarianExtendedDateTime(DateTimeOffset earthTime) : this(earthTime.Ticks) { }

        public string ToISO8601StyleString() => $"{Year + 1}-{Month + 1}-{Day + 1} {Hour:D2}:{Minute:D2}:{Second:D2}";

        public string ToAmericanLongDateStyleString() =>
            $"{DayOfWeekName}, {MonthName} {Day + 1}, {Year + 1} {HoursInMeridian:D2}:{Minute:D2}:{Second:D2} {Meridian}";

        public string GetDayCulture() =>
            DayOfWeekName switch
            {
                "Themisday" => "Mid-week day off",
                "Palday" => "Mid-week day off",
                "Galaday" => "Day off for community",
                "Kerday" => "Day off for family",
                "Lunaday" => "Day off for self and partners",
                _ => "Workday"
            };

        public string GetTimeCulture() =>
            DayOfWeekName switch
            {
                "Themisday" or "Palday" => Hour switch
                {
                    >= 0 and < 3 => "First sleep",
                    >= 3 and < 6 => "Overnight wakefulness",
                    >= 6 and < 14 => "Second sleep",
                    >= 14 and < 16 => "Breakfast",
                    >= 16 and < 24 => "Chores, homework, errands, maintenance, lunch",
                    >= 24 and < 25 => "Siesta",
                    >= 25 and < 27 => "Dinner",
                    >= 27 and < 33 => "High Leisure",
                    >= 33 and < 37 => "Low Leisure",
                    _ => throw new ArgumentOutOfRangeException()
                },
                "Galaday" => Hour switch
                {
                    >= 0 and < 3 => "First sleep",
                    >= 3 and < 6 => "Overnight wakefulness",
                    >= 6 and < 14 => "Second sleep",
                    >= 14 and < 16 => "Breakfast",
                    >= 16 and < 22 => "Fellowship with community",
                    >= 22 and < 24 => "Lunch",
                    >= 24 and < 26 => "Siesta",
                    >= 26 and < 32 => "High Leisure",
                    >= 32 and < 34 => "Dinner",
                    >= 34 and < 37 => "Low Leisure",
                    _ => throw new ArgumentOutOfRangeException()
                },
                "Kerday" => Hour switch
                {
                    >= 0 and < 3 => "First sleep",
                    >= 3 and < 6 => "Overnight wakefulness",
                    >= 6 and < 14 => "Second sleep",
                    >= 14 and < 16 => "Breakfast",
                    >= 16 and < 22 => "High Leisure",
                    >= 22 and < 24 => "Lunch",
                    >= 24 and < 30 => "Family time",
                    >= 30 and < 32 => "Siesta",
                    >= 32 and < 34 => "Dinner",
                    >= 34 and < 37 => "Low Leisure",
                    _ => throw new ArgumentOutOfRangeException()
                },
                "Lunaday" => Hour switch
                {
                    >= 0 and < 3 => "First sleep",
                    >= 3 and < 6 => "Overnight wakefulness",
                    >= 6 and < 14 => "Second sleep",
                    >= 14 and < 16 => "Breakfast",
                    >= 16 and < 20 => "High Leisure",
                    >= 20 and < 21 => "Lunch",
                    >= 21 and < 23 => "Siesta",
                    >= 23 and < 24 => "Dinner",
                    >= 25 and < 29 => "Low Leisure",
                    >= 29 and < 37 => "Lover's Time 💖",
                    _ => throw new ArgumentOutOfRangeException()
                },
                _ => Hour switch
                {
                    >= 0 and < 3 => "First sleep",
                    >= 3 and < 6 => "Overnight wakefulness",
                    >= 6 and < 14 => "Second sleep",
                    >= 14 and < 16 => "Breakfast",
                    >= 16 and < 26 => "Work",
                    >= 26 and < 27 => "Siesta",
                    >= 27 and < 29 => "Dinner",
                    >= 29 and < 33 => "Chores, homework, errands, maintenance",
                    >= 33 and < 37 => "Hobbies, projects, entertainment",
                    _ => throw new ArgumentOutOfRangeException()
                }
            };
    }
}
