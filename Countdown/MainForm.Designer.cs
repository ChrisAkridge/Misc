namespace Countdown
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
			this.ButtonAddDecimalPlace = new System.Windows.Forms.Button();
			this.ButtonSubtractDecimalPlace = new System.Windows.Forms.Button();
			this.ButtonChangeForm = new System.Windows.Forms.Button();
			this.LabelDecimalPlaces = new System.Windows.Forms.Label();
			this.ButtonRemoveEvent = new System.Windows.Forms.Button();
			this.ButtonAddEvent = new System.Windows.Forms.Button();
			this.UpdateTimer = new System.Windows.Forms.Timer(this.components);
			this.NotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
			this.ListEvents = new System.Windows.Forms.ListBox();
			this.SuspendLayout();
			// 
			// ButtonAddDecimalPlace
			// 
			this.ButtonAddDecimalPlace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonAddDecimalPlace.Location = new System.Drawing.Point(349, 13);
			this.ButtonAddDecimalPlace.Name = "ButtonAddDecimalPlace";
			this.ButtonAddDecimalPlace.Size = new System.Drawing.Size(23, 23);
			this.ButtonAddDecimalPlace.TabIndex = 0;
			this.ButtonAddDecimalPlace.Text = "+";
			this.ButtonAddDecimalPlace.UseVisualStyleBackColor = true;
			this.ButtonAddDecimalPlace.Click += new System.EventHandler(this.ButtonAddDecimalPlace_Click);
			// 
			// ButtonSubtractDecimalPlace
			// 
			this.ButtonSubtractDecimalPlace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonSubtractDecimalPlace.Location = new System.Drawing.Point(239, 13);
			this.ButtonSubtractDecimalPlace.Name = "ButtonSubtractDecimalPlace";
			this.ButtonSubtractDecimalPlace.Size = new System.Drawing.Size(23, 23);
			this.ButtonSubtractDecimalPlace.TabIndex = 1;
			this.ButtonSubtractDecimalPlace.Text = "-";
			this.ButtonSubtractDecimalPlace.UseVisualStyleBackColor = true;
			this.ButtonSubtractDecimalPlace.Click += new System.EventHandler(this.ButtonSubtractDecimalPlace_Click);
			// 
			// ButtonChangeForm
			// 
			this.ButtonChangeForm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonChangeForm.Location = new System.Drawing.Point(268, 13);
			this.ButtonChangeForm.Name = "ButtonChangeForm";
			this.ButtonChangeForm.Size = new System.Drawing.Size(75, 23);
			this.ButtonChangeForm.TabIndex = 2;
			this.ButtonChangeForm.Text = "Next Form";
			this.ButtonChangeForm.UseVisualStyleBackColor = true;
			this.ButtonChangeForm.Click += new System.EventHandler(this.ButtonChangeForm_Click);
			// 
			// LabelDecimalPlaces
			// 
			this.LabelDecimalPlaces.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.LabelDecimalPlaces.AutoSize = true;
			this.LabelDecimalPlaces.Location = new System.Drawing.Point(143, 18);
			this.LabelDecimalPlaces.Name = "LabelDecimalPlaces";
			this.LabelDecimalPlaces.Size = new System.Drawing.Size(90, 13);
			this.LabelDecimalPlaces.TabIndex = 3;
			this.LabelDecimalPlaces.Text = "2 decimal places";
			// 
			// ButtonRemoveEvent
			// 
			this.ButtonRemoveEvent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonRemoveEvent.Location = new System.Drawing.Point(284, 276);
			this.ButtonRemoveEvent.Name = "ButtonRemoveEvent";
			this.ButtonRemoveEvent.Size = new System.Drawing.Size(90, 23);
			this.ButtonRemoveEvent.TabIndex = 5;
			this.ButtonRemoveEvent.Text = "Remove Event";
			this.ButtonRemoveEvent.UseVisualStyleBackColor = true;
			// 
			// ButtonAddEvent
			// 
			this.ButtonAddEvent.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonAddEvent.Location = new System.Drawing.Point(188, 276);
			this.ButtonAddEvent.Name = "ButtonAddEvent";
			this.ButtonAddEvent.Size = new System.Drawing.Size(90, 23);
			this.ButtonAddEvent.TabIndex = 6;
			this.ButtonAddEvent.Text = "Add Event";
			this.ButtonAddEvent.UseVisualStyleBackColor = true;
			this.ButtonAddEvent.Click += new System.EventHandler(this.ButtonAddEvent_Click);
			// 
			// UpdateTimer
			// 
			this.UpdateTimer.Enabled = true;
			this.UpdateTimer.Interval = 1000;
			this.UpdateTimer.Tick += new System.EventHandler(this.UpdateTimer_Tick);
			// 
			// NotifyIcon
			// 
			this.NotifyIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
			this.NotifyIcon.Text = "Countdown";
			this.NotifyIcon.Visible = true;
			// 
			// ListEvents
			// 
			this.ListEvents.FormattingEnabled = true;
			this.ListEvents.Location = new System.Drawing.Point(12, 42);
			this.ListEvents.Name = "ListEvents";
			this.ListEvents.Size = new System.Drawing.Size(360, 225);
			this.ListEvents.TabIndex = 7;
			this.ListEvents.DoubleClick += new System.EventHandler(this.ListEvents_DoubleClick);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(384, 311);
			this.Controls.Add(this.ListEvents);
			this.Controls.Add(this.ButtonAddEvent);
			this.Controls.Add(this.ButtonRemoveEvent);
			this.Controls.Add(this.LabelDecimalPlaces);
			this.Controls.Add(this.ButtonChangeForm);
			this.Controls.Add(this.ButtonSubtractDecimalPlace);
			this.Controls.Add(this.ButtonAddDecimalPlace);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "MainForm";
			this.Text = "Countdown";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button ButtonAddDecimalPlace;
		private System.Windows.Forms.Button ButtonSubtractDecimalPlace;
		private System.Windows.Forms.Button ButtonChangeForm;
		private System.Windows.Forms.Label LabelDecimalPlaces;
		private System.Windows.Forms.Button ButtonRemoveEvent;
		private System.Windows.Forms.Button ButtonAddEvent;
		private System.Windows.Forms.Timer UpdateTimer;
		private System.Windows.Forms.NotifyIcon NotifyIcon;
		private System.Windows.Forms.ListBox ListEvents;
	}
}

