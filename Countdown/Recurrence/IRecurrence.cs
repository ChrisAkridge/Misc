using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace Countdown.Recurrence
{
	internal interface IRecurrence
	{
		Instant RescheduleAt(Event oldEvent);
		Instant RescheduleFor(Event oldEvent);
	}
}
