namespace AsymptoticCountdown
{
	partial class Form1
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
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.DTPStart = new System.Windows.Forms.DateTimePicker();
			this.DTPEnd = new System.Windows.Forms.DateTimePicker();
			this.LabelProgress = new System.Windows.Forms.Label();
			this.LabelAsymptoticProgress = new System.Windows.Forms.Label();
			this.TimerMain = new System.Windows.Forms.Timer(this.components);
			this.TimerRate = new System.Windows.Forms.Timer(this.components);
			this.ProgressOverall = new System.Windows.Forms.ProgressBar();
			this.ProgressHundredth = new System.Windows.Forms.ProgressBar();
			this.ProgressTenThousandth = new System.Windows.Forms.ProgressBar();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(34, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Start:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(13, 38);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(30, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "End:";
			// 
			// DTPStart
			// 
			this.DTPStart.CustomFormat = "yyyy-MM-dd hh:mm:ss tt";
			this.DTPStart.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.DTPStart.Location = new System.Drawing.Point(53, 6);
			this.DTPStart.Name = "DTPStart";
			this.DTPStart.Size = new System.Drawing.Size(200, 22);
			this.DTPStart.TabIndex = 2;
			this.DTPStart.ValueChanged += new System.EventHandler(this.DTPStart_ValueChanged);
			// 
			// DTPEnd
			// 
			this.DTPEnd.CustomFormat = "yyyy-MM-dd hh:mm:ss tt";
			this.DTPEnd.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.DTPEnd.Location = new System.Drawing.Point(53, 31);
			this.DTPEnd.Name = "DTPEnd";
			this.DTPEnd.Size = new System.Drawing.Size(200, 22);
			this.DTPEnd.TabIndex = 3;
			this.DTPEnd.ValueChanged += new System.EventHandler(this.dateTimePicker1_ValueChanged);
			// 
			// LabelProgress
			// 
			this.LabelProgress.AutoSize = true;
			this.LabelProgress.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LabelProgress.Location = new System.Drawing.Point(12, 56);
			this.LabelProgress.Name = "LabelProgress";
			this.LabelProgress.Size = new System.Drawing.Size(164, 21);
			this.LabelProgress.TabIndex = 4;
			this.LabelProgress.Text = "Overall Progress: {0}%";
			// 
			// LabelAsymptoticProgress
			// 
			this.LabelAsymptoticProgress.AutoSize = true;
			this.LabelAsymptoticProgress.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LabelAsymptoticProgress.Location = new System.Drawing.Point(12, 77);
			this.LabelAsymptoticProgress.Name = "LabelAsymptoticProgress";
			this.LabelAsymptoticProgress.Size = new System.Drawing.Size(232, 21);
			this.LabelAsymptoticProgress.TabIndex = 5;
			this.LabelAsymptoticProgress.Text = "Asymptotic Progress: {0} {1} ago";
			// 
			// TimerMain
			// 
			this.TimerMain.Enabled = true;
			this.TimerMain.Interval = 16;
			this.TimerMain.Tick += new System.EventHandler(this.TimerMain_Tick);
			// 
			// TimerRate
			// 
			this.TimerRate.Enabled = true;
			this.TimerRate.Tick += new System.EventHandler(this.TimerRate_Tick);
			// 
			// ProgressOverall
			// 
			this.ProgressOverall.Location = new System.Drawing.Point(16, 102);
			this.ProgressOverall.Maximum = 10000;
			this.ProgressOverall.Name = "ProgressOverall";
			this.ProgressOverall.Size = new System.Drawing.Size(403, 23);
			this.ProgressOverall.TabIndex = 6;
			// 
			// ProgressHundredth
			// 
			this.ProgressHundredth.Location = new System.Drawing.Point(16, 131);
			this.ProgressHundredth.Name = "ProgressHundredth";
			this.ProgressHundredth.Size = new System.Drawing.Size(403, 23);
			this.ProgressHundredth.TabIndex = 7;
			// 
			// ProgressTenThousandth
			// 
			this.ProgressTenThousandth.Location = new System.Drawing.Point(16, 160);
			this.ProgressTenThousandth.Name = "ProgressTenThousandth";
			this.ProgressTenThousandth.Size = new System.Drawing.Size(403, 23);
			this.ProgressTenThousandth.TabIndex = 8;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(431, 196);
			this.Controls.Add(this.ProgressTenThousandth);
			this.Controls.Add(this.ProgressHundredth);
			this.Controls.Add(this.ProgressOverall);
			this.Controls.Add(this.LabelAsymptoticProgress);
			this.Controls.Add(this.LabelProgress);
			this.Controls.Add(this.DTPEnd);
			this.Controls.Add(this.DTPStart);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "Form1";
			this.Text = "Asymptotic Countdown";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.DateTimePicker DTPStart;
		private System.Windows.Forms.DateTimePicker DTPEnd;
		private System.Windows.Forms.Label LabelProgress;
		private System.Windows.Forms.Label LabelAsymptoticProgress;
		private System.Windows.Forms.Timer TimerMain;
		private System.Windows.Forms.Timer TimerRate;
		private System.Windows.Forms.ProgressBar ProgressOverall;
		private System.Windows.Forms.ProgressBar ProgressHundredth;
		private System.Windows.Forms.ProgressBar ProgressTenThousandth;
	}
}

