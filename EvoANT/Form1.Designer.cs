using System.Windows.Forms;
using EvoANTFrontend;

namespace EvoANTCore
{
	partial class Form1
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
			this.displayGrid1 = new DisplayGrid();
			this.ButtonUpdate = new Button();
			this.SuspendLayout();
			// 
			// displayGrid1
			// 
			this.displayGrid1.AutoScroll = true;
			this.displayGrid1.GridLineColor = System.Drawing.Color.Black;
			this.displayGrid1.Location = new System.Drawing.Point(12, 12);
			this.displayGrid1.Name = "displayGrid1";
			this.displayGrid1.Size = new System.Drawing.Size(620, 597);
			this.displayGrid1.TabIndex = 0;
			this.displayGrid1.World = null;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(644, 680);
			this.Controls.Add(this.displayGrid1);
			
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			//
			// ButtonUpdate
			//
			ButtonUpdate.Size = new System.Drawing.Size(60, 20);
			ButtonUpdate.Text = "Update";
			ButtonUpdate.Location = new System.Drawing.Point(12, 614);
			ButtonUpdate.Click += ButtonUpdate_Click;
			this.Controls.Add(ButtonUpdate);
		}

		#endregion

		private DisplayGrid displayGrid1;
		private Button ButtonUpdate;
	}
}

