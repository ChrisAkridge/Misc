namespace TournamentOfPictures
{
	partial class ViewerForm
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
			this.LabelInfo = new System.Windows.Forms.Label();
			this.TrackImagePosition = new System.Windows.Forms.TrackBar();
			this.Picture = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.TrackImagePosition)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Picture)).BeginInit();
			this.SuspendLayout();
			// 
			// LabelInfo
			// 
			this.LabelInfo.AutoSize = true;
			this.LabelInfo.Location = new System.Drawing.Point(13, 13);
			this.LabelInfo.Name = "LabelInfo";
			this.LabelInfo.Size = new System.Drawing.Size(86, 26);
			this.LabelInfo.TabIndex = 0;
			this.LabelInfo.Text = "Picture {0} of {1}\r\nFilename: {2}";
			// 
			// TrackImagePosition
			// 
			this.TrackImagePosition.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TrackImagePosition.Location = new System.Drawing.Point(16, 204);
			this.TrackImagePosition.Minimum = 1;
			this.TrackImagePosition.Name = "TrackImagePosition";
			this.TrackImagePosition.Size = new System.Drawing.Size(256, 45);
			this.TrackImagePosition.TabIndex = 1;
			this.TrackImagePosition.Value = 1;
			this.TrackImagePosition.Scroll += new System.EventHandler(this.TrackImagePosition_Scroll);
			// 
			// Picture
			// 
			this.Picture.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.Picture.Location = new System.Drawing.Point(16, 42);
			this.Picture.Name = "Picture";
			this.Picture.Size = new System.Drawing.Size(256, 156);
			this.Picture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.Picture.TabIndex = 2;
			this.Picture.TabStop = false;
			// 
			// ViewerForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 261);
			this.Controls.Add(this.Picture);
			this.Controls.Add(this.TrackImagePosition);
			this.Controls.Add(this.LabelInfo);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "ViewerForm";
			this.ShowIcon = false;
			this.Text = "Image Viewer";
			((System.ComponentModel.ISupportInitialize)(this.TrackImagePosition)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Picture)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label LabelInfo;
		private System.Windows.Forms.TrackBar TrackImagePosition;
		private System.Windows.Forms.PictureBox Picture;
	}
}