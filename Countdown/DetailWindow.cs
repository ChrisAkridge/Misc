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

namespace Countdown
{
	internal partial class DetailWindow : Form
	{
		private Event ev;
		private int decimalPlaces = 2;

		public DetailWindow(Event e)
		{
			InitializeComponent();
			ev = e;
			LabelEventName.Text = e.Name;
			LabelFormsEventName.Text = e.Name;
			Text = e.Name;
		}

		private void UpdateTimer_Tick(object sender, EventArgs e)
		{
			Instant now = SystemClock.Instance.GetCurrentInstant();
			string defaultForm = ev.FormatRemainingTime(now, TimeLeftForm.Default, 0, false);
			LabelCountdownInfo.Text = defaultForm;

			if (ev.StartTime.HasValue)
			{
				long period = ev.EndTime.ToUnixTimeMilliseconds() - ev.StartTime.Value.ToUnixTimeMilliseconds();
				long elapsedPeriod = now.ToUnixTimeMilliseconds() - ev.StartTime.Value.ToUnixTimeMilliseconds();
				double progress = (double)elapsedPeriod / period;
				LabelPercentage.Text = (progress * 100d).ToString($"F{decimalPlaces}") + "%";
				Progress.Value = (int)(progress * 100d);
				LabelXKCD1017.Text = "XKCD 1017: " +
					ev.FormatRemainingTime(now, TimeLeftForm.XKCD1017Equation, 2, false);
			}

			LabelDefaultForm.Text = "Default: " + defaultForm;
			LabelRemainingSeconds.Text = "Remaining Seconds: " + 
				ev.FormatRemainingTime(now, TimeLeftForm.RemainingSeconds, decimalPlaces, false);
			LabelRemainingMinutes.Text = "Remaining Minutes: " +
				ev.FormatRemainingTime(now, TimeLeftForm.RemainingMinutes, decimalPlaces, false);
			LabelRemainingHours.Text = "Remaining Hours: " +
				ev.FormatRemainingTime(now, TimeLeftForm.RemainingHours, decimalPlaces, false);
			LabelRemainingDays.Text = "Remaining Days: "
				+ ev.FormatRemainingTime(now, TimeLeftForm.RemainingDays, decimalPlaces, false);
			LabelRemainingWeeks.Text = "Remaining Weeks: " +
				ev.FormatRemainingTime(now, TimeLeftForm.RemainingWeeks, decimalPlaces, false);
			LabelRemainingYears.Text = "Remaining Years: " +
				ev.FormatRemainingTime(now, TimeLeftForm.RemainingYears, decimalPlaces, false);
			LabelDecibelsSeconds.Text = "Decibel-Seconds: " +
				ev.FormatRemainingTime(now, TimeLeftForm.DecibelsSeconds, decimalPlaces, false);
		}

		private void ButtonSubtractDecimalPlace_Click(object sender, EventArgs e)
		{
			decimalPlaces--;
			LabelDecimalPlaces.Text = $"{decimalPlaces} decimal place(s)";
		}

		private void ButtonAddDecimalPlace_Click(object sender, EventArgs e)
		{
			decimalPlaces++;
			LabelDecimalPlaces.Text = $"{decimalPlaces} decimal place(s)";
		}

		private void ButtonEditEvent_Click(object sender, EventArgs e)
		{
			int oldIndex = State.Events.IndexOf(ev);
			var editForm = new EditEventForm(ev);
			if (editForm.ShowDialog() == DialogResult.OK)
			{
				State.Events[oldIndex] = editForm.ResultEvent;
				State.Events = State.Events.OrderBy(ev => ev.EndTime.ToUnixTimeSeconds()).ToList();
			}
		}
	}
}
