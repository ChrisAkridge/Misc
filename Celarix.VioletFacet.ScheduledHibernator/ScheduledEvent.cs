using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.VioletFacet.ScheduledHibernator
{
    internal class ScheduledEvent
    {
        public DateTimeOffset EventTime { get; set; }
        public EventType EventType { get; set; }

        public bool EventPassed(DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset >= EventTime;
        }
    }

    internal enum EventType
    {
        Hibernate,
        SaveSettings
    }
}
