using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Countdown.Recurrence;
using Newtonsoft.Json.Linq;
using NodaTime;

namespace Countdown
{
	internal static class IO
	{
		public static bool SaveFilePathIsValid(out Exception exception)
		{
			var saveFilePath = Properties.Settings.Default.SaveFilePath;
			if (string.IsNullOrEmpty(saveFilePath)) { exception = null; return false; }
			if (File.Exists(saveFilePath)) { exception = null; return true; }

			try
			{
				// Try to create the file if it doesn't exist.
				File.Create(saveFilePath);

				// Now delete it, since we know it can be made.
				File.Delete(saveFilePath);
			}
			catch (Exception ex)
			{
				exception = ex;
				return false;
			}

			exception = null;
			return true;
		}

		public static void Load()
		{
			var path = Properties.Settings.Default.SaveFilePath;
			if (!File.Exists(path))
			{
				CreateInitialState();
				return;
			}
			
			string json = File.ReadAllText(path);
			if (string.IsNullOrEmpty(json))
			{
				CreateInitialState();
				return;
			}
			
			DeserializeStateObjects(JObject.Parse(json));
		}

		private static void CreateInitialState()
		{
			State.Settings = new Settings(true, TimeLeftForm.Default, 2);
			State.Events = new List<Event>();
			Save();
		}

		public static void Save()
		{
			File.WriteAllText(Properties.Settings.Default.SaveFilePath,
				Serialize());
		}

		private static string Serialize() => JObject.FromObject(GetStateObjects()).ToString();

		private static object GetStateObjects()
		{
			var eventsObjects = new List<object>(State.Events.Count);

			foreach (var e in State.Events)
			{
				eventsObjects.Add(GetEventObject(e));
			}

			return new
			{
				settings = GetSettingsObjects(State.Settings),
				events = eventsObjects
			};
		}

		private static void DeserializeStateObjects(JObject obj)
		{
			State.Settings = DeserializeSettings((JObject)obj["settings"]);
			State.Events = new List<Event>();

			foreach (var e in (JArray)obj["events"])
			{
				State.Events.Add(DeserializeEvent((JObject)e));
			}
		}

		private static object GetSettingsObjects(Settings settings)
		{
			return new
			{
				notificationsEnabled = settings.NotificationsEnabled,
				notificationsTimeLeftForm = settings.NotificationsTimeLeftForm,
				notificationsDecimalPlaces = settings.NotificationDecimalPlaces
			};
		}

		private static Settings DeserializeSettings(JObject obj)
		{
			return new Settings(
				(bool)obj["notificationsEnabled"],
				(TimeLeftForm)(int)obj["notificationsTimeLeftForm"],
				(int)obj["notificationsDecimalPlaces"]);
		}

		private static object GetRecurrenceObject(IRecurrence recurrence)
		{
			if (recurrence == null) { return null; }
			else if (recurrence is DailyRecurrence)
			{
				return GetDailyRecurrenceObject((DailyRecurrence)recurrence);
			}
			else if (recurrence is WeeklyRecurrence)
			{
				return GetWeeklyRecurrenceObject((WeeklyRecurrence)recurrence);
			}
			else if (recurrence is MonthlyRecurrence)
			{
				return GetMonthlyRecurrenceObject((MonthlyRecurrence)recurrence);
			}
			else if (recurrence is YearlyRecurrence)
			{
				return GetYearlyRecurrenceObject((YearlyRecurrence)recurrence);
			}
			throw new InvalidOperationException();
		}

		private static IRecurrence DeserializeRecurrence(JObject obj, RecurrenceType type)
		{
			switch (type)
			{
				case RecurrenceType.DailyRecurrence:
					return DeserializeDailyRecurrency(obj);
				case RecurrenceType.WeeklyRecurrence:
					return DeserializeWeeklyRecurrence(obj);
				case RecurrenceType.MonthlyRecurrence:
					return DeserializeMonthlyRecurrence(obj);
				case RecurrenceType.YearlyRecurrence:
					return DeserializeYearlyRecurrence(obj);
				default:
					throw new InvalidOperationException();
			}
		}

		private static object GetDailyRecurrenceObject(DailyRecurrence recurrence)
		{
			return new
			{
				repeatEveryNDays = recurrence.RepeatEveryNDays
			};
		}

		private static DailyRecurrence DeserializeDailyRecurrency(JObject obj)
		{
			return new DailyRecurrence((int)obj["repeatEveryNDays"]);
		}

		private static object GetWeeklyRecurrenceObject(WeeklyRecurrence recurrence)
		{
			return new
			{
				weekdayFlags = recurrence.GetWeekdayFlagsString()
			};
		}

		private static WeeklyRecurrence DeserializeWeeklyRecurrence(JObject obj)
		{
			string weekdayFlags = (string)obj["weekdayFlags"];
			return new WeeklyRecurrence(weekdayFlags.Select(c => (c == 'E')));
		}

		private static object GetMonthlyRecurrenceObject(MonthlyRecurrence recurrence)
		{
			return new
			{
				occursOnDayNumber = (recurrence.DayNumberOrWeekdayNumber) ? recurrence.OccursOnDayNumber : -1,
				occursOnNthWeekday = (!recurrence.DayNumberOrWeekdayNumber) ? recurrence.OccursOnNthWeekday : -1,
				weekday = (!recurrence.DayNumberOrWeekdayNumber) ? recurrence.OccursOnWeekday.IsoWeekdayToInt() : -1
			};
		}

		private static MonthlyRecurrence DeserializeMonthlyRecurrence(JObject obj)
		{
			int dayNumber = (int)obj["occursOnDayNumber"];
			int nthWeekday = (int)obj["occursOnNthWeekday"];
			var weekday = ((int)obj["weekday"]).ToIsoWeekday();

			if (dayNumber == -1)
			{
				return new MonthlyRecurrence(nthWeekday, weekday);
			}
			else
			{
				return new MonthlyRecurrence(dayNumber);
			}
		}

		private static object GetYearlyRecurrenceObject(YearlyRecurrence recurrence)
		{
			return new
			{
				month = recurrence.DayInYear.Month,
				day = recurrence.DayInYear.Day
			};
		}

		private static YearlyRecurrence DeserializeYearlyRecurrence(JObject obj)
		{
			return new YearlyRecurrence((int)obj["month"], (int)obj["day"]);
		}

		private static object GetEventObject(Event e)
		{
			return new
			{
				name = e.Name,
				startTimeEnabled = e.StartTime.HasValue,
				startTime = (e.StartTime.HasValue) ? e.StartTime.Value.ToUnixTimeSeconds() : -1L,
				endTime = e.EndTime.ToUnixTimeSeconds(),
				recurrenceEnabled = e.Recurrence != null,
				recurrenceType = (e.Recurrence != null) ? (int)e.Recurrence.GetRecurrenceType() : -1,
				recurrence = GetRecurrenceObject(e.Recurrence)
			};
		}

		private static Event DeserializeEvent(JObject obj)
		{
			string name = (string)obj["name"];
			bool startTimeEnabled = (bool)obj["startTimeEnabled"];
			Instant? startTime;
			if (startTimeEnabled) { startTime = Instant.FromUnixTimeSeconds((long)obj["startTime"]); }
			else { startTime = null; }
			bool recurrenceEnabled = (bool)obj["recurrenceEnabled"];
			var endTime = Instant.FromUnixTimeSeconds((long)obj["endTime"]);
			IRecurrence recurrence = null;
			if (recurrenceEnabled)
			{
				recurrence = DeserializeRecurrence((JObject)obj["recurrence"], 
				(RecurrenceType)(int)obj["recurrenceType"]);
			}

			return new Event(name, startTime, endTime, (recurrenceEnabled) ? recurrence : null);
		}
	}
}
