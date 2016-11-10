namespace TournamentOfPictures
{
	partial class QuickSortForm
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
			this.LabelInformation = new System.Windows.Forms.Label();
			this.TLPPictures = new System.Windows.Forms.TableLayoutPanel();
			this.PictureLeft = new System.Windows.Forms.PictureBox();
			this.PictureRight = new System.Windows.Forms.PictureBox();
			this.SortDelayTimer = new System.Windows.Forms.Timer(this.components);
			this.TLPPictures.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PictureLeft)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PictureRight)).BeginInit();
			this.SuspendLayout();
			// 
			// LabelInformation
			// 
			this.LabelInformation.AutoSize = true;
			this.LabelInformation.Location = new System.Drawing.Point(13, 416);
			this.LabelInformation.Name = "LabelInformation";
			this.LabelInformation.Size = new System.Drawing.Size(392, 13);
			this.LabelInformation.TabIndex = 0;
			this.LabelInformation.Text = "{0} comparisons ({1} user-directed, {2} computer-made). {3} rounds complete.";
			// 
			// TLPPictures
			// 
			this.TLPPictures.ColumnCount = 2;
			this.TLPPictures.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLPPictures.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLPPictures.Controls.Add(this.PictureLeft, 0, 0);
			this.TLPPictures.Controls.Add(this.PictureRight, 1, 0);
			this.TLPPictures.Location = new System.Drawing.Point(13, 13);
			this.TLPPictures.Name = "TLPPictures";
			this.TLPPictures.RowCount = 1;
			this.TLPPictures.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.TLPPictures.Size = new System.Drawing.Size(599, 400);
			this.TLPPictures.TabIndex = 1;
			// 
			// PictureLeft
			// 
			this.PictureLeft.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PictureLeft.Location = new System.Drawing.Point(3, 3);
			this.PictureLeft.Name = "PictureLeft";
			this.PictureLeft.Size = new System.Drawing.Size(293, 394);
			this.PictureLeft.TabIndex = 0;
			this.PictureLeft.TabStop = false;
			this.PictureLeft.Click += new System.EventHandler(this.PictureLeft_Click);
			// 
			// PictureRight
			// 
			this.PictureRight.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PictureRight.Location = new System.Drawing.Point(302, 3);
			this.PictureRight.Name = "PictureRight";
			this.PictureRight.Size = new System.Drawing.Size(294, 394);
			this.PictureRight.TabIndex = 1;
			this.PictureRight.TabStop = false;
			this.PictureRight.Click += new System.EventHandler(this.PictureRight_Click);
			// 
			// SortDelayTimer
			// 
			this.SortDelayTimer.Enabled = true;
			this.SortDelayTimer.Interval = 1000;
			this.SortDelayTimer.Tick += new System.EventHandler(this.SortDelayTimer_Tick);
			// 
			// QuickSortForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(624, 441);
			this.Controls.Add(this.TLPPictures);
			this.Controls.Add(this.LabelInformation);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MaximumSize = new System.Drawing.Size(1920, 1080);
			this.MinimumSize = new System.Drawing.Size(450, 480);
			this.Name = "QuickSortForm";
			this.ShowIcon = false;
			this.Text = "Quick Sorter";
			this.Load += new System.EventHandler(this.QuickSortForm_Load);
			this.TLPPictures.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PictureLeft)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PictureRight)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label LabelInformation;
		private System.Windows.Forms.TableLayoutPanel TLPPictures;
		private System.Windows.Forms.PictureBox PictureLeft;
		private System.Windows.Forms.PictureBox PictureRight;
		private System.Windows.Forms.Timer SortDelayTimer;
	}
}