namespace QuickTag
{
	partial class FolderSelector
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
			this.StaticLabelPath = new System.Windows.Forms.Label();
			this.TextBoxPath = new System.Windows.Forms.TextBox();
			this.ButtonSelectFolder = new System.Windows.Forms.Button();
			this.ButtonCancel = new System.Windows.Forms.Button();
			this.ButtonOK = new System.Windows.Forms.Button();
			this.FBDFolderSelector = new System.Windows.Forms.FolderBrowserDialog();
			this.SuspendLayout();
			// 
			// StaticLabelPath
			// 
			this.StaticLabelPath.AutoSize = true;
			this.StaticLabelPath.Location = new System.Drawing.Point(13, 13);
			this.StaticLabelPath.Name = "StaticLabelPath";
			this.StaticLabelPath.Size = new System.Drawing.Size(33, 13);
			this.StaticLabelPath.TabIndex = 0;
			this.StaticLabelPath.Text = "Path:";
			// 
			// TextBoxPath
			// 
			this.TextBoxPath.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
			this.TextBoxPath.Location = new System.Drawing.Point(52, 10);
			this.TextBoxPath.Name = "TextBoxPath";
			this.TextBoxPath.Size = new System.Drawing.Size(192, 22);
			this.TextBoxPath.TabIndex = 1;
			// 
			// ButtonSelectFolder
			// 
			this.ButtonSelectFolder.Location = new System.Drawing.Point(250, 9);
			this.ButtonSelectFolder.Name = "ButtonSelectFolder";
			this.ButtonSelectFolder.Size = new System.Drawing.Size(26, 23);
			this.ButtonSelectFolder.TabIndex = 2;
			this.ButtonSelectFolder.Text = "...";
			this.ButtonSelectFolder.UseVisualStyleBackColor = true;
			this.ButtonSelectFolder.Click += new System.EventHandler(this.ButtonSelectFolder_Click);
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.Location = new System.Drawing.Point(201, 38);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = new System.Drawing.Size(75, 23);
			this.ButtonCancel.TabIndex = 3;
			this.ButtonCancel.Text = "C&ancel";
			this.ButtonCancel.UseVisualStyleBackColor = true;
			this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
			// 
			// ButtonOK
			// 
			this.ButtonOK.Location = new System.Drawing.Point(120, 38);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.Size = new System.Drawing.Size(75, 23);
			this.ButtonOK.TabIndex = 4;
			this.ButtonOK.Text = "&OK";
			this.ButtonOK.UseVisualStyleBackColor = true;
			this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// FBDFolderSelector
			// 
			this.FBDFolderSelector.RootFolder = System.Environment.SpecialFolder.MyComputer;
			this.FBDFolderSelector.ShowNewFolderButton = false;
			// 
			// FolderSelector
			// 
			this.AcceptButton = this.ButtonOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.ButtonCancel;
			this.ClientSize = new System.Drawing.Size(284, 70);
			this.Controls.Add(this.ButtonOK);
			this.Controls.Add(this.ButtonCancel);
			this.Controls.Add(this.ButtonSelectFolder);
			this.Controls.Add(this.TextBoxPath);
			this.Controls.Add(this.StaticLabelPath);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FolderSelector";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Select Folder";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label StaticLabelPath;
		private System.Windows.Forms.Button ButtonSelectFolder;
		private System.Windows.Forms.Button ButtonCancel;
		private System.Windows.Forms.Button ButtonOK;
		private System.Windows.Forms.FolderBrowserDialog FBDFolderSelector;
		public System.Windows.Forms.TextBox TextBoxPath;
	}
}