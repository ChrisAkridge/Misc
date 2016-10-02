namespace TournamentOfPictures
{
	partial class StandingsForm
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
			this.StaticLabelStandings = new System.Windows.Forms.Label();
			this.TextStandings = new System.Windows.Forms.TextBox();
			this.ButtonOK = new System.Windows.Forms.Button();
			this.ButtonSavePlaylist = new System.Windows.Forms.Button();
			this.SFDSavePlaylist = new System.Windows.Forms.SaveFileDialog();
			this.SuspendLayout();
			// 
			// StaticLabelStandings
			// 
			this.StaticLabelStandings.AutoSize = true;
			this.StaticLabelStandings.Location = new System.Drawing.Point(13, 13);
			this.StaticLabelStandings.Name = "StaticLabelStandings";
			this.StaticLabelStandings.Size = new System.Drawing.Size(62, 13);
			this.StaticLabelStandings.TabIndex = 0;
			this.StaticLabelStandings.Text = "Standings:";
			// 
			// TextStandings
			// 
			this.TextStandings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TextStandings.BackColor = System.Drawing.Color.White;
			this.TextStandings.Location = new System.Drawing.Point(16, 30);
			this.TextStandings.Multiline = true;
			this.TextStandings.Name = "TextStandings";
			this.TextStandings.ReadOnly = true;
			this.TextStandings.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.TextStandings.Size = new System.Drawing.Size(556, 297);
			this.TextStandings.TabIndex = 1;
			// 
			// ButtonOK
			// 
			this.ButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonOK.Location = new System.Drawing.Point(497, 333);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.Size = new System.Drawing.Size(75, 23);
			this.ButtonOK.TabIndex = 2;
			this.ButtonOK.Text = "&OK";
			this.ButtonOK.UseVisualStyleBackColor = true;
			this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// ButtonSavePlaylist
			// 
			this.ButtonSavePlaylist.Location = new System.Drawing.Point(405, 333);
			this.ButtonSavePlaylist.Name = "ButtonSavePlaylist";
			this.ButtonSavePlaylist.Size = new System.Drawing.Size(86, 23);
			this.ButtonSavePlaylist.TabIndex = 3;
			this.ButtonSavePlaylist.Text = "&Save Playlist...";
			this.ButtonSavePlaylist.UseVisualStyleBackColor = true;
			this.ButtonSavePlaylist.Click += new System.EventHandler(this.ButtonSavePlaylist_Click);
			// 
			// SFDSavePlaylist
			// 
			this.SFDSavePlaylist.DefaultExt = "ivpl";
			this.SFDSavePlaylist.Filter = "ImageView Playlists (*.ivpl)|*.ivpl|All files|*.*";
			this.SFDSavePlaylist.Title = "Tournament of Pictures";
			// 
			// StandingsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(584, 361);
			this.Controls.Add(this.ButtonSavePlaylist);
			this.Controls.Add(this.ButtonOK);
			this.Controls.Add(this.TextStandings);
			this.Controls.Add(this.StaticLabelStandings);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "StandingsForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Standings";
			this.Load += new System.EventHandler(this.StandingsForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label StaticLabelStandings;
		private System.Windows.Forms.TextBox TextStandings;
		private System.Windows.Forms.Button ButtonOK;
		private System.Windows.Forms.Button ButtonSavePlaylist;
		private System.Windows.Forms.SaveFileDialog SFDSavePlaylist;
	}
}