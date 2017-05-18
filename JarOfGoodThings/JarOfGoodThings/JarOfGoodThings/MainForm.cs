using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JarOfGoodThings
{
	/// <summary>
	/// The main form for the application. The user can input good things into the jar in this form.
	/// </summary>
	public partial class MainForm : Form
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MainForm"/> class. 
		/// </summary>
		public MainForm()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Handles the Load event on <see cref="MainForm"/>. Initializes the jar, changes the
		/// form's labels, and checks if it's time to notify the user that they can open the jar.
		/// </summary>
		/// <param name="sender">The object that this event was called on.</param>
		/// <param name="e">Arguments about the event.</param>
		private void MainForm_Load(object sender, EventArgs e)
		{
			IO.Initialize();
			UpdateLabelGoodThingCount();

			int daysUntilNotify = (int)IO.TimeUntilNotify.TotalDays;
			if (daysUntilNotify == 1)
			{
				LabelDaysUntilJarUnlock.Text = $"There is 1 day until the jar unlocks!";
			}
			else
			{
				LabelDaysUntilJarUnlock.Text = $"There are {daysUntilNotify} days left in {DateTime.Now.Year}.";
			}

			if (IO.CanNotifyUser())
			{
				NotifyUser();
			}
		}

		/// <summary>
		/// Updates the <see cref="LabelGoodThingCount"/> with the number of good things in the jar. 
		/// </summary>
		private void UpdateLabelGoodThingCount()
		{
			int numberOfGoodThings = IO.NumberOfGoodThings;
			string plural = (numberOfGoodThings == 1) ? "thing" : "things";
			LabelGoodThingCount.Text = $"You've put {numberOfGoodThings} {plural} in the jar!";
		}

		/// <summary>
		/// Submits a new good thing to the jar.
		/// </summary>
		/// <param name="sender">The object that this event was called on.</param>
		/// <param name="e">The arguments for this event.</param>
		private void ButtonSubmit_Click(object sender, EventArgs e)
		{
			string goodThing = TextGoodThing.Text;
			IO.WriteGoodThing(goodThing);
			UpdateLabelGoodThingCount();
			TextGoodThing.Text = "";
		}

		/// <summary>
		/// Enables <see cref="ButtonSubmit"/> if there's text in <see cref="TextGoodThing"/>.  
		/// </summary>
		/// <param name="sender">The object that this event was called on.</param>
		/// <param name="e">The arguments for this event.</param>
		private void TextGoodThing_TextChanged(object sender, EventArgs e)
		{
			ButtonSubmit.Enabled = !string.IsNullOrEmpty(TextGoodThing.Text);
		}

		/// <summary>
		/// Notifies the user that they can now open this year's jar, and asks if they want to open
		/// it. If the user answers yes, the default application for text files will open the jar.
		/// </summary>
		private void NotifyUser()
		{
			var nl = Environment.NewLine;
			string messageLine1 = $"It's the end of {DateTime.Now.Year}!";
			string messageLine2 = "You can now open the jar and see all the good things you've saved!";
			string messageLine3 = "Would you like to open the jar and see what you've saved?";
			string message = $"{messageLine1}{nl}{messageLine2}{nl}{nl}{messageLine3}";

			var result = MessageBox.Show(message, "Jar of Good Things", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
			if (result == DialogResult.Yes)
			{
				System.Diagnostics.Process.Start(IO.GetJarFilePath());
			}
		}

		/// <summary>
		/// If this app is built with the DEBUG preprocessing symbol defined, the user can hold
		/// CTRL and double-click an empty part of the form to force a notification.
		/// </summary>
		/// <param name="sender">The object that this event was called on.</param>
		/// <param name="e">The arguments for this event.</param>
		private void MainForm_DoubleClick(object sender, EventArgs e)
		{
#if DEBUG
			if (ModifierKeys.HasFlag(Keys.Control))
			{
				NotifyUser();
			}
#endif
		}
	}
}
