using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Logic
{
    internal sealed class ExtendedDayCultureInfo : IComparable<ExtendedDayCultureInfo>
    {
        public int DayOfWeekNumber { get; set; }
        public int StartingHour { get; set; }
        public int EndingHour { get; set; }
        public string DayCulture { get; set; }
        public string TimeCulture { get; set; }

        public ExtendedDayCultureInfo(int dayOfWeekNumber, int startingHour, int endingHour, string dayCulture, string timeCulture)
        {
            DayOfWeekNumber = dayOfWeekNumber;
            StartingHour = startingHour;
            EndingHour = endingHour;
            DayCulture = dayCulture;
            TimeCulture = timeCulture;
        }

        /// <summary>Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.</summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings:
        /// <list type="table"><listheader><term> Value</term><description> Meaning</description></listheader><item><term> Less than zero</term><description> This instance precedes <paramref name="other" /> in the sort order.</description></item><item><term> Zero</term><description> This instance occurs in the same position in the sort order as <paramref name="other" />.</description></item><item><term> Greater than zero</term><description> This instance follows <paramref name="other" /> in the sort order.</description></item></list></returns>
        public int CompareTo(ExtendedDayCultureInfo? other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }
            if (ReferenceEquals(null, other))
            {
                return 1;
            }
            
            var dayOfWeekNumberComparison = DayOfWeekNumber.CompareTo(other.DayOfWeekNumber);
            return dayOfWeekNumberComparison != 0
                ? dayOfWeekNumberComparison
                : StartingHour.CompareTo(other.StartingHour);
        }
    }
}
