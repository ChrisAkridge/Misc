using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Countdown
{
	internal static class State
	{
		public static Settings Settings { get; set; }
		public static List<Event> Events { get; set; }
	}

	internal sealed class Settings
	{
		public bool NotificationsEnabled { get; }
		public TimeLeftForm NotificationsTimeLeftForm { get; }
		public int NotificationDecimalPlaces { get; }

		public Settings(bool notificationsEnabled, TimeLeftForm notificationsTimeLeftForm,
			int notificationDecimalPlaces)
		{
			NotificationsEnabled = notificationsEnabled;
			NotificationsTimeLeftForm = notificationsTimeLeftForm;
			NotificationDecimalPlaces = notificationDecimalPlaces;
		}
	}
}
