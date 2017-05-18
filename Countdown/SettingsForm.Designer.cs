namespace Countdown
{
	partial class SettingsForm
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
			this.CheckEnableNotifications = new System.Windows.Forms.CheckBox();
			this.StaticLabelTimeForm = new System.Windows.Forms.Label();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.StaticLabelDecimalPlaces = new System.Windows.Forms.Label();
			this.NUDDecimalPlaces = new System.Windows.Forms.NumericUpDown();
			this.ButtonCancel = new System.Windows.Forms.Button();
			this.ButtonOK = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.NUDDecimalPlaces)).BeginInit();
			this.SuspendLayout();
			// 
			// CheckEnableNotifications
			// 
			this.CheckEnableNotifications.AutoSize = true;
			this.CheckEnableNotifications.Location = new System.Drawing.Point(13, 13);
			this.CheckEnableNotifications.Name = "CheckEnableNotifications";
			this.CheckEnableNotifications.Size = new System.Drawing.Size(208, 17);
			this.CheckEnableNotifications.TabIndex = 0;
			this.CheckEnableNotifications.Text = "Enable Notifications On Milestones";
			this.CheckEnableNotifications.UseVisualStyleBackColor = true;
			// 
			// StaticLabelTimeForm
			// 
			this.StaticLabelTimeForm.AutoSize = true;
			this.StaticLabelTimeForm.Location = new System.Drawing.Point(13, 37);
			this.StaticLabelTimeForm.Name = "StaticLabelTimeForm";
			this.StaticLabelTimeForm.Size = new System.Drawing.Size(36, 13);
			this.StaticLabelTimeForm.TabIndex = 1;
			this.StaticLabelTimeForm.Text = "Form:";
			// 
			// comboBox1
			// 
			this.comboBox1.FormattingEnabled = true;
			this.comboBox1.Items.AddRange(new object[] {
            "Default",
            "Remaining Seconds",
            "Remaining Minutes",
            "Remaining Hours",
            "Remaining Days",
            "Remaining Weeks",
            "Remaining Years",
            "Decibels-Seconds",
            "XKCD 1017 Equation"});
			this.comboBox1.Location = new System.Drawing.Point(55, 34);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(166, 21);
			this.comboBox1.TabIndex = 2;
			// 
			// StaticLabelDecimalPlaces
			// 
			this.StaticLabelDecimalPlaces.AutoSize = true;
			this.StaticLabelDecimalPlaces.Location = new System.Drawing.Point(13, 65);
			this.StaticLabelDecimalPlaces.Name = "StaticLabelDecimalPlaces";
			this.StaticLabelDecimalPlaces.Size = new System.Drawing.Size(84, 13);
			this.StaticLabelDecimalPlaces.TabIndex = 3;
			this.StaticLabelDecimalPlaces.Text = "Decimal Places:";
			// 
			// NUDDecimalPlaces
			// 
			this.NUDDecimalPlaces.Location = new System.Drawing.Point(101, 63);
			this.NUDDecimalPlaces.Name = "NUDDecimalPlaces";
			this.NUDDecimalPlaces.Size = new System.Drawing.Size(53, 22);
			this.NUDDecimalPlaces.TabIndex = 4;
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.Location = new System.Drawing.Point(201, 99);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = new System.Drawing.Size(75, 23);
			this.ButtonCancel.TabIndex = 5;
			this.ButtonCancel.Text = "Cancel";
			this.ButtonCancel.UseVisualStyleBackColor = true;
			// 
			// ButtonOK
			// 
			this.ButtonOK.Location = new System.Drawing.Point(120, 99);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.Size = new System.Drawing.Size(75, 23);
			this.ButtonOK.TabIndex = 6;
			this.ButtonOK.Text = "OK";
			this.ButtonOK.UseVisualStyleBackColor = true;
			// 
			// SettingsForm
			// 
			this.AcceptButton = this.ButtonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.ButtonCancel;
			this.ClientSize = new System.Drawing.Size(284, 134);
			this.Controls.Add(this.ButtonOK);
			this.Controls.Add(this.ButtonCancel);
			this.Controls.Add(this.NUDDecimalPlaces);
			this.Controls.Add(this.StaticLabelDecimalPlaces);
			this.Controls.Add(this.comboBox1);
			this.Controls.Add(this.StaticLabelTimeForm);
			this.Controls.Add(this.CheckEnableNotifications);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "SettingsForm";
			this.Text = "Settings";
			((System.ComponentModel.ISupportInitialize)(this.NUDDecimalPlaces)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.CheckBox CheckEnableNotifications;
		private System.Windows.Forms.Label StaticLabelTimeForm;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.Label StaticLabelDecimalPlaces;
		private System.Windows.Forms.NumericUpDown NUDDecimalPlaces;
		private System.Windows.Forms.Button ButtonCancel;
		private System.Windows.Forms.Button ButtonOK;
	}
}