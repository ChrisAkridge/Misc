using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.LunaGalatea.Logic;
using LewisFam.Extensions;
using NodaTime;

namespace Celarix.JustForFun.LunaGalatea.Providers
{
    public sealed class TimeDisplayProvider : IProvider<IReadOnlyList<string>>
    {
        private readonly Dictionary<string, DateTimeZone> timeZones;
        private readonly List<string> buffer;
        private readonly DateTimeZone easternTime;

        public bool UseMonospaceFont => true;

        public TimeDisplayProvider()
        {
            timeZones = new Dictionary<string, DateTimeZone>
            {
                { "UTC-12", DateTimeZoneProviders.Tzdb["Etc/GMT+12"]},
                { "Hawaii", DateTimeZoneProviders.Tzdb["Pacific/Honolulu"]},
                { "Alaska", DateTimeZoneProviders.Tzdb["America/Juneau"]},
                { "Pacific", DateTimeZoneProviders.Tzdb["America/Los_Angeles"]},
                { "Eastern", DateTimeZoneProviders.Tzdb["America/New_York"]},
                { "Atlantic", DateTimeZoneProviders.Tzdb["America/Halifax"]},
                { "Mary's Harbour", DateTimeZoneProviders.Tzdb["Canada/Newfoundland"]},
                { "UTC", DateTimeZoneProviders.Tzdb["Etc/UTC"]},
                { "Japan", DateTimeZoneProviders.Tzdb["Asia/Tokyo"]},
                { "UTC+14", DateTimeZoneProviders.Tzdb["Pacific/Kiritimati"] },
            };

            buffer = new List<string>();
            easternTime = DateTimeZoneProviders.Tzdb["America/New_York"];
        }
        
        public IReadOnlyList<string> GetDisplayObject()
        {
            const int timeZonesPerLine = 2;
            buffer.Clear();
            
            // https://stackoverflow.com/a/21031514
            var now = SystemClock.Instance.GetCurrentInstant();

            buffer.AddRange(GetTimeZoneTable(now, timeZonesPerLine));

            var extendedDate = new CelarianExtendedDateTime(DateTimeOffset.UtcNow);
            var nextCultureStartTime = extendedDate.GetTimeOfNextCulture().ToOffset(DateTimeOffset.Now.Offset);
            var timeUntilNextCulture = nextCultureStartTime - DateTimeOffset.Now;
            var timeString =
                $"{timeUntilNextCulture.Hours}h{timeUntilNextCulture.Minutes}m{timeUntilNextCulture.Seconds}s";
            
            buffer.Add($"Extended: {extendedDate.ToAmericanLongDateStyleString()}");
            buffer.Add($"({extendedDate.GetDayCulture()}, {extendedDate.GetTimeCulture()})");
            buffer.Add($"(until {nextCultureStartTime:yyyy-MM-dd hh:mm tt}, in {timeString})");
            return buffer;
        }

        private string GetTimeZoneNowDisplay(DateTimeZone zone, string zoneName, Instant now)
        {
            var nowDayOfWeekInEastern = now.InZone(easternTime).DayOfWeek;
            var zonedDateTime = now.InZone(zone);
            var nowDayOfWeekInZone = zonedDateTime.DayOfWeek;

            return nowDayOfWeekInZone == nowDayOfWeekInEastern
                ? $"{zoneName} {zonedDateTime:hh:mm:ss tt}"
                : $"{zoneName} {nowDayOfWeekInZone.ToString().Substring(0, 3)} {zonedDateTime:hh:mm:ss tt}";
        }

        private IEnumerable<string> GetTimeZoneTable(Instant now, int timeZonesPerLine)
        {
            var timeZoneDisplays = timeZones.Select(kvp => GetTimeZoneNowDisplay(kvp.Value, kvp.Key, now)).ToArray();
            var maximumDisplayLength = timeZoneDisplays.Select(d => d.Length).Max() + 1;
            var paddedDisplays = timeZoneDisplays.Select(d => d.PadRight(maximumDisplayLength));
            var displayBatches = paddedDisplays.Batch(timeZonesPerLine);
            return displayBatches.Select(b => string.Join("", b));
        }
    }
}
