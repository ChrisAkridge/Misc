namespace QuickTag
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
			this.ButtonAddFolders = new System.Windows.Forms.Button();
			this.TreeViewFiles = new System.Windows.Forms.TreeView();
			this.StaticLabelFolders = new System.Windows.Forms.Label();
			this.StaticLabelTags = new System.Windows.Forms.Label();
			this.TreeViewTags = new System.Windows.Forms.TreeView();
			this.PictureBox = new System.Windows.Forms.PictureBox();
			this.StaticLabelImageTags = new System.Windows.Forms.Label();
			this.TextBoxImageTags = new System.Windows.Forms.TextBox();
			this.ButtonSaveAndClose = new System.Windows.Forms.Button();
			this.ButtonSetTags = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
			this.SuspendLayout();
			// 
			// ButtonAddFolders
			// 
			this.ButtonAddFolders.Location = new System.Drawing.Point(12, 13);
			this.ButtonAddFolders.Name = "ButtonAddFolders";
			this.ButtonAddFolders.Size = new System.Drawing.Size(181, 23);
			this.ButtonAddFolders.TabIndex = 0;
			this.ButtonAddFolders.Text = "&Add Folders...";
			this.ButtonAddFolders.UseVisualStyleBackColor = true;
			this.ButtonAddFolders.Click += new System.EventHandler(this.ButtonAddFolders_Click);
			// 
			// TreeViewFiles
			// 
			this.TreeViewFiles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.TreeViewFiles.Location = new System.Drawing.Point(12, 55);
			this.TreeViewFiles.Name = "TreeViewFiles";
			this.TreeViewFiles.Size = new System.Drawing.Size(181, 458);
			this.TreeViewFiles.TabIndex = 1;
			this.TreeViewFiles.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewFiles_AfterSelect);
			// 
			// StaticLabelFolders
			// 
			this.StaticLabelFolders.AutoSize = true;
			this.StaticLabelFolders.Location = new System.Drawing.Point(12, 39);
			this.StaticLabelFolders.Name = "StaticLabelFolders";
			this.StaticLabelFolders.Size = new System.Drawing.Size(48, 13);
			this.StaticLabelFolders.TabIndex = 2;
			this.StaticLabelFolders.Text = "Folders:";
			// 
			// StaticLabelTags
			// 
			this.StaticLabelTags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.StaticLabelTags.AutoSize = true;
			this.StaticLabelTags.Location = new System.Drawing.Point(12, 516);
			this.StaticLabelTags.Name = "StaticLabelTags";
			this.StaticLabelTags.Size = new System.Drawing.Size(33, 13);
			this.StaticLabelTags.TabIndex = 3;
			this.StaticLabelTags.Text = "Tags:";
			// 
			// TreeViewTags
			// 
			this.TreeViewTags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.TreeViewTags.Location = new System.Drawing.Point(12, 532);
			this.TreeViewTags.Name = "TreeViewTags";
			this.TreeViewTags.Size = new System.Drawing.Size(181, 185);
			this.TreeViewTags.TabIndex = 4;
			this.TreeViewTags.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeViewTags_AfterSelect);
			// 
			// PictureBox
			// 
			this.PictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.PictureBox.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.PictureBox.Location = new System.Drawing.Point(202, 13);
			this.PictureBox.Name = "PictureBox";
			this.PictureBox.Size = new System.Drawing.Size(794, 683);
			this.PictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.PictureBox.TabIndex = 5;
			this.PictureBox.TabStop = false;
			this.PictureBox.Click += new System.EventHandler(this.PictureBox_Click);
			// 
			// StaticLabelImageTags
			// 
			this.StaticLabelImageTags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.StaticLabelImageTags.AutoSize = true;
			this.StaticLabelImageTags.Location = new System.Drawing.Point(199, 703);
			this.StaticLabelImageTags.Name = "StaticLabelImageTags";
			this.StaticLabelImageTags.Size = new System.Drawing.Size(66, 13);
			this.StaticLabelImageTags.TabIndex = 6;
			this.StaticLabelImageTags.Text = "Image tags:";
			// 
			// TextBoxImageTags
			// 
			this.TextBoxImageTags.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TextBoxImageTags.Location = new System.Drawing.Point(271, 700);
			this.TextBoxImageTags.Name = "TextBoxImageTags";
			this.TextBoxImageTags.Size = new System.Drawing.Size(576, 22);
			this.TextBoxImageTags.TabIndex = 7;
			this.TextBoxImageTags.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxImageTags_KeyDown);
			// 
			// ButtonSaveAndClose
			// 
			this.ButtonSaveAndClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonSaveAndClose.Location = new System.Drawing.Point(892, 698);
			this.ButtonSaveAndClose.Name = "ButtonSaveAndClose";
			this.ButtonSaveAndClose.Size = new System.Drawing.Size(104, 23);
			this.ButtonSaveAndClose.TabIndex = 8;
			this.ButtonSaveAndClose.Text = "&Save and Close...";
			this.ButtonSaveAndClose.UseVisualStyleBackColor = true;
			this.ButtonSaveAndClose.Click += new System.EventHandler(this.ButtonSaveAndClose_Click);
			// 
			// ButtonSetTags
			// 
			this.ButtonSetTags.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonSetTags.Location = new System.Drawing.Point(853, 698);
			this.ButtonSetTags.Name = "ButtonSetTags";
			this.ButtonSetTags.Size = new System.Drawing.Size(33, 23);
			this.ButtonSetTags.TabIndex = 9;
			this.ButtonSetTags.Text = "&Set";
			this.ButtonSetTags.UseVisualStyleBackColor = true;
			this.ButtonSetTags.Click += new System.EventHandler(this.ButtonSetTags_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1008, 729);
			this.Controls.Add(this.ButtonSetTags);
			this.Controls.Add(this.ButtonSaveAndClose);
			this.Controls.Add(this.TextBoxImageTags);
			this.Controls.Add(this.StaticLabelImageTags);
			this.Controls.Add(this.PictureBox);
			this.Controls.Add(this.TreeViewTags);
			this.Controls.Add(this.StaticLabelTags);
			this.Controls.Add(this.StaticLabelFolders);
			this.Controls.Add(this.TreeViewFiles);
			this.Controls.Add(this.ButtonAddFolders);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MinimumSize = new System.Drawing.Size(326, 337);
			this.Name = "MainForm";
			this.Text = "Quick Tag - {DB Name}";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button ButtonAddFolders;
		private System.Windows.Forms.TreeView TreeViewFiles;
		private System.Windows.Forms.Label StaticLabelFolders;
		private System.Windows.Forms.Label StaticLabelTags;
		private System.Windows.Forms.TreeView TreeViewTags;
		private System.Windows.Forms.PictureBox PictureBox;
		private System.Windows.Forms.Label StaticLabelImageTags;
		private System.Windows.Forms.TextBox TextBoxImageTags;
		private System.Windows.Forms.Button ButtonSaveAndClose;
		private System.Windows.Forms.Button ButtonSetTags;
	}
}