using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;
using NodaTime.Extensions;

namespace Celarix.JustForFun.LunaGalatea.Providers
{
    public sealed class TimeVisualizerProvider : IProvider<IReadOnlyList<string>>
    {
        private const int BarWidth = 62;

        private readonly DateTimeZone easternTime = DateTimeZoneProviders.Tzdb["America/New_York"];
        private readonly CalendarSystem isoCalendar = CalendarSystem.Iso;
        private readonly List<string> buffer = new List<string>();

        public bool UseMonospaceFont => true;

        public IReadOnlyList<string> GetDisplayObject()
        {
            buffer.Clear();
            
            var now = SystemClock.Instance.GetCurrentInstant();
            var zonedNow = now.InZone(easternTime);
            
            AddRangeToBuffer("Year",
                now,
                new LocalDateTime(zonedNow.Year, 1, 1, 0, 0, 0).InZoneLeniently(easternTime),
                new LocalDateTime(zonedNow.Year, 12, 31, 23, 59, 59).InZoneLeniently(easternTime));
            AddRangeToBuffer("Month",
                now,
                new LocalDateTime(zonedNow.Year, zonedNow.Month, 1, 0, 0, 0).InZoneLeniently(easternTime),
                new LocalDateTime(zonedNow.Year, zonedNow.Month, isoCalendar.GetDaysInMonth(zonedNow.Year, zonedNow.Month), 23, 59, 59).InZoneLeniently(easternTime));
            AddRangeToBuffer("Week", now, GetStartOfWeek(zonedNow), GetEndOfWeek(zonedNow));
            AddRangeToBuffer("Day",
                now,
                zonedNow.Date.AtStartOfDayInZone(easternTime),
                zonedNow.Date.At(new LocalTime(23, 59, 59)).InZoneStrictly(easternTime));
            AddRangeToBuffer("Hour",
                now,
                zonedNow.Date.At(new LocalTime(zonedNow.Hour, 0, 0)).InZoneStrictly(easternTime),
                zonedNow.Date.At(new LocalTime(zonedNow.Hour, 59, 59)).InZoneStrictly(easternTime));
            AddRangeToBuffer("Minute",
                now,
                zonedNow.Date.At(new LocalTime(zonedNow.Hour, zonedNow.Minute, 0)).InZoneStrictly(easternTime),
                zonedNow.Date.At(new LocalTime(zonedNow.Hour, zonedNow.Minute, 59)).InZoneStrictly(easternTime));
            
            return buffer;
        }

        private void AddRangeToBuffer(string name, Instant now, ZonedDateTime start, ZonedDateTime end)
        {
            end += Duration.FromSeconds(1d);
            
            var elapsedSeconds = (int)((now - start.ToInstant()).TotalSeconds);
            var totalSeconds = (int)((end.ToInstant() - start.ToInstant()).TotalSeconds);
            
            buffer.Add(GetProgressStatusString(name, elapsedSeconds, totalSeconds, BarWidth));
            buffer.Add(GetProgressBarString(elapsedSeconds, totalSeconds, BarWidth));
        }

        private static string GetProgressStatusString(string name, int current, int max, int totalWidth)
        {
            var percentProgress = (float)current / max;
            var percentString = $"{percentProgress * 100:F2}%";
            var currentMaxString = $"{current} / {max}";

            var combinedMinimumLength = name.Length + percentString.Length + currentMaxString.Length;
            if (combinedMinimumLength > totalWidth)
            {
                throw new ArgumentOutOfRangeException(nameof(totalWidth),
                    $"Progress components too wide (need {combinedMinimumLength} characters, got {totalWidth})");
            }

            var nameStringLastIndex = name.Length - 1;
            var currentMaxStringFirstIndex = totalWidth - currentMaxString.Length - 1;
            var gapWidth = currentMaxStringFirstIndex - nameStringLastIndex;
            var halfGapWidth = gapWidth / 2;
            var percentDotIndex = totalWidth / 2;
            var digitsBeforeDot = (percentProgress * 100f) < 10 ? 1 : 2;
            var percentStringStartIndex = percentDotIndex - digitsBeforeDot;
            var leftGapWidth = percentStringStartIndex - nameStringLastIndex;
            var rightGapWidth = currentMaxStringFirstIndex - (percentStringStartIndex + percentString.Length);

            return $"{name}{new string(' ', leftGapWidth)}{percentString}{new string(' ', rightGapWidth)}{currentMaxString}";
        }
        
        private static string GetProgressBarString(int current, int max, int barWidth)
        {
            const char FullSymbol = '$';
            const char EmptySymbol = '.';
            
            if (barWidth < 2) { throw new ArgumentOutOfRangeException(nameof(barWidth)); }

            var progressSymbolBarWidth = barWidth - 2;
            var stepsPerSymbol = (float)max / progressSymbolBarWidth;
            var symbolsFilled = current / stepsPerSymbol;
            var partialSymbolProgress = symbolsFilled - Math.Truncate(symbolsFilled);
            var partialSymbolDigit = (int)Math.Floor(partialSymbolProgress * 10f);

            var fullyFilled = new string(FullSymbol, (int)symbolsFilled);
            var notFilled = new string(EmptySymbol, progressSymbolBarWidth - (int)symbolsFilled - 1);
            var partiallyFilled = (char)(partialSymbolDigit + 0x30);

            return $"[{fullyFilled}{partiallyFilled}{notFilled}]";
        }

        private ZonedDateTime GetStartOfWeek(ZonedDateTime zonedNow)
        {
            var zonedDateTime = zonedNow;
            while (zonedDateTime.DayOfWeek != IsoDayOfWeek.Monday)
            {
                zonedDateTime -= Duration.FromDays(1d);
            }
            return zonedDateTime.Date.AtStartOfDayInZone(easternTime);
        }

        private ZonedDateTime GetEndOfWeek(ZonedDateTime zonedNow)
        {
            var zonedDateTime = zonedNow;
            while (zonedDateTime.DayOfWeek != IsoDayOfWeek.Sunday)
            {
                zonedDateTime += Duration.FromDays(1d);
            }
            return zonedDateTime.Date.At(new LocalTime(23, 59, 59)).InZoneStrictly(easternTime);
        }
    }
}
