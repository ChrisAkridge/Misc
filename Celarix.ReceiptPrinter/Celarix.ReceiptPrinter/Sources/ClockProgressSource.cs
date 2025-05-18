using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celarix.ReceiptPrinter.Utilities;

namespace Celarix.ReceiptPrinter.Sources
{
    public sealed class ClockProgressSource
    {
        private const int MaxColumns = 48;

        public static string GetClockProgressForDate(DateOnly date)
        {
            var dayNumberOfWeek = date.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)date.DayOfWeek - 1;
            const int DaysInWeek = 7;   // hope this doesn't change
            var weekPortion = (double)dayNumberOfWeek / DaysInWeek;

            var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            var monthPortion = (double)date.Day / daysInMonth;

            var daysInYear = DateTime.IsLeapYear(date.Year) ? 366 : 365;
            var yearPortion = (double)date.DayOfYear / daysInYear;

            var startOfCurrentDecade = (date.Year / 10) * 10;
            var daysInCurrentDecade = Enumerable.Range(startOfCurrentDecade, 10)
                .Select(year => DateTime.IsLeapYear(year) ? 366 : 365)
                .Sum();
            var dayNumberOfDecade = Enumerable.Range(startOfCurrentDecade, date.Year - startOfCurrentDecade)
                .Select(year => DateTime.IsLeapYear(year) ? 366 : 365)
                .Sum() + date.DayOfYear;
            var decadePortion = (double)dayNumberOfDecade / daysInCurrentDecade;

            var startOfCurrentCentury = (date.Year / 100) * 100;
            var daysInCurrentCentury = startOfCurrentCentury % 400 == 0
                ? (25 * 366) + (75 * 365)
                : (24 * 366) + (76 * 366);
            var dayNumberOfCentury = Enumerable.Range(startOfCurrentCentury, date.Year - startOfCurrentCentury)
                .Select(year => DateTime.IsLeapYear(year) ? 366 : 365)
                .Sum() + date.DayOfYear;
            var centuryPortion = (double)dayNumberOfCentury / daysInCurrentCentury;

            // If you're running this code in 3000, sorry
            const int DaysInCurrentMillennium = (243 * 366) + (757 * 365);
            const int StartOfCurrentMillennium = 2000;
            var daysNumberOfMillennium = Enumerable.Range(StartOfCurrentMillennium, date.Year - 2000)
                .Select(year => DateTime.IsLeapYear(year) ? 366 : 365)
                .Sum() + date.DayOfYear;
            var millenniumPortion = (double)daysNumberOfMillennium / DaysInCurrentMillennium;

            var progressBuilder = new StringBuilder();
            progressBuilder.AppendLine($"Week: {dayNumberOfWeek} of 7 ({weekPortion * 100:F2}%)");
            progressBuilder.AppendLine(GetProgress(dayNumberOfWeek, DaysInWeek, MaxColumns));
            progressBuilder.AppendLine();
            progressBuilder.AppendLine($"Month: {date.Day} of {daysInMonth} ({monthPortion * 100:F2}%)");
            progressBuilder.AppendLine(GetProgress(date.Day, daysInMonth, MaxColumns));
            progressBuilder.AppendLine();
            progressBuilder.AppendLine($"Year: {date.DayOfYear} of {daysInYear} ({yearPortion * 100:F2}%)");
            progressBuilder.AppendLine(GetProgress(date.DayOfYear, daysInYear, MaxColumns));
            progressBuilder.AppendLine();
            progressBuilder.AppendLine($"Decade: {dayNumberOfDecade} of {daysInCurrentDecade:#,###} ({decadePortion * 100:F2}%)");
            progressBuilder.AppendLine(GetProgress(dayNumberOfDecade, daysInCurrentDecade, MaxColumns));
            progressBuilder.AppendLine();
            progressBuilder.AppendLine($"Century: {dayNumberOfCentury} of {daysInCurrentCentury:#,###} ({centuryPortion * 100:F2}%)");
            progressBuilder.AppendLine(GetProgress(dayNumberOfCentury, daysInCurrentCentury, MaxColumns));
            progressBuilder.AppendLine();
            progressBuilder.AppendLine($"Millennium: {daysNumberOfMillennium} of {DaysInCurrentMillennium:#,###} ({millenniumPortion * 100:F2}%)");
            progressBuilder.AppendLine(GetProgress(daysNumberOfMillennium, DaysInCurrentMillennium, MaxColumns));

            return progressBuilder.ToString();
        }
    }
}
