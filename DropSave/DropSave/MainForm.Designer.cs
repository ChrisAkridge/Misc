namespace DropSave
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.StaticLabelInstructions = new System.Windows.Forms.Label();
			this.LabelNextFile = new System.Windows.Forms.Label();
			this.ButtonOpenFolder = new System.Windows.Forms.Button();
			this.Notify = new System.Windows.Forms.NotifyIcon(this.components);
			this.ButtonUpdate = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// StaticLabelInstructions
			// 
			this.StaticLabelInstructions.AutoSize = true;
			this.StaticLabelInstructions.Location = new System.Drawing.Point(13, 13);
			this.StaticLabelInstructions.Name = "StaticLabelInstructions";
			this.StaticLabelInstructions.Size = new System.Drawing.Size(243, 26);
			this.StaticLabelInstructions.TabIndex = 0;
			this.StaticLabelInstructions.Text = "Drag and drop any image or file URL to save it\r\nto the Pictures folder.";
			// 
			// LabelNextFile
			// 
			this.LabelNextFile.AutoSize = true;
			this.LabelNextFile.Location = new System.Drawing.Point(13, 43);
			this.LabelNextFile.Name = "LabelNextFile";
			this.LabelNextFile.Size = new System.Drawing.Size(107, 13);
			this.LabelNextFile.TabIndex = 1;
			this.LabelNextFile.Text = "Next File: {filename}";
			// 
			// ButtonOpenFolder
			// 
			this.ButtonOpenFolder.Location = new System.Drawing.Point(16, 60);
			this.ButtonOpenFolder.Name = "ButtonOpenFolder";
			this.ButtonOpenFolder.Size = new System.Drawing.Size(240, 23);
			this.ButtonOpenFolder.TabIndex = 2;
			this.ButtonOpenFolder.Text = "&Open Folder...";
			this.ButtonOpenFolder.UseVisualStyleBackColor = true;
			this.ButtonOpenFolder.Click += new System.EventHandler(this.ButtonOpenFolder_Click);
			// 
			// Notify
			// 
			this.Notify.Icon = ((System.Drawing.Icon)(resources.GetObject("Notify.Icon")));
			this.Notify.Text = "DropSave - Click to Open";
			this.Notify.Visible = true;
			this.Notify.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.Notify_MouseDoubleClick);
			// 
			// ButtonUpdate
			// 
			this.ButtonUpdate.Location = new System.Drawing.Point(16, 90);
			this.ButtonUpdate.Name = "ButtonUpdate";
			this.ButtonUpdate.Size = new System.Drawing.Size(240, 23);
			this.ButtonUpdate.TabIndex = 3;
			this.ButtonUpdate.Text = "Update";
			this.ButtonUpdate.UseVisualStyleBackColor = true;
			this.ButtonUpdate.Click += new System.EventHandler(this.ButtonUpdate_Click);
			// 
			// MainForm
			// 
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(262, 118);
			this.Controls.Add(this.ButtonUpdate);
			this.Controls.Add(this.ButtonOpenFolder);
			this.Controls.Add(this.LabelNextFile);
			this.Controls.Add(this.StaticLabelInstructions);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.ShowInTaskbar = false;
			this.Text = "DropSave";
			this.TopMost = true;
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
			this.Resize += new System.EventHandler(this.MainForm_Resize);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label StaticLabelInstructions;
		private System.Windows.Forms.Label LabelNextFile;
		private System.Windows.Forms.Button ButtonOpenFolder;
		private System.Windows.Forms.NotifyIcon Notify;
		private System.Windows.Forms.Button ButtonUpdate;
	}
}

