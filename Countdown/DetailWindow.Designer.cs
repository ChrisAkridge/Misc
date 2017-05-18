namespace Countdown
{
	partial class DetailWindow
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.TabPageMain = new System.Windows.Forms.TabPage();
			this.LabelPercentage = new System.Windows.Forms.Label();
			this.Progress = new System.Windows.Forms.ProgressBar();
			this.ButtonEditEvent = new System.Windows.Forms.Button();
			this.LabelCountdownInfo = new System.Windows.Forms.Label();
			this.LabelEventName = new System.Windows.Forms.Label();
			this.TabPageForms = new System.Windows.Forms.TabPage();
			this.LabelDecimalPlaces = new System.Windows.Forms.Label();
			this.ButtonSubtractDecimalPlace = new System.Windows.Forms.Button();
			this.ButtonAddDecimalPlace = new System.Windows.Forms.Button();
			this.LabelXKCD1017 = new System.Windows.Forms.Label();
			this.LabelDecibelsSeconds = new System.Windows.Forms.Label();
			this.LabelRemainingYears = new System.Windows.Forms.Label();
			this.LabelRemainingWeeks = new System.Windows.Forms.Label();
			this.LabelRemainingDays = new System.Windows.Forms.Label();
			this.LabelRemainingHours = new System.Windows.Forms.Label();
			this.LabelRemainingMinutes = new System.Windows.Forms.Label();
			this.LabelRemainingSeconds = new System.Windows.Forms.Label();
			this.LabelDefaultForm = new System.Windows.Forms.Label();
			this.LabelFormsEventName = new System.Windows.Forms.Label();
			this.UpdateTimer = new System.Windows.Forms.Timer(this.components);
			this.tabControl1.SuspendLayout();
			this.TabPageMain.SuspendLayout();
			this.TabPageForms.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.TabPageMain);
			this.tabControl1.Controls.Add(this.TabPageForms);
			this.tabControl1.Location = new System.Drawing.Point(13, 13);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(259, 236);
			this.tabControl1.TabIndex = 0;
			// 
			// TabPageMain
			// 
			this.TabPageMain.Controls.Add(this.LabelPercentage);
			this.TabPageMain.Controls.Add(this.Progress);
			this.TabPageMain.Controls.Add(this.ButtonEditEvent);
			this.TabPageMain.Controls.Add(this.LabelCountdownInfo);
			this.TabPageMain.Controls.Add(this.LabelEventName);
			this.TabPageMain.Location = new System.Drawing.Point(4, 22);
			this.TabPageMain.Name = "TabPageMain";
			this.TabPageMain.Padding = new System.Windows.Forms.Padding(3);
			this.TabPageMain.Size = new System.Drawing.Size(251, 210);
			this.TabPageMain.TabIndex = 0;
			this.TabPageMain.Text = "Details";
			this.TabPageMain.UseVisualStyleBackColor = true;
			// 
			// LabelPercentage
			// 
			this.LabelPercentage.AutoSize = true;
			this.LabelPercentage.Location = new System.Drawing.Point(8, 135);
			this.LabelPercentage.Name = "LabelPercentage";
			this.LabelPercentage.Size = new System.Drawing.Size(43, 13);
			this.LabelPercentage.TabIndex = 7;
			this.LabelPercentage.Text = "12.34%";
			// 
			// Progress
			// 
			this.Progress.Location = new System.Drawing.Point(11, 151);
			this.Progress.Name = "Progress";
			this.Progress.Size = new System.Drawing.Size(232, 23);
			this.Progress.TabIndex = 6;
			// 
			// ButtonEditEvent
			// 
			this.ButtonEditEvent.Location = new System.Drawing.Point(190, 180);
			this.ButtonEditEvent.Name = "ButtonEditEvent";
			this.ButtonEditEvent.Size = new System.Drawing.Size(54, 24);
			this.ButtonEditEvent.TabIndex = 5;
			this.ButtonEditEvent.Text = "Edit...";
			this.ButtonEditEvent.UseVisualStyleBackColor = true;
			this.ButtonEditEvent.Click += new System.EventHandler(this.ButtonEditEvent_Click);
			// 
			// LabelCountdownInfo
			// 
			this.LabelCountdownInfo.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LabelCountdownInfo.Location = new System.Drawing.Point(5, 43);
			this.LabelCountdownInfo.Name = "LabelCountdownInfo";
			this.LabelCountdownInfo.Size = new System.Drawing.Size(239, 40);
			this.LabelCountdownInfo.TabIndex = 4;
			this.LabelCountdownInfo.Text = "1y2w3d 04:56:12.345";
			this.LabelCountdownInfo.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// LabelEventName
			// 
			this.LabelEventName.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LabelEventName.Location = new System.Drawing.Point(6, 3);
			this.LabelEventName.Name = "LabelEventName";
			this.LabelEventName.Size = new System.Drawing.Size(239, 40);
			this.LabelEventName.TabIndex = 3;
			this.LabelEventName.Text = "{event name}";
			this.LabelEventName.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// TabPageForms
			// 
			this.TabPageForms.Controls.Add(this.LabelDecimalPlaces);
			this.TabPageForms.Controls.Add(this.ButtonSubtractDecimalPlace);
			this.TabPageForms.Controls.Add(this.ButtonAddDecimalPlace);
			this.TabPageForms.Controls.Add(this.LabelXKCD1017);
			this.TabPageForms.Controls.Add(this.LabelDecibelsSeconds);
			this.TabPageForms.Controls.Add(this.LabelRemainingYears);
			this.TabPageForms.Controls.Add(this.LabelRemainingWeeks);
			this.TabPageForms.Controls.Add(this.LabelRemainingDays);
			this.TabPageForms.Controls.Add(this.LabelRemainingHours);
			this.TabPageForms.Controls.Add(this.LabelRemainingMinutes);
			this.TabPageForms.Controls.Add(this.LabelRemainingSeconds);
			this.TabPageForms.Controls.Add(this.LabelDefaultForm);
			this.TabPageForms.Controls.Add(this.LabelFormsEventName);
			this.TabPageForms.Location = new System.Drawing.Point(4, 22);
			this.TabPageForms.Name = "TabPageForms";
			this.TabPageForms.Padding = new System.Windows.Forms.Padding(3);
			this.TabPageForms.Size = new System.Drawing.Size(251, 210);
			this.TabPageForms.TabIndex = 1;
			this.TabPageForms.Text = "Forms";
			this.TabPageForms.UseVisualStyleBackColor = true;
			// 
			// LabelDecimalPlaces
			// 
			this.LabelDecimalPlaces.AutoSize = true;
			this.LabelDecimalPlaces.Location = new System.Drawing.Point(85, 186);
			this.LabelDecimalPlaces.Name = "LabelDecimalPlaces";
			this.LabelDecimalPlaces.Size = new System.Drawing.Size(90, 13);
			this.LabelDecimalPlaces.TabIndex = 16;
			this.LabelDecimalPlaces.Text = "0 decimal places";
			// 
			// ButtonSubtractDecimalPlace
			// 
			this.ButtonSubtractDecimalPlace.Location = new System.Drawing.Point(181, 181);
			this.ButtonSubtractDecimalPlace.Name = "ButtonSubtractDecimalPlace";
			this.ButtonSubtractDecimalPlace.Size = new System.Drawing.Size(29, 23);
			this.ButtonSubtractDecimalPlace.TabIndex = 15;
			this.ButtonSubtractDecimalPlace.Text = "-";
			this.ButtonSubtractDecimalPlace.UseVisualStyleBackColor = true;
			this.ButtonSubtractDecimalPlace.Click += new System.EventHandler(this.ButtonSubtractDecimalPlace_Click);
			// 
			// ButtonAddDecimalPlace
			// 
			this.ButtonAddDecimalPlace.Location = new System.Drawing.Point(216, 181);
			this.ButtonAddDecimalPlace.Name = "ButtonAddDecimalPlace";
			this.ButtonAddDecimalPlace.Size = new System.Drawing.Size(29, 23);
			this.ButtonAddDecimalPlace.TabIndex = 14;
			this.ButtonAddDecimalPlace.Text = "+";
			this.ButtonAddDecimalPlace.UseVisualStyleBackColor = true;
			this.ButtonAddDecimalPlace.Click += new System.EventHandler(this.ButtonAddDecimalPlace_Click);
			// 
			// LabelXKCD1017
			// 
			this.LabelXKCD1017.AutoSize = true;
			this.LabelXKCD1017.Location = new System.Drawing.Point(7, 151);
			this.LabelXKCD1017.Name = "LabelXKCD1017";
			this.LabelXKCD1017.Size = new System.Drawing.Size(193, 13);
			this.LabelXKCD1017.TabIndex = 13;
			this.LabelXKCD1017.Text = "XKCD 1017: Fri 2016-11-25 7:45:23am";
			// 
			// LabelDecibelsSeconds
			// 
			this.LabelDecibelsSeconds.AutoSize = true;
			this.LabelDecibelsSeconds.Location = new System.Drawing.Point(7, 138);
			this.LabelDecibelsSeconds.Name = "LabelDecibelsSeconds";
			this.LabelDecibelsSeconds.Size = new System.Drawing.Size(163, 13);
			this.LabelDecibelsSeconds.TabIndex = 12;
			this.LabelDecibelsSeconds.Text = "Decibels-Seconds: 12.34 dBsec";
			// 
			// LabelRemainingYears
			// 
			this.LabelRemainingYears.AutoSize = true;
			this.LabelRemainingYears.Location = new System.Drawing.Point(7, 125);
			this.LabelRemainingYears.Name = "LabelRemainingYears";
			this.LabelRemainingYears.Size = new System.Drawing.Size(146, 13);
			this.LabelRemainingYears.TabIndex = 11;
			this.LabelRemainingYears.Text = "Remaining Years: 1.23 years";
			// 
			// LabelRemainingWeeks
			// 
			this.LabelRemainingWeeks.AutoSize = true;
			this.LabelRemainingWeeks.Location = new System.Drawing.Point(7, 112);
			this.LabelRemainingWeeks.Name = "LabelRemainingWeeks";
			this.LabelRemainingWeeks.Size = new System.Drawing.Size(167, 13);
			this.LabelRemainingWeeks.TabIndex = 10;
			this.LabelRemainingWeeks.Text = "Remaining Weeks: 12.34 weeks";
			// 
			// LabelRemainingDays
			// 
			this.LabelRemainingDays.AutoSize = true;
			this.LabelRemainingDays.Location = new System.Drawing.Point(7, 99);
			this.LabelRemainingDays.Name = "LabelRemainingDays";
			this.LabelRemainingDays.Size = new System.Drawing.Size(154, 13);
			this.LabelRemainingDays.TabIndex = 9;
			this.LabelRemainingDays.Text = "Remaining Days: 123.45 days";
			// 
			// LabelRemainingHours
			// 
			this.LabelRemainingHours.AutoSize = true;
			this.LabelRemainingHours.Location = new System.Drawing.Point(7, 86);
			this.LabelRemainingHours.Name = "LabelRemainingHours";
			this.LabelRemainingHours.Size = new System.Drawing.Size(177, 13);
			this.LabelRemainingHours.TabIndex = 8;
			this.LabelRemainingHours.Text = "Remaining Hours: 1,234.56 hours";
			// 
			// LabelRemainingMinutes
			// 
			this.LabelRemainingMinutes.AutoSize = true;
			this.LabelRemainingMinutes.Location = new System.Drawing.Point(7, 73);
			this.LabelRemainingMinutes.Name = "LabelRemainingMinutes";
			this.LabelRemainingMinutes.Size = new System.Drawing.Size(211, 13);
			this.LabelRemainingMinutes.TabIndex = 7;
			this.LabelRemainingMinutes.Text = "Remaining Minutes: 123,456.78 minutes";
			// 
			// LabelRemainingSeconds
			// 
			this.LabelRemainingSeconds.AutoSize = true;
			this.LabelRemainingSeconds.Location = new System.Drawing.Point(7, 60);
			this.LabelRemainingSeconds.Name = "LabelRemainingSeconds";
			this.LabelRemainingSeconds.Size = new System.Drawing.Size(228, 13);
			this.LabelRemainingSeconds.TabIndex = 6;
			this.LabelRemainingSeconds.Text = "Remaining Seconds: 1,234,567.890 seconds";
			// 
			// LabelDefaultForm
			// 
			this.LabelDefaultForm.AutoSize = true;
			this.LabelDefaultForm.Location = new System.Drawing.Point(7, 47);
			this.LabelDefaultForm.Name = "LabelDefaultForm";
			this.LabelDefaultForm.Size = new System.Drawing.Size(150, 13);
			this.LabelDefaultForm.TabIndex = 5;
			this.LabelDefaultForm.Text = "Default: 1y2w3d 4:56:12.345";
			// 
			// LabelFormsEventName
			// 
			this.LabelFormsEventName.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LabelFormsEventName.Location = new System.Drawing.Point(6, 3);
			this.LabelFormsEventName.Name = "LabelFormsEventName";
			this.LabelFormsEventName.Size = new System.Drawing.Size(239, 40);
			this.LabelFormsEventName.TabIndex = 4;
			this.LabelFormsEventName.Text = "{event name}";
			this.LabelFormsEventName.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// UpdateTimer
			// 
			this.UpdateTimer.Enabled = true;
			this.UpdateTimer.Interval = 33;
			this.UpdateTimer.Tick += new System.EventHandler(this.UpdateTimer_Tick);
			// 
			// DetailWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 261);
			this.Controls.Add(this.tabControl1);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "DetailWindow";
			this.Text = "{event name)";
			this.tabControl1.ResumeLayout(false);
			this.TabPageMain.ResumeLayout(false);
			this.TabPageMain.PerformLayout();
			this.TabPageForms.ResumeLayout(false);
			this.TabPageForms.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage TabPageMain;
		private System.Windows.Forms.Button ButtonEditEvent;
		private System.Windows.Forms.Label LabelCountdownInfo;
		private System.Windows.Forms.Label LabelEventName;
		private System.Windows.Forms.TabPage TabPageForms;
		private System.Windows.Forms.Label LabelDecimalPlaces;
		private System.Windows.Forms.Button ButtonSubtractDecimalPlace;
		private System.Windows.Forms.Button ButtonAddDecimalPlace;
		private System.Windows.Forms.Label LabelXKCD1017;
		private System.Windows.Forms.Label LabelDecibelsSeconds;
		private System.Windows.Forms.Label LabelRemainingYears;
		private System.Windows.Forms.Label LabelRemainingWeeks;
		private System.Windows.Forms.Label LabelRemainingDays;
		private System.Windows.Forms.Label LabelRemainingHours;
		private System.Windows.Forms.Label LabelRemainingMinutes;
		private System.Windows.Forms.Label LabelRemainingSeconds;
		private System.Windows.Forms.Label LabelDefaultForm;
		private System.Windows.Forms.Label LabelFormsEventName;
		private System.Windows.Forms.Timer UpdateTimer;
		private System.Windows.Forms.Label LabelPercentage;
		private System.Windows.Forms.ProgressBar Progress;
	}
}