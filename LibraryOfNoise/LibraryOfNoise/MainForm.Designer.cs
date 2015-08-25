namespace LibraryOfNoise
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
			this.LabelSeed = new System.Windows.Forms.Label();
			this.NUDSetNumber = new System.Windows.Forms.NumericUpDown();
			this.LabelPage = new System.Windows.Forms.Label();
			this.NUDPageNumber = new System.Windows.Forms.NumericUpDown();
			this.TextPage = new System.Windows.Forms.TextBox();
			this.ButtonSearch = new System.Windows.Forms.Button();
			this.ButtonRandomPage = new System.Windows.Forms.Button();
			this.ButtonGo = new System.Windows.Forms.Button();
			this.TextSearch = new System.Windows.Forms.TextBox();
			this.StaticLabelSet = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.NUDSetNumber)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NUDPageNumber)).BeginInit();
			this.SuspendLayout();
			// 
			// LabelSeed
			// 
			this.LabelSeed.AutoSize = true;
			this.LabelSeed.Location = new System.Drawing.Point(13, 13);
			this.LabelSeed.Name = "LabelSeed";
			this.LabelSeed.Size = new System.Drawing.Size(50, 13);
			this.LabelSeed.TabIndex = 0;
			this.LabelSeed.Text = "Seed: {0}";
			// 
			// NUDSetNumber
			// 
			this.NUDSetNumber.Location = new System.Drawing.Point(47, 29);
			this.NUDSetNumber.Maximum = new decimal(new int[] {
            -1,
            -1,
            0,
            0});
			this.NUDSetNumber.Name = "NUDSetNumber";
			this.NUDSetNumber.Size = new System.Drawing.Size(164, 22);
			this.NUDSetNumber.TabIndex = 2;
			this.NUDSetNumber.ValueChanged += new System.EventHandler(this.NUDSetNumber_ValueChanged);
			// 
			// LabelPage
			// 
			this.LabelPage.AutoSize = true;
			this.LabelPage.Location = new System.Drawing.Point(217, 31);
			this.LabelPage.Name = "LabelPage";
			this.LabelPage.Size = new System.Drawing.Size(32, 13);
			this.LabelPage.TabIndex = 3;
			this.LabelPage.Text = "Page";
			// 
			// NUDPageNumber
			// 
			this.NUDPageNumber.Location = new System.Drawing.Point(255, 29);
			this.NUDPageNumber.Maximum = new decimal(new int[] {
            -1,
            -1,
            0,
            0});
			this.NUDPageNumber.Name = "NUDPageNumber";
			this.NUDPageNumber.Size = new System.Drawing.Size(148, 22);
			this.NUDPageNumber.TabIndex = 4;
			this.NUDPageNumber.ValueChanged += new System.EventHandler(this.NUDPageNumber_ValueChanged);
			// 
			// TextPage
			// 
			this.TextPage.BackColor = System.Drawing.Color.White;
			this.TextPage.Font = new System.Drawing.Font("Consolas", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TextPage.Location = new System.Drawing.Point(16, 57);
			this.TextPage.Multiline = true;
			this.TextPage.Name = "TextPage";
			this.TextPage.ReadOnly = true;
			this.TextPage.Size = new System.Drawing.Size(596, 343);
			this.TextPage.TabIndex = 5;
			// 
			// ButtonSearch
			// 
			this.ButtonSearch.Location = new System.Drawing.Point(537, 406);
			this.ButtonSearch.Name = "ButtonSearch";
			this.ButtonSearch.Size = new System.Drawing.Size(75, 23);
			this.ButtonSearch.TabIndex = 6;
			this.ButtonSearch.Text = "&Search";
			this.ButtonSearch.UseVisualStyleBackColor = true;
			this.ButtonSearch.Click += new System.EventHandler(this.ButtonSearch_Click);
			// 
			// ButtonRandomPage
			// 
			this.ButtonRandomPage.Location = new System.Drawing.Point(524, 29);
			this.ButtonRandomPage.Name = "ButtonRandomPage";
			this.ButtonRandomPage.Size = new System.Drawing.Size(88, 23);
			this.ButtonRandomPage.TabIndex = 7;
			this.ButtonRandomPage.Text = "Random Page";
			this.ButtonRandomPage.UseVisualStyleBackColor = true;
			this.ButtonRandomPage.Click += new System.EventHandler(this.ButtonRandomPage_Click);
			// 
			// ButtonGo
			// 
			this.ButtonGo.Location = new System.Drawing.Point(409, 29);
			this.ButtonGo.Name = "ButtonGo";
			this.ButtonGo.Size = new System.Drawing.Size(34, 22);
			this.ButtonGo.TabIndex = 8;
			this.ButtonGo.Text = "Go";
			this.ButtonGo.UseVisualStyleBackColor = true;
			this.ButtonGo.Click += new System.EventHandler(this.ButtonGo_Click);
			// 
			// TextSearch
			// 
			this.TextSearch.Location = new System.Drawing.Point(407, 406);
			this.TextSearch.Name = "TextSearch";
			this.TextSearch.Size = new System.Drawing.Size(124, 22);
			this.TextSearch.TabIndex = 9;
			// 
			// StaticLabelSet
			// 
			this.StaticLabelSet.AutoSize = true;
			this.StaticLabelSet.Location = new System.Drawing.Point(15, 31);
			this.StaticLabelSet.Name = "StaticLabelSet";
			this.StaticLabelSet.Size = new System.Drawing.Size(26, 13);
			this.StaticLabelSet.TabIndex = 1;
			this.StaticLabelSet.Text = "Set:";
			this.StaticLabelSet.Click += new System.EventHandler(this.StaticLabelSet_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(624, 441);
			this.Controls.Add(this.TextSearch);
			this.Controls.Add(this.ButtonGo);
			this.Controls.Add(this.ButtonRandomPage);
			this.Controls.Add(this.ButtonSearch);
			this.Controls.Add(this.TextPage);
			this.Controls.Add(this.NUDPageNumber);
			this.Controls.Add(this.LabelPage);
			this.Controls.Add(this.NUDSetNumber);
			this.Controls.Add(this.StaticLabelSet);
			this.Controls.Add(this.LabelSeed);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "MainForm";
			this.Text = "Library of Noise";
			this.Load += new System.EventHandler(this.MainForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.NUDSetNumber)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NUDPageNumber)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label LabelSeed;
		private System.Windows.Forms.NumericUpDown NUDSetNumber;
		private System.Windows.Forms.Label LabelPage;
		private System.Windows.Forms.NumericUpDown NUDPageNumber;
		private System.Windows.Forms.TextBox TextPage;
		private System.Windows.Forms.Button ButtonSearch;
		private System.Windows.Forms.Button ButtonRandomPage;
		private System.Windows.Forms.Button ButtonGo;
		private System.Windows.Forms.TextBox TextSearch;
		private System.Windows.Forms.Label StaticLabelSet;
	}
}

