namespace SupermarketSimulatorMain
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
			this.ButtonLaunchInstance = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// ButtonLaunchInstance
			// 
			this.ButtonLaunchInstance.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ButtonLaunchInstance.Location = new System.Drawing.Point(13, 6);
			this.ButtonLaunchInstance.Name = "ButtonLaunchInstance";
			this.ButtonLaunchInstance.Size = new System.Drawing.Size(259, 23);
			this.ButtonLaunchInstance.TabIndex = 0;
			this.ButtonLaunchInstance.Text = "&Launch New Game Instance...";
			this.ButtonLaunchInstance.UseVisualStyleBackColor = true;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 36);
			this.Controls.Add(this.ButtonLaunchInstance);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.ShowIcon = false;
			this.Text = "Supermarket Simulator";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button ButtonLaunchInstance;
	}
}

