using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.LunaGalatea.Logic;
using NodaTime;

namespace Celarix.JustForFun.LunaGalatea.Providers
{
    public sealed class TimeDisplayProvider : IProvider<IReadOnlyList<string>>
    {
        private readonly Dictionary<string, DateTimeZone> timeZones;
        private readonly List<string> buffer;

        public TimeDisplayProvider()
        {
            timeZones = new Dictionary<string, DateTimeZone>
            {
                { "UTC-12", DateTimeZoneProviders.Tzdb["Etc/GMT_12"]},
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
        }
        
        public IReadOnlyList<string> GetDisplayObject()
        {
            buffer.Clear();
            
            // https://stackoverflow.com/a/21031514
            var now = SystemClock.Instance.GetCurrentInstant();

            foreach (var (name, timeZone) in timeZones)
            {
                var zdt = now.InZone(timeZone);
                buffer.Add($"{name}: {zdt:ddd yyyy-MM-dd hh:mm:ss tt}");
            }

            var extendedDate = new CelarianExtendedDateTime(DateTimeOffset.Now);
            buffer.Add($"Extended: {extendedDate.ToAmericanLongDateStyleString()}");
            return buffer;
        }
    }
}
