namespace TournamentOfPictures
{
	partial class QuantificationForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QuantificationForm));
            this.ButtonSubmit = new System.Windows.Forms.Button();
            this.ButtonPlusOne = new System.Windows.Forms.Button();
            this.LabelScore = new System.Windows.Forms.Label();
            this.ButtonMinusOne = new System.Windows.Forms.Button();
            this.Picture = new System.Windows.Forms.PictureBox();
            this.StaticLabelInstructions = new System.Windows.Forms.Label();
            this.LabelInfo = new System.Windows.Forms.Label();
            this.Progress = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.Picture)).BeginInit();
            this.SuspendLayout();
            // 
            // ButtonSubmit
            // 
            this.ButtonSubmit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonSubmit.Location = new System.Drawing.Point(195, 222);
            this.ButtonSubmit.Name = "ButtonSubmit";
            this.ButtonSubmit.Size = new System.Drawing.Size(75, 23);
            this.ButtonSubmit.TabIndex = 11;
            this.ButtonSubmit.Text = "&Submit";
            this.ButtonSubmit.UseVisualStyleBackColor = true;
            this.ButtonSubmit.Click += new System.EventHandler(this.ButtonSubmit_Click);
            // 
            // ButtonPlusOne
            // 
            this.ButtonPlusOne.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ButtonPlusOne.Location = new System.Drawing.Point(82, 222);
            this.ButtonPlusOne.Name = "ButtonPlusOne";
            this.ButtonPlusOne.Size = new System.Drawing.Size(30, 23);
            this.ButtonPlusOne.TabIndex = 10;
            this.ButtonPlusOne.Text = "+1";
            this.ButtonPlusOne.UseVisualStyleBackColor = true;
            this.ButtonPlusOne.Click += new System.EventHandler(this.ButtonPlusOne_Click);
            // 
            // LabelScore
            // 
            this.LabelScore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LabelScore.AutoSize = true;
            this.LabelScore.Location = new System.Drawing.Point(51, 227);
            this.LabelScore.Name = "LabelScore";
            this.LabelScore.Size = new System.Drawing.Size(13, 13);
            this.LabelScore.TabIndex = 9;
            this.LabelScore.Text = "0";
            // 
            // ButtonMinusOne
            // 
            this.ButtonMinusOne.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ButtonMinusOne.Location = new System.Drawing.Point(15, 222);
            this.ButtonMinusOne.Name = "ButtonMinusOne";
            this.ButtonMinusOne.Size = new System.Drawing.Size(30, 23);
            this.ButtonMinusOne.TabIndex = 8;
            this.ButtonMinusOne.Text = "-1";
            this.ButtonMinusOne.UseVisualStyleBackColor = true;
            this.ButtonMinusOne.Click += new System.EventHandler(this.ButtonMinusOne_Click);
            // 
            // Picture
            // 
            this.Picture.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Picture.Location = new System.Drawing.Point(15, 39);
            this.Picture.Name = "Picture";
            this.Picture.Size = new System.Drawing.Size(256, 176);
            this.Picture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.Picture.TabIndex = 7;
            this.Picture.TabStop = false;
            // 
            // StaticLabelInstructions
            // 
            this.StaticLabelInstructions.AutoSize = true;
            this.StaticLabelInstructions.Location = new System.Drawing.Point(12, 9);
            this.StaticLabelInstructions.Name = "StaticLabelInstructions";
            this.StaticLabelInstructions.Size = new System.Drawing.Size(258, 26);
            this.StaticLabelInstructions.TabIndex = 6;
            this.StaticLabelInstructions.Text = "Evaluate the image and add one for every quality\r\nand subtract one for every flaw" +
    ".";
            // 
            // LabelInfo
            // 
            this.LabelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LabelInfo.AutoSize = true;
            this.LabelInfo.Location = new System.Drawing.Point(15, 252);
            this.LabelInfo.Name = "LabelInfo";
            this.LabelInfo.Size = new System.Drawing.Size(89, 13);
            this.LabelInfo.TabIndex = 12;
            this.LabelInfo.Text = "Picture {0} of {1}.";
            // 
            // Progress
            // 
            this.Progress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Progress.Location = new System.Drawing.Point(18, 268);
            this.Progress.Name = "Progress";
            this.Progress.Size = new System.Drawing.Size(252, 23);
            this.Progress.TabIndex = 13;
            // 
            // QuantificationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 306);
            this.Controls.Add(this.Progress);
            this.Controls.Add(this.LabelInfo);
            this.Controls.Add(this.ButtonSubmit);
            this.Controls.Add(this.ButtonPlusOne);
            this.Controls.Add(this.LabelScore);
            this.Controls.Add(this.ButtonMinusOne);
            this.Controls.Add(this.Picture);
            this.Controls.Add(this.StaticLabelInstructions);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "QuantificationForm";
            this.Text = "Quantification Tournament";
            ((System.ComponentModel.ISupportInitialize)(this.Picture)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button ButtonSubmit;
		private System.Windows.Forms.Button ButtonPlusOne;
		private System.Windows.Forms.Label LabelScore;
		private System.Windows.Forms.Button ButtonMinusOne;
		private System.Windows.Forms.PictureBox Picture;
		private System.Windows.Forms.Label StaticLabelInstructions;
		private System.Windows.Forms.Label LabelInfo;
		private System.Windows.Forms.ProgressBar Progress;
	}
}