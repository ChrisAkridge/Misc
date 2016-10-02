namespace TournamentOfPictures
{
	partial class LauncherForm
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
			this.LabelFolderDetails = new System.Windows.Forms.Label();
			this.ButtonLaunchBracketedTournament = new System.Windows.Forms.Button();
			this.ButtonLaunchRating = new System.Windows.Forms.Button();
			this.ButtonLaunchRecursiveRating = new System.Windows.Forms.Button();
			this.ButtonLaunchQuantificationTournament = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// LabelFolderDetails
			// 
			this.LabelFolderDetails.AutoSize = true;
			this.LabelFolderDetails.Location = new System.Drawing.Point(13, 13);
			this.LabelFolderDetails.Name = "LabelFolderDetails";
			this.LabelFolderDetails.Size = new System.Drawing.Size(226, 65);
			this.LabelFolderDetails.TabIndex = 0;
			this.LabelFolderDetails.Text = "Selected Folder \"{name}\"\r\nPictures: {pictureCount}\r\nn-1: {pictureCount-1}\r\nn log " +
    "n: {pictureCount log_2 pictureCount}\r\nn^2: {pictureCount^2}";
			// 
			// ButtonLaunchBracketedTournament
			// 
			this.ButtonLaunchBracketedTournament.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonLaunchBracketedTournament.Location = new System.Drawing.Point(16, 82);
			this.ButtonLaunchBracketedTournament.Name = "ButtonLaunchBracketedTournament";
			this.ButtonLaunchBracketedTournament.Size = new System.Drawing.Size(425, 23);
			this.ButtonLaunchBracketedTournament.TabIndex = 1;
			this.ButtonLaunchBracketedTournament.Text = "Launch Bracketed Tournament...";
			this.ButtonLaunchBracketedTournament.UseVisualStyleBackColor = true;
			this.ButtonLaunchBracketedTournament.Click += new System.EventHandler(this.ButtonLaunchBracketedTournament_Click);
			// 
			// ButtonLaunchRating
			// 
			this.ButtonLaunchRating.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonLaunchRating.Location = new System.Drawing.Point(16, 112);
			this.ButtonLaunchRating.Name = "ButtonLaunchRating";
			this.ButtonLaunchRating.Size = new System.Drawing.Size(425, 23);
			this.ButtonLaunchRating.TabIndex = 2;
			this.ButtonLaunchRating.Text = "Launch Picture Rating Selector...";
			this.ButtonLaunchRating.UseVisualStyleBackColor = true;
			this.ButtonLaunchRating.Click += new System.EventHandler(this.ButtonLaunchRating_Click);
			// 
			// ButtonLaunchRecursiveRating
			// 
			this.ButtonLaunchRecursiveRating.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonLaunchRecursiveRating.Location = new System.Drawing.Point(16, 142);
			this.ButtonLaunchRecursiveRating.Name = "ButtonLaunchRecursiveRating";
			this.ButtonLaunchRecursiveRating.Size = new System.Drawing.Size(425, 23);
			this.ButtonLaunchRecursiveRating.TabIndex = 3;
			this.ButtonLaunchRecursiveRating.Text = "Launch Recursive Picture Rating Selector...";
			this.ButtonLaunchRecursiveRating.UseVisualStyleBackColor = true;
			this.ButtonLaunchRecursiveRating.Click += new System.EventHandler(this.ButtonLaunchRecursiveRating_Click);
			// 
			// ButtonLaunchQuantificationTournament
			// 
			this.ButtonLaunchQuantificationTournament.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonLaunchQuantificationTournament.Location = new System.Drawing.Point(16, 171);
			this.ButtonLaunchQuantificationTournament.Name = "ButtonLaunchQuantificationTournament";
			this.ButtonLaunchQuantificationTournament.Size = new System.Drawing.Size(425, 23);
			this.ButtonLaunchQuantificationTournament.TabIndex = 4;
			this.ButtonLaunchQuantificationTournament.Text = "Launch Quantification Tournament...";
			this.ButtonLaunchQuantificationTournament.UseVisualStyleBackColor = true;
			this.ButtonLaunchQuantificationTournament.Click += new System.EventHandler(this.ButtonLaunchQuantificationTournament_Click);
			// 
			// LauncherForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(453, 206);
			this.Controls.Add(this.ButtonLaunchQuantificationTournament);
			this.Controls.Add(this.ButtonLaunchRecursiveRating);
			this.Controls.Add(this.ButtonLaunchRating);
			this.Controls.Add(this.ButtonLaunchBracketedTournament);
			this.Controls.Add(this.LabelFolderDetails);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "LauncherForm";
			this.Text = "Tournament of Pictures";
			this.Load += new System.EventHandler(this.LauncherForm_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label LabelFolderDetails;
		private System.Windows.Forms.Button ButtonLaunchBracketedTournament;
		private System.Windows.Forms.Button ButtonLaunchRating;
		private System.Windows.Forms.Button ButtonLaunchRecursiveRating;
		private System.Windows.Forms.Button ButtonLaunchQuantificationTournament;
	}
}