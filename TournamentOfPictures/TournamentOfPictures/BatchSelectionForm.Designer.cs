namespace TournamentOfPictures
{
	partial class BatchSelectionForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BatchSelectionForm));
            this.StaticLabelInstructions = new System.Windows.Forms.Label();
            this.ButtonSubmit = new System.Windows.Forms.Button();
            this.ButtonStartOver = new System.Windows.Forms.Button();
            this.ProgressRound = new System.Windows.Forms.ProgressBar();
            this.LabelRoundInfo = new System.Windows.Forms.Label();
            this.TLPBatches = new System.Windows.Forms.TableLayoutPanel();
            this.PanelTopLeft = new System.Windows.Forms.Panel();
            this.PanelTopRight = new System.Windows.Forms.Panel();
            this.PanelBottomLeft = new System.Windows.Forms.Panel();
            this.PanelBottomRight = new System.Windows.Forms.Panel();
            this.TLPBatches.SuspendLayout();
            this.SuspendLayout();
            // 
            // StaticLabelInstructions
            // 
            this.StaticLabelInstructions.AutoSize = true;
            this.StaticLabelInstructions.Location = new System.Drawing.Point(13, 13);
            this.StaticLabelInstructions.Name = "StaticLabelInstructions";
            this.StaticLabelInstructions.Size = new System.Drawing.Size(283, 13);
            this.StaticLabelInstructions.TabIndex = 0;
            this.StaticLabelInstructions.Text = "Click each image or batch in order from best to worst.";
            // 
            // ButtonSubmit
            // 
            this.ButtonSubmit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonSubmit.Location = new System.Drawing.Point(429, 387);
            this.ButtonSubmit.Name = "ButtonSubmit";
            this.ButtonSubmit.Size = new System.Drawing.Size(75, 23);
            this.ButtonSubmit.TabIndex = 1;
            this.ButtonSubmit.Text = "Submit";
            this.ButtonSubmit.UseVisualStyleBackColor = true;
            // 
            // ButtonStartOver
            // 
            this.ButtonStartOver.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonStartOver.Location = new System.Drawing.Point(348, 387);
            this.ButtonStartOver.Name = "ButtonStartOver";
            this.ButtonStartOver.Size = new System.Drawing.Size(75, 23);
            this.ButtonStartOver.TabIndex = 2;
            this.ButtonStartOver.Text = "Start Over";
            this.ButtonStartOver.UseVisualStyleBackColor = true;
            this.ButtonStartOver.Click += new System.EventHandler(this.ButtonStartOver_Click);
            // 
            // ProgressRound
            // 
            this.ProgressRound.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ProgressRound.Location = new System.Drawing.Point(12, 357);
            this.ProgressRound.Name = "ProgressRound";
            this.ProgressRound.Size = new System.Drawing.Size(492, 23);
            this.ProgressRound.TabIndex = 3;
            // 
            // LabelRoundInfo
            // 
            this.LabelRoundInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LabelRoundInfo.AutoSize = true;
            this.LabelRoundInfo.Location = new System.Drawing.Point(9, 392);
            this.LabelRoundInfo.Name = "LabelRoundInfo";
            this.LabelRoundInfo.Size = new System.Drawing.Size(135, 13);
            this.LabelRoundInfo.TabIndex = 4;
            this.LabelRoundInfo.Text = "Round {0}: Batch {1} of {2}";
            // 
            // TLPBatches
            // 
            this.TLPBatches.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TLPBatches.ColumnCount = 2;
            this.TLPBatches.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TLPBatches.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TLPBatches.Controls.Add(this.PanelTopLeft, 0, 0);
            this.TLPBatches.Controls.Add(this.PanelTopRight, 1, 0);
            this.TLPBatches.Controls.Add(this.PanelBottomLeft, 0, 1);
            this.TLPBatches.Controls.Add(this.PanelBottomRight, 1, 1);
            this.TLPBatches.Location = new System.Drawing.Point(16, 30);
            this.TLPBatches.Name = "TLPBatches";
            this.TLPBatches.RowCount = 2;
            this.TLPBatches.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TLPBatches.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TLPBatches.Size = new System.Drawing.Size(488, 321);
            this.TLPBatches.TabIndex = 5;
            // 
            // PanelTopLeft
            // 
            this.PanelTopLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.PanelTopLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelTopLeft.Location = new System.Drawing.Point(3, 3);
            this.PanelTopLeft.Name = "PanelTopLeft";
            this.PanelTopLeft.Size = new System.Drawing.Size(238, 154);
            this.PanelTopLeft.TabIndex = 0;
            this.PanelTopLeft.Click += new System.EventHandler(this.PanelTopLeft_Click);
            // 
            // PanelTopRight
            // 
            this.PanelTopRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.PanelTopRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelTopRight.Location = new System.Drawing.Point(247, 3);
            this.PanelTopRight.Name = "PanelTopRight";
            this.PanelTopRight.Size = new System.Drawing.Size(238, 154);
            this.PanelTopRight.TabIndex = 1;
            this.PanelTopRight.Click += new System.EventHandler(this.PanelTopLeft_Click);
            // 
            // PanelBottomLeft
            // 
            this.PanelBottomLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.PanelBottomLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelBottomLeft.Location = new System.Drawing.Point(3, 163);
            this.PanelBottomLeft.Name = "PanelBottomLeft";
            this.PanelBottomLeft.Size = new System.Drawing.Size(238, 155);
            this.PanelBottomLeft.TabIndex = 2;
            this.PanelBottomLeft.Click += new System.EventHandler(this.PanelTopLeft_Click);
            // 
            // PanelBottomRight
            // 
            this.PanelBottomRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.PanelBottomRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PanelBottomRight.Location = new System.Drawing.Point(247, 163);
            this.PanelBottomRight.Name = "PanelBottomRight";
            this.PanelBottomRight.Size = new System.Drawing.Size(238, 155);
            this.PanelBottomRight.TabIndex = 3;
            this.PanelBottomRight.Click += new System.EventHandler(this.PanelTopLeft_Click);
            // 
            // BatchSelectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(516, 422);
            this.Controls.Add(this.TLPBatches);
            this.Controls.Add(this.LabelRoundInfo);
            this.Controls.Add(this.ProgressRound);
            this.Controls.Add(this.ButtonStartOver);
            this.Controls.Add(this.ButtonSubmit);
            this.Controls.Add(this.StaticLabelInstructions);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BatchSelectionForm";
            this.Text = "Batch Selection Tournament";
            this.TLPBatches.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label StaticLabelInstructions;
		private System.Windows.Forms.Button ButtonSubmit;
		private System.Windows.Forms.Button ButtonStartOver;
		private System.Windows.Forms.ProgressBar ProgressRound;
		private System.Windows.Forms.Label LabelRoundInfo;
		private System.Windows.Forms.TableLayoutPanel TLPBatches;
		private System.Windows.Forms.Panel PanelTopLeft;
		private System.Windows.Forms.Panel PanelTopRight;
		private System.Windows.Forms.Panel PanelBottomLeft;
		private System.Windows.Forms.Panel PanelBottomRight;
	}
}