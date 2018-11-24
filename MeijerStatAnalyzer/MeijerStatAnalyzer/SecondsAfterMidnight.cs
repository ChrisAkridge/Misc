using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeijerStatAnalyzer
{
	public struct SecondsAfterMidnight : IComparable<SecondsAfterMidnight>
	{
		public static readonly SecondsAfterMidnight Midnight = new SecondsAfterMidnight(0);

		public int Seconds { get; }

		public SecondsAfterMidnight(int seconds) => Seconds = seconds;

		public SecondsAfterMidnight(TimeSpan span) => Seconds = (int)span.TotalSeconds;

		public SecondsAfterMidnight(DateTime dateTime) : this(dateTime.Hour, dateTime.Minute, dateTime.Second)
		{
		}

		public SecondsAfterMidnight(int hours, int minutes, int seconds)
		{
			Seconds = hours * 3600;
			Seconds += minutes * 60;
			Seconds += seconds;
		}

		public DateTime ToDateTime()
		{
			int year = DateTime.Now.Year;
			int month = DateTime.Now.Month;
			int day = DateTime.Now.Day;
			var todayMidnight = new DateTime(year, month, day);
			return todayMidnight + TimeSpan.FromSeconds(Seconds);
		}

		public string ToTimeString() => ToDateTime().ToLongTimeString();

		public override string ToString() => $"{ToDateTime().ToShortTimeString()}";

		public override int GetHashCode() => Seconds;

		public override bool Equals(object obj)
		{
			if (!(obj is SecondsAfterMidnight)) { return false; }
			else
			{
				var that = (SecondsAfterMidnight)obj;
				return Seconds == that.Seconds;
			}
		}

		public int CompareTo(SecondsAfterMidnight other) => Seconds.CompareTo(other.Seconds);

		public static bool operator ==(SecondsAfterMidnight a, SecondsAfterMidnight b) => a.Seconds == b.Seconds;
		public static bool operator !=(SecondsAfterMidnight a, SecondsAfterMidnight b) => a.Seconds != b.Seconds;
		public static bool operator <(SecondsAfterMidnight a, SecondsAfterMidnight b) => a.Seconds < b.Seconds;
		public static bool operator >(SecondsAfterMidnight a, SecondsAfterMidnight b) => a.Seconds > b.Seconds;
		public static bool operator <=(SecondsAfterMidnight a, SecondsAfterMidnight b) => a.Seconds <= b.Seconds;
		public static bool operator >=(SecondsAfterMidnight a, SecondsAfterMidnight b) => a.Seconds >= b.Seconds;
	}
}
