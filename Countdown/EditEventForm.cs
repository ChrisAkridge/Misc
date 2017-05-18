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
	internal partial class EditEventForm : Form
	{
		public Event ResultEvent { get; private set; }

		public EditEventForm()
		{
			InitializeComponent();
		}

		public EditEventForm(Event e)
		{
			InitializeComponent();

			ResultEvent = e;
			TextName.Text = e.Name;
			DTPEndTime.Value = e.EndTime.ToDateTimeUtc().ToLocalTime();
			CheckBoxStartTime.Checked = e.StartTime.HasValue;
			if (CheckBoxStartTime.Checked)
			{
				DTPStartTime.Value = e.StartTime.Value.ToDateTimeUtc().ToLocalTime();
			}

			if (e.Recurrence != null)
			{
				CheckBoxEnableRecurrence.Checked = true;
				if (e.Recurrence is DailyRecurrence)
				{
					RadioDaily.Checked = true;
					NUDDailyRepeat.Value = ((DailyRecurrence)e.Recurrence).RepeatEveryNDays;
				}
				else if (e.Recurrence is WeeklyRecurrence)
				{
					RadioWeekly.Checked = true;
					var weekdays = ((WeeklyRecurrence)e.Recurrence).WeekdayFlags;
					CheckBoxSunday.Checked = weekdays[0];
					CheckBoxMonday.Checked = weekdays[1];
					CheckBoxTuesday.Checked = weekdays[2];
					CheckBoxWednesday.Checked = weekdays[3];
					CheckBoxThursday.Checked = weekdays[4];
					CheckBoxFriday.Checked = weekdays[5];
					CheckBoxSaturday.Checked = weekdays[6];
				}
				else if (e.Recurrence is MonthlyRecurrence)
				{
					var monthlyRecurrence = (MonthlyRecurrence)e.Recurrence;
					if (monthlyRecurrence.DayNumberOrWeekdayNumber)
					{
						RadioSpecificDay.Checked = true;
						NUDMonthlyDayOfMonth.Value = monthlyRecurrence.OccursOnDayNumber;
					}
					else
					{
						RadioSpecificWeekday.Checked = true;
						NUDSpecificWeekdayNumber.Value = monthlyRecurrence.OccursOnNthWeekday;
						ComboWeekday.SelectedIndex = monthlyRecurrence.OccursOnWeekday.IsoWeekdayToInt();
					}
				}
				else if (e.Recurrence is YearlyRecurrence)
				{
					var yearlyRecurrence = (YearlyRecurrence)e.Recurrence;
					DTPYearlyOnDate.Value = new DateTime(DateTime.Now.Year,
						yearlyRecurrence.DayInYear.Month, yearlyRecurrence.DayInYear.Day);
				}
			}
		}

		private void EditEventForm_Load(object sender, EventArgs e)
		{
			SetRecurrenceControlsEnabled(CheckBoxEnableRecurrence.Checked);
		}

		private void CheckBoxStartTime_CheckedChanged(object sender, EventArgs e)
		{
			DTPStartTime.Enabled = CheckBoxStartTime.Checked;
		}

		private void CheckBoxEnableRecurrence_CheckedChanged(object sender, EventArgs e)
		{
			SetRecurrenceControlsEnabled(CheckBoxEnableRecurrence.Checked);
		}

		private void SetRecurrenceControlsEnabled(bool enabled)
		{
			RadioDaily.Enabled = StaticLabelRepeatEvery.Enabled = NUDDailyRepeat.Enabled =
				StaticLabelDays.Enabled = RadioWeekly.Enabled = CheckBoxSunday.Enabled =
				CheckBoxMonday.Enabled = CheckBoxTuesday.Enabled = CheckBoxWednesday.Enabled =
				CheckBoxThursday.Enabled = CheckBoxFriday.Enabled = CheckBoxSaturday.Enabled =
				RadioMonthly.Enabled = RadioSpecificDay.Enabled = NUDMonthlyDayOfMonth.Enabled =
				RadioSpecificWeekday.Enabled = NUDSpecificWeekdayNumber.Enabled =
				LabelOrdinalSuffix.Enabled = ComboWeekday.Enabled = StaticLabelOfTheMonth.Enabled =
				RadioYearly.Enabled = DTPYearlyOnDate.Enabled = enabled;
		}

		private void NUDSpecificWeekdayNumber_ValueChanged(object sender, EventArgs e)
		{
			switch ((int)NUDSpecificWeekdayNumber.Value)
			{
				case 1: LabelOrdinalSuffix.Text = "st"; break;
				case 2: LabelOrdinalSuffix.Text = "nd"; break;
				case 3: LabelOrdinalSuffix.Text = "rd"; break;
				default: LabelOrdinalSuffix.Text = "th"; break;
			}
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			// Perform validations.
			if (!ValidateName()) { return; }
			else if (!ValidateWeekdayCheckboxes()) { return; }
			else if (!ValidateWeekdaySelected()) { return; }
			else if (!ValidateRecurrenceRadioButtonsChecked()) { return; }

			// Retrieve values from the controls.
			var name = TextName.Text;
			var endTime = DTPEndTime.Value;
			var startTime = DTPStartTime.Value;
			var startTimeEnabled = CheckBoxStartTime.Checked;
			var recurrence = GetRecurrenceFromControls();

			// Convert the start and end times to NodaTime Instants.
			// http://stackoverflow.com/a/19462661/2709212
			var zone = DateTimeZoneProviders.Tzdb["America/New_York"];
			var localStartTime = LocalDateTime.FromDateTime(startTime);
			var localEndTime = LocalDateTime.FromDateTime(endTime);
			var zonedStartTime = localStartTime.InZoneLeniently(zone);
			var zonedEndTime = localEndTime.InZoneLeniently(zone);
			var startInstant = zonedStartTime.ToInstant();
			var endInstant = zonedEndTime.ToInstant();

			// Create the resulting event.
			Instant? startTimeNullable;
			if (startTimeEnabled) { startTimeNullable = startInstant; }
			else { startTimeNullable = null; }
			ResultEvent = new Event(name, startTimeNullable, endInstant, recurrence);

			DialogResult = DialogResult.OK;
		}

		private IRecurrence GetRecurrenceFromControls()
		{
			if (!CheckBoxEnableRecurrence.Checked) { return null; }

			if (RadioDaily.Checked)
			{
				int repeatEveryNDays = (int)NUDDailyRepeat.Value;
				return new DailyRecurrence(repeatEveryNDays);
			}
			else if (RadioWeekly.Checked)
			{
				bool sunday = CheckBoxSunday.Checked;
				bool monday = CheckBoxMonday.Checked;
				bool tuesday = CheckBoxTuesday.Checked;
				bool wednesday = CheckBoxWednesday.Checked;
				bool thursday = CheckBoxThursday.Checked;
				bool friday = CheckBoxFriday.Checked;
				bool saturday = CheckBoxSaturday.Checked;
				return new WeeklyRecurrence(new bool[] { sunday, monday, tuesday, wednesday,
					thursday, friday, saturday });
			}
			else if (RadioMonthly.Checked)
			{
				if (RadioSpecificDay.Checked)
				{
					int dayOfMonth = (int)NUDMonthlyDayOfMonth.Value;
					return new MonthlyRecurrence(dayOfMonth);
				}
				else if (RadioSpecificWeekday.Checked)
				{
					int nthWeekday = (int)NUDSpecificWeekdayNumber.Value;
					int weekday = ComboWeekday.SelectedIndex;
					return new MonthlyRecurrence(nthWeekday, weekday.ToIsoWeekday());
				}
				else
				{
					throw new InvalidOperationException();
				}
			}
			else if (RadioYearly.Checked)
			{
				var date = DTPYearlyOnDate.Value;
				return new YearlyRecurrence(date.Month, date.Day);
			}
			else
			{
				throw new InvalidOperationException();
			}
		}

		// Validation methods
		private void ShowValidationMessageBox(string message)
		{
			MessageBox.Show(message, "Countdown", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		private bool ValidateName()
		{
			if (string.IsNullOrEmpty(TextName.Text))
			{
				ShowValidationMessageBox("The event's name must not be blank.");
				return false;
			}
			return true;
		}

		private bool ValidateWeekdayCheckboxes()
		{
			// At least one of the weekday checkboxes have to be checked.
			if (!CheckBoxEnableRecurrence.Checked || !RadioWeekly.Checked) { return true; }

			bool noWeekdaysChecked = !CheckBoxSunday.Checked && !CheckBoxMonday.Checked &&
				!CheckBoxTuesday.Checked && !CheckBoxWednesday.Checked && !CheckBoxThursday.Checked &&
				!CheckBoxFriday.Checked && !CheckBoxSaturday.Checked;
			if (noWeekdaysChecked)
			{
				ShowValidationMessageBox("For weekly recurring event, at least one weekday checkbox needs to be selected.");
				return false;
			}
			return true;
		}

		private bool ValidateWeekdaySelected()
		{
			if (!CheckBoxEnableRecurrence.Checked || !RadioMonthly.Checked || 
				!RadioSpecificWeekday.Checked) { return true; }
			if (ComboWeekday.SelectedIndex < 0)
			{
				ShowValidationMessageBox("For monthly recurring events, select a weekday.");
				return false;
			}
			return true;
		}

		private bool ValidateRecurrenceRadioButtonsChecked()
		{
			if (!CheckBoxEnableRecurrence.Checked) { return true; }
			if (!RadioDaily.Checked && !RadioWeekly.Checked && !RadioMonthly.Checked &&
				!RadioYearly.Checked)
			{
				ShowValidationMessageBox("Please select a recurrence type.");
				return false;
			}

			if (RadioMonthly.Checked)
			{
				if (!RadioSpecificDay.Checked && !RadioSpecificWeekday.Checked)
				{
					ShowValidationMessageBox("Please select a monthly recurrence type.");
					return false;
				}
			}
			return true;
		}
	}
}
