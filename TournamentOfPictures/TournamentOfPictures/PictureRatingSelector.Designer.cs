namespace TournamentOfPictures
{
	partial class PictureRatingSelector
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PictureRatingSelector));
            this.Picture = new System.Windows.Forms.PictureBox();
            this.ButtonRate1 = new System.Windows.Forms.Button();
            this.ButtonRate2 = new System.Windows.Forms.Button();
            this.ButtonRate3 = new System.Windows.Forms.Button();
            this.ButtonRate4 = new System.Windows.Forms.Button();
            this.ButtonRate5 = new System.Windows.Forms.Button();
            this.LabelCurrentScoreGroup = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.Picture)).BeginInit();
            this.SuspendLayout();
            // 
            // Picture
            // 
            this.Picture.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Picture.Location = new System.Drawing.Point(13, 13);
            this.Picture.Name = "Picture";
            this.Picture.Size = new System.Drawing.Size(275, 209);
            this.Picture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.Picture.TabIndex = 0;
            this.Picture.TabStop = false;
            // 
            // ButtonRate1
            // 
            this.ButtonRate1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ButtonRate1.Location = new System.Drawing.Point(13, 226);
            this.ButtonRate1.Name = "ButtonRate1";
            this.ButtonRate1.Size = new System.Drawing.Size(23, 23);
            this.ButtonRate1.TabIndex = 1;
            this.ButtonRate1.Text = "1";
            this.ButtonRate1.UseVisualStyleBackColor = true;
            this.ButtonRate1.Click += new System.EventHandler(this.ButtonRate1_Click);
            // 
            // ButtonRate2
            // 
            this.ButtonRate2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ButtonRate2.Location = new System.Drawing.Point(42, 226);
            this.ButtonRate2.Name = "ButtonRate2";
            this.ButtonRate2.Size = new System.Drawing.Size(23, 23);
            this.ButtonRate2.TabIndex = 2;
            this.ButtonRate2.Text = "2";
            this.ButtonRate2.UseVisualStyleBackColor = true;
            this.ButtonRate2.Click += new System.EventHandler(this.ButtonRate2_Click);
            // 
            // ButtonRate3
            // 
            this.ButtonRate3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ButtonRate3.Location = new System.Drawing.Point(71, 226);
            this.ButtonRate3.Name = "ButtonRate3";
            this.ButtonRate3.Size = new System.Drawing.Size(23, 23);
            this.ButtonRate3.TabIndex = 3;
            this.ButtonRate3.Text = "3";
            this.ButtonRate3.UseVisualStyleBackColor = true;
            this.ButtonRate3.Click += new System.EventHandler(this.ButtonRate3_Click);
            // 
            // ButtonRate4
            // 
            this.ButtonRate4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ButtonRate4.Location = new System.Drawing.Point(100, 226);
            this.ButtonRate4.Name = "ButtonRate4";
            this.ButtonRate4.Size = new System.Drawing.Size(23, 23);
            this.ButtonRate4.TabIndex = 4;
            this.ButtonRate4.Text = "4";
            this.ButtonRate4.UseVisualStyleBackColor = true;
            this.ButtonRate4.Click += new System.EventHandler(this.ButtonRate4_Click);
            // 
            // ButtonRate5
            // 
            this.ButtonRate5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ButtonRate5.Location = new System.Drawing.Point(129, 226);
            this.ButtonRate5.Name = "ButtonRate5";
            this.ButtonRate5.Size = new System.Drawing.Size(23, 23);
            this.ButtonRate5.TabIndex = 5;
            this.ButtonRate5.Text = "5";
            this.ButtonRate5.UseVisualStyleBackColor = true;
            this.ButtonRate5.Click += new System.EventHandler(this.ButtonRate5_Click);
            // 
            // LabelCurrentScoreGroup
            // 
            this.LabelCurrentScoreGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.LabelCurrentScoreGroup.AutoSize = true;
            this.LabelCurrentScoreGroup.Location = new System.Drawing.Point(160, 231);
            this.LabelCurrentScoreGroup.Name = "LabelCurrentScoreGroup";
            this.LabelCurrentScoreGroup.Size = new System.Drawing.Size(0, 13);
            this.LabelCurrentScoreGroup.TabIndex = 6;
            // 
            // PictureRatingSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 261);
            this.Controls.Add(this.LabelCurrentScoreGroup);
            this.Controls.Add(this.ButtonRate5);
            this.Controls.Add(this.ButtonRate4);
            this.Controls.Add(this.ButtonRate3);
            this.Controls.Add(this.ButtonRate2);
            this.Controls.Add(this.ButtonRate1);
            this.Controls.Add(this.Picture);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PictureRatingSelector";
            this.Text = "Picture Rating Selector";
            this.Load += new System.EventHandler(this.PictureRatingSelector_Load);
            ((System.ComponentModel.ISupportInitialize)(this.Picture)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox Picture;
		private System.Windows.Forms.Button ButtonRate1;
		private System.Windows.Forms.Button ButtonRate2;
		private System.Windows.Forms.Button ButtonRate3;
		private System.Windows.Forms.Button ButtonRate4;
		private System.Windows.Forms.Button ButtonRate5;
		private System.Windows.Forms.Label LabelCurrentScoreGroup;
	}
}