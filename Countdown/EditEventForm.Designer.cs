namespace Countdown
{
	partial class EditEventForm
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
			this.TabControl = new System.Windows.Forms.TabControl();
			this.TabPageDetails = new System.Windows.Forms.TabPage();
			this.StaticLabelStartTimeInfo = new System.Windows.Forms.Label();
			this.CheckBoxStartTime = new System.Windows.Forms.CheckBox();
			this.StaticLabelEndTime = new System.Windows.Forms.Label();
			this.DTPStartTime = new System.Windows.Forms.DateTimePicker();
			this.DTPEndTime = new System.Windows.Forms.DateTimePicker();
			this.TextName = new System.Windows.Forms.TextBox();
			this.StaticLabelName = new System.Windows.Forms.Label();
			this.TabPageRecurrence = new System.Windows.Forms.TabPage();
			this.DTPYearlyOnDate = new System.Windows.Forms.DateTimePicker();
			this.RadioYearly = new System.Windows.Forms.RadioButton();
			this.StaticLabelOfTheMonth = new System.Windows.Forms.Label();
			this.ComboWeekday = new System.Windows.Forms.ComboBox();
			this.LabelOrdinalSuffix = new System.Windows.Forms.Label();
			this.NUDSpecificWeekdayNumber = new System.Windows.Forms.NumericUpDown();
			this.RadioSpecificWeekday = new System.Windows.Forms.RadioButton();
			this.NUDMonthlyDayOfMonth = new System.Windows.Forms.NumericUpDown();
			this.RadioSpecificDay = new System.Windows.Forms.RadioButton();
			this.RadioMonthly = new System.Windows.Forms.RadioButton();
			this.CheckBoxSaturday = new System.Windows.Forms.CheckBox();
			this.CheckBoxFriday = new System.Windows.Forms.CheckBox();
			this.CheckBoxThursday = new System.Windows.Forms.CheckBox();
			this.CheckBoxWednesday = new System.Windows.Forms.CheckBox();
			this.CheckBoxTuesday = new System.Windows.Forms.CheckBox();
			this.CheckBoxMonday = new System.Windows.Forms.CheckBox();
			this.CheckBoxSunday = new System.Windows.Forms.CheckBox();
			this.RadioWeekly = new System.Windows.Forms.RadioButton();
			this.StaticLabelDays = new System.Windows.Forms.Label();
			this.NUDDailyRepeat = new System.Windows.Forms.NumericUpDown();
			this.StaticLabelRepeatEvery = new System.Windows.Forms.Label();
			this.RadioDaily = new System.Windows.Forms.RadioButton();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.CheckBoxEnableRecurrence = new System.Windows.Forms.CheckBox();
			this.ButtonCancel = new System.Windows.Forms.Button();
			this.ButtonOK = new System.Windows.Forms.Button();
			this.MonthlyGroupBox = new System.Windows.Forms.GroupBox();
			this.TabControl.SuspendLayout();
			this.TabPageDetails.SuspendLayout();
			this.TabPageRecurrence.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.NUDSpecificWeekdayNumber)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NUDMonthlyDayOfMonth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NUDDailyRepeat)).BeginInit();
			this.MonthlyGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// TabControl
			// 
			this.TabControl.Controls.Add(this.TabPageDetails);
			this.TabControl.Controls.Add(this.TabPageRecurrence);
			this.TabControl.Location = new System.Drawing.Point(13, 13);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = new System.Drawing.Size(339, 344);
			this.TabControl.TabIndex = 0;
			// 
			// TabPageDetails
			// 
			this.TabPageDetails.Controls.Add(this.StaticLabelStartTimeInfo);
			this.TabPageDetails.Controls.Add(this.CheckBoxStartTime);
			this.TabPageDetails.Controls.Add(this.StaticLabelEndTime);
			this.TabPageDetails.Controls.Add(this.DTPStartTime);
			this.TabPageDetails.Controls.Add(this.DTPEndTime);
			this.TabPageDetails.Controls.Add(this.TextName);
			this.TabPageDetails.Controls.Add(this.StaticLabelName);
			this.TabPageDetails.Location = new System.Drawing.Point(4, 22);
			this.TabPageDetails.Name = "TabPageDetails";
			this.TabPageDetails.Padding = new System.Windows.Forms.Padding(3);
			this.TabPageDetails.Size = new System.Drawing.Size(331, 271);
			this.TabPageDetails.TabIndex = 0;
			this.TabPageDetails.Text = "Details";
			this.TabPageDetails.UseVisualStyleBackColor = true;
			// 
			// StaticLabelStartTimeInfo
			// 
			this.StaticLabelStartTimeInfo.AutoSize = true;
			this.StaticLabelStartTimeInfo.Location = new System.Drawing.Point(6, 90);
			this.StaticLabelStartTimeInfo.Name = "StaticLabelStartTimeInfo";
			this.StaticLabelStartTimeInfo.Size = new System.Drawing.Size(307, 26);
			this.StaticLabelStartTimeInfo.TabIndex = 6;
			this.StaticLabelStartTimeInfo.Text = "Setting a start time is required for displaying progress and\r\ndisplaying the XKCD" +
    " 1017 display form.";
			// 
			// CheckBoxStartTime
			// 
			this.CheckBoxStartTime.AutoSize = true;
			this.CheckBoxStartTime.Location = new System.Drawing.Point(6, 66);
			this.CheckBoxStartTime.Name = "CheckBoxStartTime";
			this.CheckBoxStartTime.Size = new System.Drawing.Size(79, 17);
			this.CheckBoxStartTime.TabIndex = 5;
			this.CheckBoxStartTime.Text = "Start Time:";
			this.CheckBoxStartTime.UseVisualStyleBackColor = true;
			this.CheckBoxStartTime.CheckedChanged += new System.EventHandler(this.CheckBoxStartTime_CheckedChanged);
			// 
			// StaticLabelEndTime
			// 
			this.StaticLabelEndTime.AutoSize = true;
			this.StaticLabelEndTime.Location = new System.Drawing.Point(8, 39);
			this.StaticLabelEndTime.Name = "StaticLabelEndTime";
			this.StaticLabelEndTime.Size = new System.Drawing.Size(56, 13);
			this.StaticLabelEndTime.TabIndex = 4;
			this.StaticLabelEndTime.Text = "End Time:";
			// 
			// DTPStartTime
			// 
			this.DTPStartTime.CustomFormat = "MM/dd/yy hh:mm:ss tt";
			this.DTPStartTime.Enabled = false;
			this.DTPStartTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.DTPStartTime.Location = new System.Drawing.Point(92, 64);
			this.DTPStartTime.Name = "DTPStartTime";
			this.DTPStartTime.Size = new System.Drawing.Size(233, 22);
			this.DTPStartTime.TabIndex = 3;
			// 
			// DTPEndTime
			// 
			this.DTPEndTime.CustomFormat = "MM/dd/yy hh:mm:ss tt";
			this.DTPEndTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.DTPEndTime.Location = new System.Drawing.Point(70, 36);
			this.DTPEndTime.Name = "DTPEndTime";
			this.DTPEndTime.Size = new System.Drawing.Size(255, 22);
			this.DTPEndTime.TabIndex = 2;
			// 
			// TextName
			// 
			this.TextName.Location = new System.Drawing.Point(53, 8);
			this.TextName.Name = "TextName";
			this.TextName.Size = new System.Drawing.Size(272, 22);
			this.TextName.TabIndex = 1;
			// 
			// StaticLabelName
			// 
			this.StaticLabelName.AutoSize = true;
			this.StaticLabelName.Location = new System.Drawing.Point(8, 10);
			this.StaticLabelName.Name = "StaticLabelName";
			this.StaticLabelName.Size = new System.Drawing.Size(39, 13);
			this.StaticLabelName.TabIndex = 0;
			this.StaticLabelName.Text = "Name:";
			// 
			// TabPageRecurrence
			// 
			this.TabPageRecurrence.Controls.Add(this.MonthlyGroupBox);
			this.TabPageRecurrence.Controls.Add(this.DTPYearlyOnDate);
			this.TabPageRecurrence.Controls.Add(this.RadioYearly);
			this.TabPageRecurrence.Controls.Add(this.RadioMonthly);
			this.TabPageRecurrence.Controls.Add(this.CheckBoxSaturday);
			this.TabPageRecurrence.Controls.Add(this.CheckBoxFriday);
			this.TabPageRecurrence.Controls.Add(this.CheckBoxThursday);
			this.TabPageRecurrence.Controls.Add(this.CheckBoxWednesday);
			this.TabPageRecurrence.Controls.Add(this.CheckBoxTuesday);
			this.TabPageRecurrence.Controls.Add(this.CheckBoxMonday);
			this.TabPageRecurrence.Controls.Add(this.CheckBoxSunday);
			this.TabPageRecurrence.Controls.Add(this.RadioWeekly);
			this.TabPageRecurrence.Controls.Add(this.StaticLabelDays);
			this.TabPageRecurrence.Controls.Add(this.NUDDailyRepeat);
			this.TabPageRecurrence.Controls.Add(this.StaticLabelRepeatEvery);
			this.TabPageRecurrence.Controls.Add(this.RadioDaily);
			this.TabPageRecurrence.Controls.Add(this.groupBox1);
			this.TabPageRecurrence.Controls.Add(this.CheckBoxEnableRecurrence);
			this.TabPageRecurrence.Location = new System.Drawing.Point(4, 22);
			this.TabPageRecurrence.Name = "TabPageRecurrence";
			this.TabPageRecurrence.Padding = new System.Windows.Forms.Padding(3);
			this.TabPageRecurrence.Size = new System.Drawing.Size(331, 318);
			this.TabPageRecurrence.TabIndex = 1;
			this.TabPageRecurrence.Text = "Recurrence";
			this.TabPageRecurrence.UseVisualStyleBackColor = true;
			// 
			// DTPYearlyOnDate
			// 
			this.DTPYearlyOnDate.CustomFormat = "MM/dd";
			this.DTPYearlyOnDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.DTPYearlyOnDate.Location = new System.Drawing.Point(80, 279);
			this.DTPYearlyOnDate.Name = "DTPYearlyOnDate";
			this.DTPYearlyOnDate.Size = new System.Drawing.Size(68, 22);
			this.DTPYearlyOnDate.TabIndex = 23;
			// 
			// RadioYearly
			// 
			this.RadioYearly.AutoSize = true;
			this.RadioYearly.Location = new System.Drawing.Point(10, 281);
			this.RadioYearly.Name = "RadioYearly";
			this.RadioYearly.Size = new System.Drawing.Size(73, 17);
			this.RadioYearly.TabIndex = 22;
			this.RadioYearly.TabStop = true;
			this.RadioYearly.Text = "Yearly on:";
			this.RadioYearly.UseVisualStyleBackColor = true;
			// 
			// StaticLabelOfTheMonth
			// 
			this.StaticLabelOfTheMonth.AutoSize = true;
			this.StaticLabelOfTheMonth.Location = new System.Drawing.Point(219, 48);
			this.StaticLabelOfTheMonth.Name = "StaticLabelOfTheMonth";
			this.StaticLabelOfTheMonth.Size = new System.Drawing.Size(75, 13);
			this.StaticLabelOfTheMonth.TabIndex = 21;
			this.StaticLabelOfTheMonth.Text = "of the month";
			// 
			// ComboWeekday
			// 
			this.ComboWeekday.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ComboWeekday.FormattingEnabled = true;
			this.ComboWeekday.Items.AddRange(new object[] {
            "Sunday",
            "Monday",
            "Tuesday",
            "Wednesday",
            "Thursday",
            "Friday",
            "Saturday"});
			this.ComboWeekday.Location = new System.Drawing.Point(128, 45);
			this.ComboWeekday.Name = "ComboWeekday";
			this.ComboWeekday.Size = new System.Drawing.Size(85, 21);
			this.ComboWeekday.TabIndex = 20;
			// 
			// LabelOrdinalSuffix
			// 
			this.LabelOrdinalSuffix.AutoSize = true;
			this.LabelOrdinalSuffix.Location = new System.Drawing.Point(106, 48);
			this.LabelOrdinalSuffix.Name = "LabelOrdinalSuffix";
			this.LabelOrdinalSuffix.Size = new System.Drawing.Size(16, 13);
			this.LabelOrdinalSuffix.TabIndex = 19;
			this.LabelOrdinalSuffix.Text = "st";
			// 
			// NUDSpecificWeekdayNumber
			// 
			this.NUDSpecificWeekdayNumber.Location = new System.Drawing.Point(72, 44);
			this.NUDSpecificWeekdayNumber.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
			this.NUDSpecificWeekdayNumber.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.NUDSpecificWeekdayNumber.Name = "NUDSpecificWeekdayNumber";
			this.NUDSpecificWeekdayNumber.Size = new System.Drawing.Size(29, 22);
			this.NUDSpecificWeekdayNumber.TabIndex = 18;
			this.NUDSpecificWeekdayNumber.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.NUDSpecificWeekdayNumber.ValueChanged += new System.EventHandler(this.NUDSpecificWeekdayNumber_ValueChanged);
			// 
			// RadioSpecificWeekday
			// 
			this.RadioSpecificWeekday.AutoSize = true;
			this.RadioSpecificWeekday.Location = new System.Drawing.Point(5, 44);
			this.RadioSpecificWeekday.Name = "RadioSpecificWeekday";
			this.RadioSpecificWeekday.Size = new System.Drawing.Size(61, 17);
			this.RadioSpecificWeekday.TabIndex = 17;
			this.RadioSpecificWeekday.TabStop = true;
			this.RadioSpecificWeekday.Text = "On the";
			this.RadioSpecificWeekday.UseVisualStyleBackColor = true;
			// 
			// NUDMonthlyDayOfMonth
			// 
			this.NUDMonthlyDayOfMonth.Location = new System.Drawing.Point(120, 21);
			this.NUDMonthlyDayOfMonth.Maximum = new decimal(new int[] {
            31,
            0,
            0,
            0});
			this.NUDMonthlyDayOfMonth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.NUDMonthlyDayOfMonth.Name = "NUDMonthlyDayOfMonth";
			this.NUDMonthlyDayOfMonth.Size = new System.Drawing.Size(39, 22);
			this.NUDMonthlyDayOfMonth.TabIndex = 16;
			this.NUDMonthlyDayOfMonth.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// RadioSpecificDay
			// 
			this.RadioSpecificDay.AutoSize = true;
			this.RadioSpecificDay.Location = new System.Drawing.Point(6, 21);
			this.RadioSpecificDay.Name = "RadioSpecificDay";
			this.RadioSpecificDay.Size = new System.Drawing.Size(108, 17);
			this.RadioSpecificDay.TabIndex = 15;
			this.RadioSpecificDay.TabStop = true;
			this.RadioSpecificDay.Text = "Occurs on day #";
			this.RadioSpecificDay.UseVisualStyleBackColor = true;
			// 
			// RadioMonthly
			// 
			this.RadioMonthly.AutoSize = true;
			this.RadioMonthly.Location = new System.Drawing.Point(7, 176);
			this.RadioMonthly.Name = "RadioMonthly";
			this.RadioMonthly.Size = new System.Drawing.Size(68, 17);
			this.RadioMonthly.TabIndex = 14;
			this.RadioMonthly.TabStop = true;
			this.RadioMonthly.Text = "Monthly";
			this.RadioMonthly.UseVisualStyleBackColor = true;
			// 
			// CheckBoxSaturday
			// 
			this.CheckBoxSaturday.AutoSize = true;
			this.CheckBoxSaturday.Location = new System.Drawing.Point(147, 152);
			this.CheckBoxSaturday.Name = "CheckBoxSaturday";
			this.CheckBoxSaturday.Size = new System.Drawing.Size(71, 17);
			this.CheckBoxSaturday.TabIndex = 13;
			this.CheckBoxSaturday.Text = "Saturday";
			this.CheckBoxSaturday.UseVisualStyleBackColor = true;
			// 
			// CheckBoxFriday
			// 
			this.CheckBoxFriday.AutoSize = true;
			this.CheckBoxFriday.Location = new System.Drawing.Point(77, 152);
			this.CheckBoxFriday.Name = "CheckBoxFriday";
			this.CheckBoxFriday.Size = new System.Drawing.Size(57, 17);
			this.CheckBoxFriday.TabIndex = 12;
			this.CheckBoxFriday.Text = "Friday";
			this.CheckBoxFriday.UseVisualStyleBackColor = true;
			// 
			// CheckBoxThursday
			// 
			this.CheckBoxThursday.AutoSize = true;
			this.CheckBoxThursday.Location = new System.Drawing.Point(7, 152);
			this.CheckBoxThursday.Name = "CheckBoxThursday";
			this.CheckBoxThursday.Size = new System.Drawing.Size(72, 17);
			this.CheckBoxThursday.TabIndex = 11;
			this.CheckBoxThursday.Text = "Thursday";
			this.CheckBoxThursday.UseVisualStyleBackColor = true;
			// 
			// CheckBoxWednesday
			// 
			this.CheckBoxWednesday.AutoSize = true;
			this.CheckBoxWednesday.Location = new System.Drawing.Point(217, 129);
			this.CheckBoxWednesday.Name = "CheckBoxWednesday";
			this.CheckBoxWednesday.Size = new System.Drawing.Size(86, 17);
			this.CheckBoxWednesday.TabIndex = 10;
			this.CheckBoxWednesday.Text = "Wednesday";
			this.CheckBoxWednesday.UseVisualStyleBackColor = true;
			// 
			// CheckBoxTuesday
			// 
			this.CheckBoxTuesday.AutoSize = true;
			this.CheckBoxTuesday.Location = new System.Drawing.Point(147, 129);
			this.CheckBoxTuesday.Name = "CheckBoxTuesday";
			this.CheckBoxTuesday.Size = new System.Drawing.Size(67, 17);
			this.CheckBoxTuesday.TabIndex = 9;
			this.CheckBoxTuesday.Text = "Tuesday";
			this.CheckBoxTuesday.UseVisualStyleBackColor = true;
			// 
			// CheckBoxMonday
			// 
			this.CheckBoxMonday.AutoSize = true;
			this.CheckBoxMonday.Location = new System.Drawing.Point(77, 129);
			this.CheckBoxMonday.Name = "CheckBoxMonday";
			this.CheckBoxMonday.Size = new System.Drawing.Size(68, 17);
			this.CheckBoxMonday.TabIndex = 8;
			this.CheckBoxMonday.Text = "Monday";
			this.CheckBoxMonday.UseVisualStyleBackColor = true;
			// 
			// CheckBoxSunday
			// 
			this.CheckBoxSunday.AutoSize = true;
			this.CheckBoxSunday.Location = new System.Drawing.Point(7, 129);
			this.CheckBoxSunday.Name = "CheckBoxSunday";
			this.CheckBoxSunday.Size = new System.Drawing.Size(64, 17);
			this.CheckBoxSunday.TabIndex = 7;
			this.CheckBoxSunday.Text = "Sunday";
			this.CheckBoxSunday.UseVisualStyleBackColor = true;
			// 
			// RadioWeekly
			// 
			this.RadioWeekly.AutoSize = true;
			this.RadioWeekly.Location = new System.Drawing.Point(7, 105);
			this.RadioWeekly.Name = "RadioWeekly";
			this.RadioWeekly.Size = new System.Drawing.Size(62, 17);
			this.RadioWeekly.TabIndex = 6;
			this.RadioWeekly.TabStop = true;
			this.RadioWeekly.Text = "Weekly";
			this.RadioWeekly.UseVisualStyleBackColor = true;
			// 
			// StaticLabelDays
			// 
			this.StaticLabelDays.AutoSize = true;
			this.StaticLabelDays.Location = new System.Drawing.Point(130, 77);
			this.StaticLabelDays.Name = "StaticLabelDays";
			this.StaticLabelDays.Size = new System.Drawing.Size(30, 13);
			this.StaticLabelDays.TabIndex = 5;
			this.StaticLabelDays.Text = "days";
			// 
			// NUDDailyRepeat
			// 
			this.NUDDailyRepeat.Location = new System.Drawing.Point(85, 75);
			this.NUDDailyRepeat.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.NUDDailyRepeat.Name = "NUDDailyRepeat";
			this.NUDDailyRepeat.Size = new System.Drawing.Size(39, 22);
			this.NUDDailyRepeat.TabIndex = 4;
			this.NUDDailyRepeat.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// StaticLabelRepeatEvery
			// 
			this.StaticLabelRepeatEvery.AutoSize = true;
			this.StaticLabelRepeatEvery.Location = new System.Drawing.Point(7, 77);
			this.StaticLabelRepeatEvery.Name = "StaticLabelRepeatEvery";
			this.StaticLabelRepeatEvery.Size = new System.Drawing.Size(72, 13);
			this.StaticLabelRepeatEvery.TabIndex = 3;
			this.StaticLabelRepeatEvery.Text = "Repeat every";
			// 
			// RadioDaily
			// 
			this.RadioDaily.AutoSize = true;
			this.RadioDaily.Location = new System.Drawing.Point(7, 53);
			this.RadioDaily.Name = "RadioDaily";
			this.RadioDaily.Size = new System.Drawing.Size(50, 17);
			this.RadioDaily.TabIndex = 2;
			this.RadioDaily.TabStop = true;
			this.RadioDaily.Text = "Daily";
			this.RadioDaily.UseVisualStyleBackColor = true;
			// 
			// groupBox1
			// 
			this.groupBox1.Location = new System.Drawing.Point(7, 31);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(316, 11);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			// 
			// CheckBoxEnableRecurrence
			// 
			this.CheckBoxEnableRecurrence.AutoSize = true;
			this.CheckBoxEnableRecurrence.Location = new System.Drawing.Point(7, 7);
			this.CheckBoxEnableRecurrence.Name = "CheckBoxEnableRecurrence";
			this.CheckBoxEnableRecurrence.Size = new System.Drawing.Size(107, 17);
			this.CheckBoxEnableRecurrence.TabIndex = 0;
			this.CheckBoxEnableRecurrence.Text = "Recurring Event";
			this.CheckBoxEnableRecurrence.UseVisualStyleBackColor = true;
			this.CheckBoxEnableRecurrence.CheckedChanged += new System.EventHandler(this.CheckBoxEnableRecurrence_CheckedChanged);
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.Location = new System.Drawing.Point(279, 363);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = new System.Drawing.Size(75, 23);
			this.ButtonCancel.TabIndex = 1;
			this.ButtonCancel.Text = "Cancel";
			this.ButtonCancel.UseVisualStyleBackColor = true;
			// 
			// ButtonOK
			// 
			this.ButtonOK.Location = new System.Drawing.Point(198, 363);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.Size = new System.Drawing.Size(75, 23);
			this.ButtonOK.TabIndex = 2;
			this.ButtonOK.Text = "OK";
			this.ButtonOK.UseVisualStyleBackColor = true;
			this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// MonthlyGroupBox
			// 
			this.MonthlyGroupBox.Controls.Add(this.RadioSpecificDay);
			this.MonthlyGroupBox.Controls.Add(this.NUDMonthlyDayOfMonth);
			this.MonthlyGroupBox.Controls.Add(this.RadioSpecificWeekday);
			this.MonthlyGroupBox.Controls.Add(this.StaticLabelOfTheMonth);
			this.MonthlyGroupBox.Controls.Add(this.NUDSpecificWeekdayNumber);
			this.MonthlyGroupBox.Controls.Add(this.ComboWeekday);
			this.MonthlyGroupBox.Controls.Add(this.LabelOrdinalSuffix);
			this.MonthlyGroupBox.Location = new System.Drawing.Point(10, 199);
			this.MonthlyGroupBox.Name = "MonthlyGroupBox";
			this.MonthlyGroupBox.Size = new System.Drawing.Size(313, 71);
			this.MonthlyGroupBox.TabIndex = 24;
			this.MonthlyGroupBox.TabStop = false;
			// 
			// EditEventForm
			// 
			this.AcceptButton = this.ButtonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.ButtonCancel;
			this.ClientSize = new System.Drawing.Size(364, 398);
			this.Controls.Add(this.ButtonOK);
			this.Controls.Add(this.ButtonCancel);
			this.Controls.Add(this.TabControl);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MaximizeBox = false;
			this.Name = "EditEventForm";
			this.Text = "Edit Event";
			this.Load += new System.EventHandler(this.EditEventForm_Load);
			this.TabControl.ResumeLayout(false);
			this.TabPageDetails.ResumeLayout(false);
			this.TabPageDetails.PerformLayout();
			this.TabPageRecurrence.ResumeLayout(false);
			this.TabPageRecurrence.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.NUDSpecificWeekdayNumber)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NUDMonthlyDayOfMonth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NUDDailyRepeat)).EndInit();
			this.MonthlyGroupBox.ResumeLayout(false);
			this.MonthlyGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl TabControl;
		private System.Windows.Forms.TabPage TabPageDetails;
		private System.Windows.Forms.Label StaticLabelStartTimeInfo;
		private System.Windows.Forms.CheckBox CheckBoxStartTime;
		private System.Windows.Forms.Label StaticLabelEndTime;
		private System.Windows.Forms.DateTimePicker DTPStartTime;
		private System.Windows.Forms.DateTimePicker DTPEndTime;
		private System.Windows.Forms.TextBox TextName;
		private System.Windows.Forms.Label StaticLabelName;
		private System.Windows.Forms.TabPage TabPageRecurrence;
		private System.Windows.Forms.RadioButton RadioMonthly;
		private System.Windows.Forms.CheckBox CheckBoxSaturday;
		private System.Windows.Forms.CheckBox CheckBoxFriday;
		private System.Windows.Forms.CheckBox CheckBoxThursday;
		private System.Windows.Forms.CheckBox CheckBoxWednesday;
		private System.Windows.Forms.CheckBox CheckBoxTuesday;
		private System.Windows.Forms.CheckBox CheckBoxMonday;
		private System.Windows.Forms.CheckBox CheckBoxSunday;
		private System.Windows.Forms.RadioButton RadioWeekly;
		private System.Windows.Forms.Label StaticLabelDays;
		private System.Windows.Forms.NumericUpDown NUDDailyRepeat;
		private System.Windows.Forms.Label StaticLabelRepeatEvery;
		private System.Windows.Forms.RadioButton RadioDaily;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox CheckBoxEnableRecurrence;
		private System.Windows.Forms.Button ButtonCancel;
		private System.Windows.Forms.Button ButtonOK;
		private System.Windows.Forms.Label StaticLabelOfTheMonth;
		private System.Windows.Forms.ComboBox ComboWeekday;
		private System.Windows.Forms.Label LabelOrdinalSuffix;
		private System.Windows.Forms.NumericUpDown NUDSpecificWeekdayNumber;
		private System.Windows.Forms.RadioButton RadioSpecificWeekday;
		private System.Windows.Forms.NumericUpDown NUDMonthlyDayOfMonth;
		private System.Windows.Forms.RadioButton RadioSpecificDay;
		private System.Windows.Forms.DateTimePicker DTPYearlyOnDate;
		private System.Windows.Forms.RadioButton RadioYearly;
		private System.Windows.Forms.GroupBox MonthlyGroupBox;
	}
}