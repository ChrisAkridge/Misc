using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Countdown.Recurrence;
using NodaTime;

namespace Countdown
{
	public partial class MainForm : Form
	{
		private TimeLeftForm timeLeftForm = TimeLeftForm.RemainingSeconds;
		private int decimalPlaces = 2;
		private List<Event> eventsToRemove = new List<Event>();

		public MainForm()
		{
			InitializeComponent();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			// Check if the save file path is valid.
			Exception ex = null;
			if (!IO.SaveFilePathIsValid(out ex))
			{
				new SaveFilePathNotValidForm().ShowDialog();
			}

			IO.Load();
		}

		private void ButtonAddEvent_Click(object sender, EventArgs e)
		{
			var editEventForm = new EditEventForm();
			if (editEventForm.ShowDialog() == DialogResult.OK)
			{
				Event resultEvent = editEventForm.ResultEvent;
				State.Events.Add(resultEvent);
				State.Events = State.Events.OrderBy(ev => ev.EndTime.ToUnixTimeMilliseconds()).ToList();
				IO.Save();
			}
		}

		private void UpdateTimer_Tick(object sender, EventArgs e)
		{
			var now = SystemClock.Instance.GetCurrentInstant();
			int selectedIndex = ListEvents.SelectedIndex;
			ListEvents.Items.Clear();
			eventsToRemove.Clear();

			foreach (Event ev in State.Events)
			{
				if (now < ev.EndTime)
				{
					ListEvents.Items.Add(ev.FormatRemainingTime(now, timeLeftForm, decimalPlaces));
				}
				else
				{
					if (ev.Recurrence == null)
					{
						eventsToRemove.Add(ev);
					}
					else
					{
						// An event will be rescheduled if the current time is +/- 2 seconds around
						// the reschedule time.
						var rescheduleAt = ev.Recurrence.RescheduleAt(ev);
						var rescheduleRangeStart = rescheduleAt - Duration.FromSeconds(2);
						var rescheduleRangeEnd = rescheduleAt + Duration.FromSeconds(2);
						if (now >= rescheduleRangeStart && now <= rescheduleRangeEnd)
						{
							// Reschedule the event.
							var rescheduleFor = ev.Recurrence.RescheduleFor(ev);
							var newEvent = new Event(ev.Name, rescheduleAt, rescheduleFor, ev.Recurrence);
						}
					}
				}
			}

			ListEvents.SelectedIndex = selectedIndex;
			foreach (var ev in eventsToRemove) { State.Events.Remove(ev); }
		}

		private void ButtonChangeForm_Click(object sender, EventArgs e)
		{
			if (timeLeftForm == TimeLeftForm.XKCD1017Equation) { timeLeftForm = TimeLeftForm.Default; }
			else { timeLeftForm = (TimeLeftForm)(((int)timeLeftForm) + 1); }
		}

		private void ListEvents_DoubleClick(object sender, EventArgs e)
		{
			new DetailWindow(State.Events[ListEvents.SelectedIndex]).ShowDialog();
		}

		private void ButtonSubtractDecimalPlace_Click(object sender, EventArgs e)
		{
			decimalPlaces--;
			LabelDecimalPlaces.Text = $"{decimalPlaces} decimal places";
		}

		private void ButtonAddDecimalPlace_Click(object sender, EventArgs e)
		{
			decimalPlaces++;
			LabelDecimalPlaces.Text = $"{decimalPlaces} decimal places";
		}
	}
}
