using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.LunaGalatea.Logic.Countdown.CountdownKinds;
using Celarix.JustForFun.LunaGalatea.Logic.Countdown;
using NodaTime;

namespace Celarix.JustForFun.LunaGalatea.Providers
{
    public class CountdownProvider : IProvider<IReadOnlyList<string>>
    {
        private readonly IClock clock;
        private static readonly DateTimeZone easternTime = DateTimeZoneProviders.Tzdb["America/New_York"];

        public bool UseMonospaceFont => false;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public CountdownProvider(IClock clock)
        {
            this.clock = clock;
        }

        public IReadOnlyList<string> GetDisplayObject()
        {
            var nowInstant = clock.GetCurrentInstant();
            var now = nowInstant.InZone(easternTime);
            var countdowns = GetStandardCountdowns();
            var realizedCountdowns = countdowns
                .Select(c => RealizeCountdown(c, now))
                .OrderBy(c => c.DurationUntil(now));
            return realizedCountdowns
                .Select(c => GetCountdownString(c, now))
                .ToList();
        }

        internal static Countdown[] GetStandardCountdowns()
        {
            return
            [
                // Fixed date countdowns
                new FixedDateCountdown("New Year's Day", 1, 1),
                new FixedDateCountdown("Computer Science Day", 2, 1),
                new FixedDateCountdown("Groundhog Day", 2, 2),
                new FixedDateCountdown("E Day", 2, 7),
                new FixedDateCountdown("Valentine's Day", 2, 14),
                new FixedDateCountdown("Pi Day", 3, 14),
                new FixedDateCountdown("Tax Day", 4, 15),
                new FixedDateCountdown("Independence Day", 7, 4),
                new FixedDateCountdown("Holiday/Halloween Season", 10, 1),
                new FixedDateCountdown("Halloween", 10, 31),
                new FixedDateCountdown("Thanksgiving Season", 11, 1),
                new FixedDateCountdown("Veteran's Day", 11, 11),
                new FixedDateCountdown("Christmas Eve", 12, 24),
                new FixedDateCountdown("Christmas Day", 12, 25),
                new FixedDateCountdown("New Year's Season", 12, 26),
                new FixedDateCountdown("New Year's Eve", 12, 31),

                // Numbered anniversaries
                new NumberedAnniversaryCountdownWrapper("Bob Akridge", new FixedDateCountdown("", 7, 29), 1967, NumberedAnniversaryKind.Birthday),
                new NumberedAnniversaryCountdownWrapper("Michelle Akridge", new FixedDateCountdown("", 8, 16), 1965, NumberedAnniversaryKind.Birthday),
                new NumberedAnniversaryCountdownWrapper("Chris Akridge", new FixedDateCountdown("", 9, 8), 1994, NumberedAnniversaryKind.Birthday),
                new NumberedAnniversaryCountdownWrapper("Alex Akridge", new FixedDateCountdown("", 10, 8), 1996, NumberedAnniversaryKind.Birthday),
                new NumberedAnniversaryCountdownWrapper("Mom and Dad's First Wedding", new FixedDateCountdown("", 6, 3), 1989, NumberedAnniversaryKind.Anniversary),
                new NumberedAnniversaryCountdownWrapper("Mom and Dad's Second Wedding", new FixedDateCountdown("", 6, 17), 2014, NumberedAnniversaryKind.Anniversary),
                new NumberedAnniversaryCountdownWrapper("Super Bowl", new WeekdayOfMonthCountdown("", 2, IsoDayOfWeek.Sunday, 2), 1966, NumberedAnniversaryKind.RomanNumeral),
                new NumberedAnniversaryCountdownWrapper("Start at Meijer", new FixedDateCountdown("", 9, 17), 2013, NumberedAnniversaryKind.Anniversary),
                new NumberedAnniversaryCountdownWrapper("Start at e-PulseTrak.com", new FixedDateCountdown("", 5, 21), 2018, NumberedAnniversaryKind.Anniversary),
                new NumberedAnniversaryCountdownWrapper("Start at RxLightning", new FixedDateCountdown("", 3, 3), 2025, NumberedAnniversaryKind.Anniversary),

                // Weekday-of-month countdowns
                new WeekdayOfMonthCountdown("NCAA-I College Football Playoff Championship", 1, IsoDayOfWeek.Monday, 2),
                new WeekdayOfMonthCountdown("Martin Luther King Jr. Day", 1, IsoDayOfWeek.Monday, 3),
                new WeekdayOfMonthCountdown("NFL Pro Bowl Games", 2, IsoDayOfWeek.Sunday, 1),
                new WeekdayOfMonthCountdown("NBA All-Star Game/Daytona 500", 2, IsoDayOfWeek.Sunday, 3),
                new WeekdayOfMonthCountdown("Presidents' Day", 2, IsoDayOfWeek.Monday, 3),
                new WeekdayOfMonthCountdown("Selection Sunday/DST Begins", 3, IsoDayOfWeek.Sunday, 2),
                new WeekdayOfMonthCountdown("March Madness (est.)", 3, IsoDayOfWeek.Thursday, 2),
                new WeekdayOfMonthCountdown("MLB Opening Day (est.)", 3, IsoDayOfWeek.Thursday, 4),
                new WeekdayOfMonthCountdown("Final Four (est.)", 4, IsoDayOfWeek.Saturday, 1),
                new WeekdayOfMonthCountdown("NHL Playoffs (est.)", 4, IsoDayOfWeek.Wednesday, 2),
                new WeekdayOfMonthCountdown("NBA Playoffs (est.)", 4, IsoDayOfWeek.Saturday, 3),
                new WeekdayOfMonthCountdown("Kentucky Derby", 5, IsoDayOfWeek.Saturday, 1),
                new WeekdayOfMonthCountdown("Mother's Day", 5, IsoDayOfWeek.Sunday, 2),
                new WeekdayOfMonthCountdown("Memorial Day", 5, IsoDayOfWeek.Monday, 4),
                new WeekdayOfMonthCountdown("NHL Stanley Cup Finals (est.)", 6, IsoDayOfWeek.Wednesday, 1),
                new WeekdayOfMonthCountdown("NBA Finals (est.)", 6, IsoDayOfWeek.Thursday, 1),
                new WeekdayOfMonthCountdown("Father's Day", 6, IsoDayOfWeek.Sunday, 3),
                new WeekdayOfMonthCountdown("MLB All-Star Game (est.)", 7, IsoDayOfWeek.Tuesday, 2),
                new WeekdayOfMonthCountdown("NCAA-I Football Kickoff", 9, IsoDayOfWeek.Saturday, 1),
                new WeekdayOfMonthCountdown("NASCAR Playoffs (est.)", 9, IsoDayOfWeek.Sunday, 1),
                new WeekdayOfMonthCountdown("Labor Day", 9, IsoDayOfWeek.Monday, 1),
                new WeekdayOfMonthCountdown("NFL Kickoff", 9, IsoDayOfWeek.Thursday, 1),
                new WeekdayOfMonthCountdown("MLB Playoffs (est.)", 10, IsoDayOfWeek.Tuesday, 1),
                new WeekdayOfMonthCountdown("NHL Opening Night (est.)", 10, IsoDayOfWeek.Wednesday, 1),
                new WeekdayOfMonthCountdown("Columbus Day", 10, IsoDayOfWeek.Monday, 2),
                new WeekdayOfMonthCountdown("NBA Opening Night (est.)", 10, IsoDayOfWeek.Tuesday, 3),
                new WeekdayOfMonthCountdown("Homestead (NASCAR Championship)/DST Ends", 11, IsoDayOfWeek.Sunday, 1),
                new WeekdayOfMonthCountdown("NCAA-I Basketball Season Tipoff", 11, IsoDayOfWeek.Monday, 2),
                new WeekdayOfMonthCountdown("Thanksgiving Day", 11, IsoDayOfWeek.Thursday, 4),
                new WeekdayOfMonthCountdown("Black Friday/Christmas Season", 11, IsoDayOfWeek.Friday, 4),
                new WeekdayOfMonthCountdown("NCAA-I Football Conference Championships", 12, IsoDayOfWeek.Saturday, 1),

                // Month-and-day function countdowns
                new MonthAndDayFunctionCountdown("Spring Equinox", AdvancedCountdownFunctions.GetSpringEquinox),
                new MonthAndDayFunctionCountdown("Easter", AdvancedCountdownFunctions.GetEasterMonthAndDay),
                new MonthAndDayFunctionCountdown("Summer Solstice", AdvancedCountdownFunctions.GetSummerSolstice),
                new MonthAndDayFunctionCountdown("Autumn Equinox", AdvancedCountdownFunctions.GetAutumnEquinox),
                new MonthAndDayFunctionCountdown("Winter Solstice", AdvancedCountdownFunctions.GetWinterSolstice),

                // Hypoannual countdowns
                new HypoannualCountdownWrapper(new WeekdayOfMonthCountdown("Presidential Election Day", 11, IsoDayOfWeek.Tuesday, 1), 2000, 4),
                new HypoannualCountdownWrapper(new WeekdayOfMonthCountdown("Mid-Term Election Day", 11, IsoDayOfWeek.Tuesday, 1), 2002, 4),
                new HypoannualCountdownWrapper(new WeekdayOfMonthCountdown("Summer Olympics (est.)", 7, IsoDayOfWeek.Saturday, 4), 2000, 4),
                new HypoannualCountdownWrapper(new WeekdayOfMonthCountdown("Winter Olympics (est.)", 2, IsoDayOfWeek.Saturday, 2), 2002, 4),
                new HypoannualCountdownWrapper(new FixedDateCountdown("Inauguration Day", 1, 20), 2001, 4),
                new HypoannualCountdownWrapper(new FixedDateCountdown("Start of a Hexadecimal Decade", 1, 1), 2000, 16),

                // Offset countdowns
                new OffsetCountdownWrapper(new WeekdayOfMonthCountdown("Thunder Over Louisville", 5, IsoDayOfWeek.Saturday, 1), Duration.FromDays(-7 * 3)),
                new OffsetCountdownWrapper(new WeekdayOfMonthCountdown("NFL Playoffs", 9, IsoDayOfWeek.Thursday, 1), Duration.FromDays(7 * 18)),
                new OffsetCountdownWrapper(new WeekdayOfMonthCountdown("MLB World Series Start (est.)", 10, IsoDayOfWeek.Tuesday, 2), Duration.FromDays(24)),

                // Once-off countdowns
                new OnceOffCountdown("One Billions Seconds Since My Birth", new LocalDateTime(2026, 5, 17, 19, 48, 0).InZoneLeniently(easternTime)),
                new OnceOffCountdown("Great American Solar Eclipse", new LocalDateTime(2045, 8, 12, 0, 0, 0).InZoneLeniently(easternTime)),
                new OnceOffCountdown("Unix Timestamp Rollover", new LocalDateTime(2038, 1, 19, 3, 14, 7).InZoneLeniently(DateTimeZone.Utc)),
                new OnceOffCountdown("Halley's Comet Returns", new LocalDateTime(2061, 7, 29, 0, 0, 0).InZoneLeniently(easternTime)),

                // Extra countdowns
                new LeapDayCountdown(),
                new NextPowerOf2And10Countdown(DateTimeZoneProviders.Tzdb["America/Denver"], "My Birth", 1994, 9, 8, 16, 2, 0)
            ];
        }

        internal static RealizedCountdown RealizeCountdown(Countdown countdown, ZonedDateTime zonedDateTime)
        {
            return new RealizedCountdown
            {
                Name = countdown.Name(zonedDateTime),
                NextOccurrence = countdown.NextInstance(zonedDateTime),
                PreviousOccurence = countdown.PreviousInstance(zonedDateTime),
            };
        }

        private static string GetCountdownString(RealizedCountdown countdown, ZonedDateTime now)
        {
            var name = countdown.Name;
            var durationUntil = countdown.DurationUntil(now);
            var decibelSecondsUntil = countdown.DecibelSecondsUntil(now);
            var daysText = $": {durationUntil.Days}d ({decibelSecondsUntil:F4}dBs)";
            return $"{name}{daysText}";
        }
    }
}
