namespace QuickCountdown
{
	partial class MainForm
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
			this.TabMain = new System.Windows.Forms.TabControl();
			this.TabPageCountdown = new System.Windows.Forms.TabPage();
			this.LabelTotalTime = new System.Windows.Forms.Label();
			this.StaticLabelTotalTime = new System.Windows.Forms.Label();
			this.LabelRemaining = new System.Windows.Forms.Label();
			this.StaticLabelTimeRemaining = new System.Windows.Forms.Label();
			this.LabelSeparator1 = new System.Windows.Forms.Label();
			this.DTPCountdownTime = new System.Windows.Forms.DateTimePicker();
			this.DTPCountdownDate = new System.Windows.Forms.DateTimePicker();
			this.LabelCountdownDateTime = new System.Windows.Forms.Label();
			this.TabTimeBetween = new System.Windows.Forms.TabPage();
			this.MainTimer = new System.Windows.Forms.Timer(this.components);
			this.TabMain.SuspendLayout();
			this.TabPageCountdown.SuspendLayout();
			this.SuspendLayout();
			// 
			// TabMain
			// 
			this.TabMain.Controls.Add(this.TabPageCountdown);
			this.TabMain.Controls.Add(this.TabTimeBetween);
			this.TabMain.Location = new System.Drawing.Point(12, 12);
			this.TabMain.Name = "TabMain";
			this.TabMain.SelectedIndex = 0;
			this.TabMain.Size = new System.Drawing.Size(410, 387);
			this.TabMain.TabIndex = 0;
			// 
			// TabPageCountdown
			// 
			this.TabPageCountdown.Controls.Add(this.LabelTotalTime);
			this.TabPageCountdown.Controls.Add(this.StaticLabelTotalTime);
			this.TabPageCountdown.Controls.Add(this.LabelRemaining);
			this.TabPageCountdown.Controls.Add(this.StaticLabelTimeRemaining);
			this.TabPageCountdown.Controls.Add(this.LabelSeparator1);
			this.TabPageCountdown.Controls.Add(this.DTPCountdownTime);
			this.TabPageCountdown.Controls.Add(this.DTPCountdownDate);
			this.TabPageCountdown.Controls.Add(this.LabelCountdownDateTime);
			this.TabPageCountdown.Location = new System.Drawing.Point(4, 22);
			this.TabPageCountdown.Name = "TabPageCountdown";
			this.TabPageCountdown.Padding = new System.Windows.Forms.Padding(3);
			this.TabPageCountdown.Size = new System.Drawing.Size(402, 361);
			this.TabPageCountdown.TabIndex = 0;
			this.TabPageCountdown.Text = "Countdown";
			this.TabPageCountdown.UseVisualStyleBackColor = true;
			// 
			// LabelTotalTime
			// 
			this.LabelTotalTime.AutoSize = true;
			this.LabelTotalTime.Location = new System.Drawing.Point(292, 54);
			this.LabelTotalTime.Name = "LabelTotalTime";
			this.LabelTotalTime.Size = new System.Drawing.Size(38, 13);
			this.LabelTotalTime.TabIndex = 7;
			this.LabelTotalTime.Text = "label1";
			// 
			// StaticLabelTotalTime
			// 
			this.StaticLabelTotalTime.AutoSize = true;
			this.StaticLabelTotalTime.Location = new System.Drawing.Point(269, 37);
			this.StaticLabelTotalTime.Name = "StaticLabelTotalTime";
			this.StaticLabelTotalTime.Size = new System.Drawing.Size(61, 13);
			this.StaticLabelTotalTime.TabIndex = 6;
			this.StaticLabelTotalTime.Text = "Total Time:";
			// 
			// LabelRemaining
			// 
			this.LabelRemaining.AutoSize = true;
			this.LabelRemaining.Location = new System.Drawing.Point(32, 54);
			this.LabelRemaining.Name = "LabelRemaining";
			this.LabelRemaining.Size = new System.Drawing.Size(38, 13);
			this.LabelRemaining.TabIndex = 5;
			this.LabelRemaining.Text = "label1";
			// 
			// StaticLabelTimeRemaining
			// 
			this.StaticLabelTimeRemaining.AutoSize = true;
			this.StaticLabelTimeRemaining.Location = new System.Drawing.Point(9, 37);
			this.StaticLabelTimeRemaining.Name = "StaticLabelTimeRemaining";
			this.StaticLabelTimeRemaining.Size = new System.Drawing.Size(91, 13);
			this.StaticLabelTimeRemaining.TabIndex = 4;
			this.StaticLabelTimeRemaining.Text = "Time Remaining:";
			// 
			// LabelSeparator1
			// 
			this.LabelSeparator1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.LabelSeparator1.Location = new System.Drawing.Point(9, 31);
			this.LabelSeparator1.Name = "LabelSeparator1";
			this.LabelSeparator1.Size = new System.Drawing.Size(387, 2);
			this.LabelSeparator1.TabIndex = 3;
			// 
			// DTPCountdownTime
			// 
			this.DTPCountdownTime.Format = System.Windows.Forms.DateTimePickerFormat.Time;
			this.DTPCountdownTime.Location = new System.Drawing.Point(269, 6);
			this.DTPCountdownTime.Name = "DTPCountdownTime";
			this.DTPCountdownTime.Size = new System.Drawing.Size(127, 22);
			this.DTPCountdownTime.TabIndex = 2;
			this.DTPCountdownTime.ValueChanged += new System.EventHandler(this.DTPCountdownTime_ValueChanged);
			// 
			// DTPCountdownDate
			// 
			this.DTPCountdownDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.DTPCountdownDate.Location = new System.Drawing.Point(153, 6);
			this.DTPCountdownDate.Name = "DTPCountdownDate";
			this.DTPCountdownDate.Size = new System.Drawing.Size(110, 22);
			this.DTPCountdownDate.TabIndex = 1;
			this.DTPCountdownDate.ValueChanged += new System.EventHandler(this.DTPCountdownDate_ValueChanged);
			// 
			// LabelCountdownDateTime
			// 
			this.LabelCountdownDateTime.AutoSize = true;
			this.LabelCountdownDateTime.Location = new System.Drawing.Point(6, 9);
			this.LabelCountdownDateTime.Name = "LabelCountdownDateTime";
			this.LabelCountdownDateTime.Size = new System.Drawing.Size(141, 13);
			this.LabelCountdownDateTime.TabIndex = 0;
			this.LabelCountdownDateTime.Text = "Set countdown date/time:";
			// 
			// TabTimeBetween
			// 
			this.TabTimeBetween.Location = new System.Drawing.Point(4, 22);
			this.TabTimeBetween.Name = "TabTimeBetween";
			this.TabTimeBetween.Padding = new System.Windows.Forms.Padding(3);
			this.TabTimeBetween.Size = new System.Drawing.Size(402, 361);
			this.TabTimeBetween.TabIndex = 1;
			this.TabTimeBetween.Text = "Time Between";
			this.TabTimeBetween.UseVisualStyleBackColor = true;
			// 
			// MainTimer
			// 
			this.MainTimer.Interval = 1000;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(434, 411);
			this.Controls.Add(this.TabMain);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.Text = "Quick Countdown";
			this.TabMain.ResumeLayout(false);
			this.TabPageCountdown.ResumeLayout(false);
			this.TabPageCountdown.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl TabMain;
		private System.Windows.Forms.TabPage TabPageCountdown;
		private System.Windows.Forms.Label StaticLabelTimeRemaining;
		private System.Windows.Forms.Label LabelSeparator1;
		private System.Windows.Forms.DateTimePicker DTPCountdownTime;
		private System.Windows.Forms.DateTimePicker DTPCountdownDate;
		private System.Windows.Forms.Label LabelCountdownDateTime;
		private System.Windows.Forms.TabPage TabTimeBetween;
		private System.Windows.Forms.Timer MainTimer;
		private System.Windows.Forms.Label LabelRemaining;
		private System.Windows.Forms.Label LabelTotalTime;
		private System.Windows.Forms.Label StaticLabelTotalTime;
	}
}

