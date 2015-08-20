namespace QuickLog
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
			this.StaticLabelSteps = new System.Windows.Forms.Label();
			this.NUDSteps = new System.Windows.Forms.NumericUpDown();
			this.NUDSmiles = new System.Windows.Forms.NumericUpDown();
			this.StaticLabelSmiles = new System.Windows.Forms.Label();
			this.ButtonAccept = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.NUDSteps)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.NUDSmiles)).BeginInit();
			this.SuspendLayout();
			// 
			// StaticLabelSteps
			// 
			this.StaticLabelSteps.AutoSize = true;
			this.StaticLabelSteps.Location = new System.Drawing.Point(13, 13);
			this.StaticLabelSteps.Name = "StaticLabelSteps";
			this.StaticLabelSteps.Size = new System.Drawing.Size(38, 13);
			this.StaticLabelSteps.TabIndex = 0;
			this.StaticLabelSteps.Text = "Steps:";
			// 
			// NUDSteps
			// 
			this.NUDSteps.Location = new System.Drawing.Point(57, 11);
			this.NUDSteps.Maximum = new decimal(new int[] {
            10000000,
            0,
            0,
            0});
			this.NUDSteps.Name = "NUDSteps";
			this.NUDSteps.Size = new System.Drawing.Size(69, 22);
			this.NUDSteps.TabIndex = 1;
			// 
			// NUDSmiles
			// 
			this.NUDSmiles.Location = new System.Drawing.Point(57, 39);
			this.NUDSmiles.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.NUDSmiles.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
			this.NUDSmiles.Name = "NUDSmiles";
			this.NUDSmiles.Size = new System.Drawing.Size(69, 22);
			this.NUDSmiles.TabIndex = 3;
			// 
			// StaticLabelSmiles
			// 
			this.StaticLabelSmiles.AutoSize = true;
			this.StaticLabelSmiles.Location = new System.Drawing.Point(13, 41);
			this.StaticLabelSmiles.Name = "StaticLabelSmiles";
			this.StaticLabelSmiles.Size = new System.Drawing.Size(42, 13);
			this.StaticLabelSmiles.TabIndex = 2;
			this.StaticLabelSmiles.Text = "Smiles:";
			// 
			// ButtonAccept
			// 
			this.ButtonAccept.Location = new System.Drawing.Point(16, 71);
			this.ButtonAccept.Name = "ButtonAccept";
			this.ButtonAccept.Size = new System.Drawing.Size(110, 23);
			this.ButtonAccept.TabIndex = 4;
			this.ButtonAccept.Text = "&Accept";
			this.ButtonAccept.UseVisualStyleBackColor = true;
			this.ButtonAccept.Click += new System.EventHandler(this.ButtonAccept_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(144, 99);
			this.Controls.Add(this.ButtonAccept);
			this.Controls.Add(this.NUDSmiles);
			this.Controls.Add(this.StaticLabelSmiles);
			this.Controls.Add(this.NUDSteps);
			this.Controls.Add(this.StaticLabelSteps);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MainForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Quick Log";
			this.TopMost = true;
			((System.ComponentModel.ISupportInitialize)(this.NUDSteps)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.NUDSmiles)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label StaticLabelSteps;
		private System.Windows.Forms.NumericUpDown NUDSteps;
		private System.Windows.Forms.NumericUpDown NUDSmiles;
		private System.Windows.Forms.Label StaticLabelSmiles;
		private System.Windows.Forms.Button ButtonAccept;
	}
}

