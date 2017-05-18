namespace Countdown
{
	partial class SaveFilePathNotValidForm
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
			this.StaticLabelInfo = new System.Windows.Forms.Label();
			this.TextPath = new System.Windows.Forms.TextBox();
			this.ButtonSelectPath = new System.Windows.Forms.Button();
			this.ButtonOK = new System.Windows.Forms.Button();
			this.SFDPath = new System.Windows.Forms.SaveFileDialog();
			this.SuspendLayout();
			// 
			// StaticLabelInfo
			// 
			this.StaticLabelInfo.AutoSize = true;
			this.StaticLabelInfo.Location = new System.Drawing.Point(13, 13);
			this.StaticLabelInfo.Name = "StaticLabelInfo";
			this.StaticLabelInfo.Size = new System.Drawing.Size(158, 26);
			this.StaticLabelInfo.TabIndex = 0;
			this.StaticLabelInfo.Text = "The save file path is not valid.\r\nPlease select a valid path.";
			// 
			// TextPath
			// 
			this.TextPath.Location = new System.Drawing.Point(16, 43);
			this.TextPath.Name = "TextPath";
			this.TextPath.Size = new System.Drawing.Size(233, 22);
			this.TextPath.TabIndex = 1;
			// 
			// ButtonSelectPath
			// 
			this.ButtonSelectPath.Location = new System.Drawing.Point(255, 43);
			this.ButtonSelectPath.Name = "ButtonSelectPath";
			this.ButtonSelectPath.Size = new System.Drawing.Size(24, 22);
			this.ButtonSelectPath.TabIndex = 2;
			this.ButtonSelectPath.Text = "...";
			this.ButtonSelectPath.UseVisualStyleBackColor = true;
			this.ButtonSelectPath.Click += new System.EventHandler(this.ButtonSelectPath_Click);
			// 
			// ButtonOK
			// 
			this.ButtonOK.Location = new System.Drawing.Point(204, 71);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.Size = new System.Drawing.Size(75, 23);
			this.ButtonOK.TabIndex = 4;
			this.ButtonOK.Text = "OK";
			this.ButtonOK.UseVisualStyleBackColor = true;
			this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// SFDPath
			// 
			this.SFDPath.DefaultExt = "json";
			this.SFDPath.Filter = "JSON Files (*.json)|*.json|All files|*.*";
			// 
			// SaveFilePathNotValidForm
			// 
			this.AcceptButton = this.ButtonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 102);
			this.Controls.Add(this.ButtonOK);
			this.Controls.Add(this.ButtonSelectPath);
			this.Controls.Add(this.TextPath);
			this.Controls.Add(this.StaticLabelInfo);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SaveFilePathNotValidForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Countdown";
			this.Load += new System.EventHandler(this.SaveFilePathNotValidForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label StaticLabelInfo;
		private System.Windows.Forms.TextBox TextPath;
		private System.Windows.Forms.Button ButtonSelectPath;
		private System.Windows.Forms.Button ButtonOK;
		private System.Windows.Forms.SaveFileDialog SFDPath;
	}
}