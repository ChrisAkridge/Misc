using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NodaTime;

namespace QuickCountdown
{
	public partial class MainForm : Form
	{
		private ZonedDateTime countdownDateTime;
		private bool dateValid;

		public MainForm()
		{
			this.InitializeComponent();

			Instant now = SystemClock.Instance.Now;
			DateTimeZone dtz = DateTimeZoneProviders.Bcl.GetSystemDefault();
			this.countdownDateTime = now.InZone(dtz);

			DateTime bclNow = DateTime.Now;
			this.DTPCountdownDate.Value = bclNow;
			this.DTPCountdownTime.Value = bclNow;
		}

		private void UpdateCountdownDateTime()
		{
			// Grab the date and time out of the DTPs and mash them together
			DateTime dtpDateValue = this.DTPCountdownDate.Value;
			DateTime dtpTimeValue = this.DTPCountdownTime.Value;
			DateTime bclNewCountdownDate = new DateTime(dtpDateValue.Year, dtpDateValue.Month, dtpDateValue.Day, dtpTimeValue.Hour, dtpTimeValue.Minute, dtpTimeValue.Second);
			DateTime bclUtcNewCountdownDate = bclNewCountdownDate.ToUniversalTime();

			// Convert the BCL DateTime into a NodaTime ZonedDateTime and an Instant
			Instant newCountdownInstant = Instant.FromDateTimeUtc(bclUtcNewCountdownDate);
			DateTimeZone dtz = DateTimeZoneProviders.Bcl.GetSystemDefault();
			this.countdownDateTime = newCountdownInstant.InZone(dtz);

			// Set the date's validity if it is after the current time
			this.dateValid = SystemClock.Instance.Now < newCountdownInstant;
		}

		private void Update()
		{
			// Get the current instant in UTC
			Instant nowInstant = SystemClock.Instance.Now;
			LocalDateTime utcDateTime = nowInstant.InUtc().LocalDateTime;
			
			// Convert the countdown date to UTC
			
		}

		private void UpdateRemainingLabel()
		{

		}

		private void DTPCountdownDate_ValueChanged(object sender, EventArgs e)
		{
			this.UpdateCountdownDateTime();
		}

		private void DTPCountdownTime_ValueChanged(object sender, EventArgs e)
		{
			this.UpdateCountdownDateTime();
		}
	}
}
