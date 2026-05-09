using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.VioletFacet.ScheduledHibernator
{
    internal class Config
    {
        public TimeOnly Sunday { get; set; }
        public TimeOnly Monday { get; set; }
        public TimeOnly Tuesday { get; set; }
        public TimeOnly Wednesday { get; set; }
        public TimeOnly Thursday { get; set; }
        public TimeOnly Friday { get; set; }
        public TimeOnly Saturday { get; set; }

        public string WatchdogExecutablePath { get; set; }

        public IEnumerable<TimeOnly> GetTimes()
        {
            return new[] { Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday };
        }

        public TimeOnly GetHibernationTimeForDay(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Sunday => Sunday,
                DayOfWeek.Monday => Monday,
                DayOfWeek.Tuesday => Tuesday,
                DayOfWeek.Wednesday => Wednesday,
                DayOfWeek.Thursday => Thursday,
                DayOfWeek.Friday => Friday,
                DayOfWeek.Saturday => Saturday,
                _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek), $"Invalid day of week: {dayOfWeek}")
            };
        }
    }
}
