namespace JarOfGoodThings
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
			this.StaticLabelGoodThing = new System.Windows.Forms.Label();
			this.TextGoodThing = new System.Windows.Forms.TextBox();
			this.ButtonSubmit = new System.Windows.Forms.Button();
			this.LabelGoodThingCount = new System.Windows.Forms.Label();
			this.LabelDaysUntilJarUnlock = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// StaticLabelGoodThing
			// 
			this.StaticLabelGoodThing.AutoSize = true;
			this.StaticLabelGoodThing.Location = new System.Drawing.Point(13, 13);
			this.StaticLabelGoodThing.Name = "StaticLabelGoodThing";
			this.StaticLabelGoodThing.Size = new System.Drawing.Size(79, 13);
			this.StaticLabelGoodThing.TabIndex = 0;
			this.StaticLabelGoodThing.Text = "A good thing:";
			// 
			// TextGoodThing
			// 
			this.TextGoodThing.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TextGoodThing.Location = new System.Drawing.Point(13, 30);
			this.TextGoodThing.Name = "TextGoodThing";
			this.TextGoodThing.Size = new System.Drawing.Size(408, 22);
			this.TextGoodThing.TabIndex = 1;
			this.TextGoodThing.TextChanged += new System.EventHandler(this.TextGoodThing_TextChanged);
			// 
			// ButtonSubmit
			// 
			this.ButtonSubmit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonSubmit.Enabled = false;
			this.ButtonSubmit.Location = new System.Drawing.Point(345, 59);
			this.ButtonSubmit.Name = "ButtonSubmit";
			this.ButtonSubmit.Size = new System.Drawing.Size(75, 23);
			this.ButtonSubmit.TabIndex = 2;
			this.ButtonSubmit.Text = "&Submit";
			this.ButtonSubmit.UseVisualStyleBackColor = true;
			this.ButtonSubmit.Click += new System.EventHandler(this.ButtonSubmit_Click);
			// 
			// LabelGoodThingCount
			// 
			this.LabelGoodThingCount.AutoSize = true;
			this.LabelGoodThingCount.Location = new System.Drawing.Point(13, 59);
			this.LabelGoodThingCount.Name = "LabelGoodThingCount";
			this.LabelGoodThingCount.Size = new System.Drawing.Size(194, 13);
			this.LabelGoodThingCount.TabIndex = 3;
			this.LabelGoodThingCount.Text = "You\'ve put {0} good things in the jar!";
			// 
			// LabelDaysUntilJarUnlock
			// 
			this.LabelDaysUntilJarUnlock.AutoSize = true;
			this.LabelDaysUntilJarUnlock.Location = new System.Drawing.Point(13, 72);
			this.LabelDaysUntilJarUnlock.Name = "LabelDaysUntilJarUnlock";
			this.LabelDaysUntilJarUnlock.Size = new System.Drawing.Size(146, 13);
			this.LabelDaysUntilJarUnlock.TabIndex = 4;
			this.LabelDaysUntilJarUnlock.Text = "There are {0} days left in {1}.";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(433, 97);
			this.Controls.Add(this.LabelDaysUntilJarUnlock);
			this.Controls.Add(this.LabelGoodThingCount);
			this.Controls.Add(this.ButtonSubmit);
			this.Controls.Add(this.TextGoodThing);
			this.Controls.Add(this.StaticLabelGoodThing);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.ShowIcon = false;
			this.Text = "Jar of Good Things";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.DoubleClick += new System.EventHandler(this.MainForm_DoubleClick);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label StaticLabelGoodThing;
		private System.Windows.Forms.TextBox TextGoodThing;
		private System.Windows.Forms.Button ButtonSubmit;
		private System.Windows.Forms.Label LabelGoodThingCount;
		private System.Windows.Forms.Label LabelDaysUntilJarUnlock;
	}
}

