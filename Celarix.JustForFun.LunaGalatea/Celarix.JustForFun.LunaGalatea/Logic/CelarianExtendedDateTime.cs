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

        private static readonly List<ExtendedDayCultureInfo> cultureInfo = GetCultureInfo();

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

        public CelarianExtendedDateTime(int year,
            int month,
            int day,
            int hour,
            int minute,
            int second) : this((year * TicksPerYear)
            + (month * TicksPerMonth)
            + (day * TicksPerDay)
            + (hour * TicksPerHour)
            + (minute * TicksPerMinute)
            + (second * TicksPerSecond)) { }

        public string ToISO8601StyleString() => $"{Year + 1}-{Month + 1}-{Day + 1} {Hour:D2}:{Minute:D2}:{Second:D2}";

        public string ToAmericanLongDateStyleString() =>
            $"{DayOfWeekName}, {MonthName} {Day + 1}, {Year + 1} {HoursInMeridian:D2}:{Minute:D2}:{Second:D2} {Meridian}";

        public string GetDayCulture() => cultureInfo[GetIndexOfCurrentCultureInfo()].DayCulture;

        public string GetTimeCulture() => cultureInfo[GetIndexOfCurrentCultureInfo()].TimeCulture;

        public DateTimeOffset GetTimeOfNextCulture()
        {
            var currentCultureIndex = GetIndexOfCurrentCultureInfo();
            var currentCulture = cultureInfo[currentCultureIndex];
            var nextCultureIndex = currentCultureIndex + 1;
            
            if (currentCultureIndex == cultureInfo.Count - 1)
            {
                nextCultureIndex = 0;
            }
            
            var nextCulture = cultureInfo[nextCultureIndex];
            var now = new CelarianExtendedDateTime(DateTimeOffset.UtcNow);
            var startOfToday = new CelarianExtendedDateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            CelarianExtendedDateTime nextCultureStartTime;

            if (currentCulture.DayOfWeekNumber != nextCulture.DayOfWeekNumber)
            {
                var startOfTomorrow = new CelarianExtendedDateTime(startOfToday.Ticks + TicksPerDay);
                nextCultureStartTime = new CelarianExtendedDateTime(startOfTomorrow.Ticks + (nextCulture.StartingHour * TicksPerHour));
            }
            else
            {
                nextCultureStartTime =
                    new CelarianExtendedDateTime(startOfToday.Ticks + (nextCulture.StartingHour * TicksPerHour));
            }
            
            return new DateTimeOffset(nextCultureStartTime.Ticks, DateTimeOffset.UtcNow.Offset);
        }

        private int GetIndexOfCurrentCultureInfo()
        {
            for (var i = 0; i < cultureInfo.Count; i++)
            {
                var info = cultureInfo[i];
                if (info.DayOfWeekNumber == Day % 14 && Hour >= info.StartingHour && Hour < info.EndingHour)
                {
                    return i;
                }
            }

            throw new InvalidOperationException();
        }

        private static List<ExtendedDayCultureInfo> GetCultureInfo()
        {
            var cultureInfos = new List<ExtendedDayCultureInfo>();
            
            for (int i = 0; i < 14; i++)
            {
                var fullDayName = fullDayNames[i];
                switch (fullDayName)
                {
                    case "Themisday" or "Palday":
                    {
                        const string dayCulture = "Mid-week day off";
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 0, 3, dayCulture, "First sleep"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 3, 6, dayCulture, "Overnight wakefulness"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 6, 14, dayCulture, "Second sleep"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 14, 16, dayCulture, "Breakfast"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 16, 24, dayCulture, "Chores, homework, errands, maintenance, lunch"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 24, 25, dayCulture, "Siesta"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 25, 27, dayCulture, "Dinner"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 27, 33, dayCulture, "High leisure"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 33, 37, dayCulture, "Low leisure"));
                        break;
                    }
                    case "Galaday":
                    {
                        const string dayCulture = "Day off for community";
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 0, 3, dayCulture, "First sleep"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 3, 6, dayCulture, "Overnight wakefulness"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 6, 14, dayCulture, "Second sleep"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 14, 16, dayCulture, "Breakfast"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 16, 22, dayCulture, "Fellowship with community"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 22, 24, dayCulture, "Lunch"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 24, 26, dayCulture, "Siesta"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 26, 32, dayCulture, "High leisure"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 32, 34, dayCulture, "Dinner"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 34, 37, dayCulture, "Low leisure"));
                        break;
                    }
                    case "Kerday":
                    {
                        const string dayCulture = "Day off for family";
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 0, 3, dayCulture, "First sleep"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 3, 6, dayCulture, "Overnight wakefulness"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 6, 14, dayCulture, "Second sleep"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 14, 16, dayCulture, "Breakfast"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 16, 22, dayCulture, "High leisure"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 22, 24, dayCulture, "Lunch"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 24, 30, dayCulture, "Family time"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 30, 32, dayCulture, "Siesta"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 32, 34, dayCulture, "Dinner"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 34, 37, dayCulture, "Low leisure"));
                        break;
                    }
                    case "Lunaday":
                    {
                        const string dayCulture = "Day off for self and partners";
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 0, 3, dayCulture, "First sleep"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 3, 6, dayCulture, "Overnight wakefulness"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 6, 14, dayCulture, "Second sleep"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 14, 16, dayCulture, "Breakfast"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 16, 20, dayCulture, "High leisure"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 20, 21, dayCulture, "Lunch"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 21, 23, dayCulture, "Siesta"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 23, 24, dayCulture, "Dinner"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 24, 29, dayCulture, "Low leisure"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 29, 37, dayCulture, "Lover's time 💖"));
                        break;
                    }
                    default:
                    {
                        const string dayCulture = "Workday";
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 0, 3, dayCulture, "First sleep"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 3, 6, dayCulture, "Overnight wakefulness"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 6, 14, dayCulture, "Second sleep"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 14, 16, dayCulture, "Breakfast"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 16, 26, dayCulture, "Work"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 26, 27, dayCulture, "Siesta"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 27, 29, dayCulture, "Dinner"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 29, 33, dayCulture, "Chores, homework, errands, maintenance"));
                        cultureInfos.Add(new ExtendedDayCultureInfo(i, 33, 37, dayCulture, "Hobbies, projects, entertainment"));
                        break;
                    }
                }
            }
            
            return cultureInfos;
        }
    }
}
