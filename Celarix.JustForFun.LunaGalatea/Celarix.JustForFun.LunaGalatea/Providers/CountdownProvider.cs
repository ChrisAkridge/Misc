using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace Celarix.JustForFun.LunaGalatea.Providers
{
    public class CountdownProvider : IProvider<IReadOnlyList<string>>
    {
        private readonly IClock clock;
        private readonly DateTimeZone easternTime;

        public bool UseMonospaceFont => false;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public CountdownProvider(IClock clock)
        {
            this.clock = clock;
            easternTime = DateTimeZoneProviders.Tzdb["America/New_York"];
        }

        public IReadOnlyList<string> GetDisplayObject()
        {
            var nowInstant = clock.GetCurrentInstant();
            var now = nowInstant.InZone(easternTime);

            var countdowns = new List<string>
            {
                $"New Year's Day: {ForFixedDateEvent(nowInstant, now, 1, 1)}",
                $"Computer Science Day: {ForFixedDateEvent(nowInstant, now, 2, 1)}",
                $"Super Bowl: {ForFixedMonthEvent(nowInstant, now, 2, GetSuperBowlDay)}",
                $"Valentine's Day: {ForFixedDateEvent(nowInstant, now, 2, 14)}",
                $"Vernal Equinox: {ForFixedMonthEvent(nowInstant, now, 3, GetSpringEquinoxDay)}",
                $"Easter: {ForUnfixedEvent(nowInstant, now, GetEasterMonthAndDay)}",
                $"Thunder Over Louisville: {ForFixedMonthEvent(nowInstant, now, 4, GetThunderOverLouisvilleDay)}",
                $"Kentucky Derby: {ForFixedMonthEvent(nowInstant, now, 5, GetKentuckyDerbyDay)}",
                $"Mother's Day: {ForFixedMonthEvent(nowInstant, now, 5, GetMothersDayDay)}",
                $"Memorial Day: {ForFixedMonthEvent(nowInstant, now, 5, GetMemorialDayDay)}",
                $"Father's Day: {ForFixedMonthEvent(nowInstant, now, 6, GetFathersDayDay)}",
                $"Summer Solstice: {ForFixedMonthEvent(nowInstant, now, 6, GetSummerSolsticeDay)}",
                $"Independence Day: {ForFixedDateEvent(nowInstant, now, 7, 4)}",
                $"Dad's Birthday: {ForFixedDateEvent(nowInstant, now, 7, 29)}",
                $"Mom's Birthday: {ForFixedDateEvent(nowInstant, now, 8, 16)}",
                $"Labor Day: {ForFixedMonthEvent(nowInstant, now, 9, GetLaborDayDay)}",
                $"NFL Kickoff (est.): {ForFixedMonthEvent(nowInstant, now, 9, GetNFLKickoffEstimatedDay)}",
                $"My Birthday: {ForFixedDateEvent(nowInstant, now, 9, 8)}",
                $"Autumnal Equinox: {ForFixedMonthEvent(nowInstant, now, 9, GetAutumnEquinoxDay)}",
                $"Holiday/Halloween Season: {ForFixedDateEvent(nowInstant, now, 10, 1)}",
                $"Alex's Birthday: {ForFixedDateEvent(nowInstant, now, 10, 8)}",
                $"Halloween: {ForFixedDateEvent(nowInstant, now, 10, 31)}",
                $"Thanksgiving Season: {ForFixedDateEvent(nowInstant, now, 11, 1)}",
                $"Election Day: {ForElectionDay(nowInstant, now)}",
                $"Veterans Day: {ForFixedDateEvent(nowInstant, now, 11, 11)}",
                $"Thanksgiving: {ForFixedMonthEvent(nowInstant, now, 11, GetThanksgivingDay)}",
                $"Black Friday/Christmas Season: {ForFixedMonthEvent(nowInstant, now, 11, GetBlackFridayChristmasSeasonDay)}",
                $"Winter Solstice: {ForFixedMonthEvent(nowInstant, now, 12, GetWinterSolsticeDay)}",
                $"Christmas Eve: {ForFixedDateEvent(nowInstant, now, 12, 24)}",
                $"Christmas Day: {ForFixedDateEvent(nowInstant, now, 12, 25)}",
                $"New Year's Season: {ForFixedDateEvent(nowInstant, now, 12, 26)}",
                $"New Year's Eve: {ForFixedDateEvent(nowInstant, now, 12, 31)}",
                $"Great American Solar Eclipse: {ForOnceOffEvent(nowInstant, now, 2045, 8, 12)}"
            };

            // TODO: we could REALLY make this better in the future
            return countdowns
                .Select(c => c.Split(':'))
                .Where(s => !string.IsNullOrWhiteSpace(s[1]))
                .OrderBy(s => s[1].Contains("Today", StringComparison.InvariantCultureIgnoreCase) ? 0 : int.Parse(s[1][..s[1].IndexOf('d')]))
                .Select(s => string.Join(":", s))
                .ToList();
        }

        private string ForFixedDateEvent(Instant nowInstant, ZonedDateTime now, int eventMonth, int eventDay)
        {
            if (now.Month == eventMonth && now.Day == eventDay)
            {
                return "Today!";
            }

            var nextEventYear = now.Month < eventMonth || (now.Month == eventMonth && now.Day < eventDay)
                ? now.Year
                : now.Year + 1;
            var nextOccurrenceLocalDate = new LocalDateTime(nextEventYear, eventMonth, eventDay, 0, 0, 0);
            var nextOccurrenceZonedDate = easternTime.AtStrictly(nextOccurrenceLocalDate);
            return FormatDuration(nextOccurrenceZonedDate.ToInstant() - nowInstant);
        }

        private string ForFixedMonthEvent(Instant nowInstant, ZonedDateTime now, int eventMonth, Func<int, int> eventDayFunc)
        {
            var dayForThisYear = eventDayFunc(now.Year);
            var dayForNextYear = eventDayFunc(now.Year + 1);

            if (now.Month == eventMonth && now.Day == dayForThisYear)
            {
                return "Today!";
            }

            var nextOccurrenceLocalDate =
                now.Month < eventMonth || (now.Month == eventMonth && now.Day < dayForThisYear)
                    ? new LocalDateTime(now.Year, eventMonth, dayForThisYear, 0, 0, 0)
                    : new LocalDateTime(now.Year + 1, eventMonth, dayForNextYear, 0, 0, 0);
            var nextOccurrenceZonedDate = easternTime.AtStrictly(nextOccurrenceLocalDate);
            return FormatDuration(nextOccurrenceZonedDate.ToInstant() - nowInstant);
        }

        private string ForElectionDay(Instant nowInstant, ZonedDateTime now)
        {
            var thisIsAnElectionYear = now.Year % 2 == 0;
            var electionAlreadyPassed = !thisIsAnElectionYear
                || (now.Month == 12 || (now.Month >= 11 && now.Day > GetElectionDayDay(now.Year)));
            var nextElectionYear = electionAlreadyPassed
                ? now.Year + 2
                : now.Year;
            
            const int eventMonth = 11;
            var eventDay = GetElectionDayDay(nextElectionYear);
            
            if (now.Month == eventMonth && now.Day == eventDay && !electionAlreadyPassed) { return "Today!"; }

            var nextEventYear = now.Month < eventMonth || (now.Month == eventMonth && now.Day < eventDay)
                ? now.Year
                : now.Year + 2;
            var nextOccurrenceLocalDate = new LocalDateTime(nextEventYear, eventMonth, eventDay, 0, 0, 0);
            var nextOccurrenceZonedDate = easternTime.AtStrictly(nextOccurrenceLocalDate);
            return FormatDuration(nextOccurrenceZonedDate.ToInstant() - nowInstant);
        }

        private string ForUnfixedEvent(Instant nowInstant,
            ZonedDateTime now,
            Func<int, (int Month, int Day)> eventMonthDayFunc)
        {
            var (monthForThisYear, dayForThisYear) = eventMonthDayFunc(now.Year);
            var (monthForNextYear, dayForNextYear) = eventMonthDayFunc(now.Year + 1);

            if (now.Month == monthForThisYear && now.Day == dayForThisYear)
            {
                return "Today!";
            }

            var nextOccurrenceLocalDate =
                now.Month < monthForThisYear || (now.Month == monthForThisYear && now.Day < dayForThisYear)
                    ? new LocalDateTime(now.Year, monthForThisYear, dayForThisYear, 0, 0, 0)
                    : new LocalDateTime(now.Year + 1, monthForNextYear, dayForNextYear, 0, 0, 0);
            var nextOccurrenceZonedDate = easternTime.AtStrictly(nextOccurrenceLocalDate);
            return FormatDuration(nextOccurrenceZonedDate.ToInstant() - nowInstant);
        }

        private string? ForOnceOffEvent(Instant nowInstant,
            ZonedDateTime now,
            int eventYear,
            int eventMonth,
            int eventDay)
        {
            if (now.Year == eventYear
                && now.Month == eventMonth
                && now.Day == eventDay)
            {
                return "Today!";
            }

            if (now.Year > eventYear
                || (now.Year == eventYear && now.Month == eventMonth)
                || (now.Year == eventYear && now.Month == eventMonth && now.Day > eventDay))
            {
                return null;
            }

            var eventLocalDate = new LocalDateTime(eventYear, eventMonth, eventDay, 0, 0, 0);
            var eventZonedDate = easternTime.AtStrictly(eventLocalDate);
            return FormatDuration(eventZonedDate.ToInstant() - nowInstant);
        }

        private static string FormatDuration(Duration duration)
        {
            var durationString = duration.ToString("D'd'h'h'm'm's's'", CultureInfo.CurrentCulture);
            var decibelSeconds = (duration.TotalSeconds > 0d)
                ? (10d * Math.Log10(duration.TotalSeconds)).ToString("F4")
                : "Right Now!";
            return $"{durationString} ({decibelSeconds} dBsec)";
        }

        private static int GetSpringEquinoxDay(int year) =>
            year switch
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

        private static int GetSummerSolsticeDay(int year) =>
            year switch
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

        private static int GetAutumnEquinoxDay(int year) =>
            year switch
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

        private static int GetWinterSolsticeDay(int year) =>
            year switch
            {
                >= 2022 and <= 2042 => 21,
                _ => throw new ArgumentOutOfRangeException(nameof(year))
            };

        private static int GetSuperBowlDay(int year) =>
            LocalDate.FromYearMonthWeekAndDay(year, 2, 2, IsoDayOfWeek.Sunday).Day;
        
        private static (int Month, int Day) GetEasterMonthAndDay(int year) =>
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
            
        private static int GetThunderOverLouisvilleDay(int year) =>
            LocalDate.FromYearMonthWeekAndDay(year, 5, 1, IsoDayOfWeek.Saturday).PlusWeeks(-2).Day;

        private static int GetKentuckyDerbyDay(int year) =>
            LocalDate.FromYearMonthWeekAndDay(year, 5, 1, IsoDayOfWeek.Saturday).Day;

        private static int GetMothersDayDay(int year) =>
            LocalDate.FromYearMonthWeekAndDay(year, 5, 2, IsoDayOfWeek.Sunday).Day;

        private static int GetMemorialDayDay(int year) =>
            LocalDate.FromYearMonthWeekAndDay(year, 5, 5, IsoDayOfWeek.Monday).Day;

        private static int GetFathersDayDay(int year) =>
            LocalDate.FromYearMonthWeekAndDay(year, 6, 3, IsoDayOfWeek.Sunday).Day;

        private static int GetLaborDayDay(int year) =>
            LocalDate.FromYearMonthWeekAndDay(year, 9, 1, IsoDayOfWeek.Monday).Day;

        private static int GetNFLKickoffEstimatedDay(int year) =>
            LocalDate.FromYearMonthWeekAndDay(year, 9, 1, IsoDayOfWeek.Monday).Day + 3;

        private static int GetElectionDayDay(int year) => LocalDate.FromYearMonthWeekAndDay(year, 11, 1, IsoDayOfWeek.Monday).Day + 1;

        private static int GetThanksgivingDay(int year) =>
            LocalDate.FromYearMonthWeekAndDay(year, 11, 4, IsoDayOfWeek.Thursday).Day;

        private static int GetBlackFridayChristmasSeasonDay(int year) =>
            LocalDate.FromYearMonthWeekAndDay(year, 11, 4, IsoDayOfWeek.Friday).Day;
    }
}
