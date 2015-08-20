namespace SupermarketSimulator
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
			this.components = new System.ComponentModel.Container();
			this.StaticLabelGameInformation = new System.Windows.Forms.Label();
			this.TextBoxGameInformation = new System.Windows.Forms.TextBox();
			this.StaticLabelBuyingOptions = new System.Windows.Forms.Label();
			this.ListBoxBuyingOptions = new System.Windows.Forms.ListBox();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.label1 = new System.Windows.Forms.Label();
			this.ListBoxUpgrades = new System.Windows.Forms.ListBox();
			this.SuspendLayout();
			// 
			// StaticLabelGameInformation
			// 
			this.StaticLabelGameInformation.AutoSize = true;
			this.StaticLabelGameInformation.Location = new System.Drawing.Point(13, 13);
			this.StaticLabelGameInformation.Name = "StaticLabelGameInformation";
			this.StaticLabelGameInformation.Size = new System.Drawing.Size(103, 13);
			this.StaticLabelGameInformation.TabIndex = 0;
			this.StaticLabelGameInformation.Text = "Game Information:";
			// 
			// TextBoxGameInformation
			// 
			this.TextBoxGameInformation.Location = new System.Drawing.Point(13, 30);
			this.TextBoxGameInformation.Multiline = true;
			this.TextBoxGameInformation.Name = "TextBoxGameInformation";
			this.TextBoxGameInformation.ReadOnly = true;
			this.TextBoxGameInformation.Size = new System.Drawing.Size(359, 306);
			this.TextBoxGameInformation.TabIndex = 1;
			// 
			// StaticLabelBuyingOptions
			// 
			this.StaticLabelBuyingOptions.AutoSize = true;
			this.StaticLabelBuyingOptions.Location = new System.Drawing.Point(13, 339);
			this.StaticLabelBuyingOptions.Name = "StaticLabelBuyingOptions";
			this.StaticLabelBuyingOptions.Size = new System.Drawing.Size(91, 13);
			this.StaticLabelBuyingOptions.TabIndex = 2;
			this.StaticLabelBuyingOptions.Text = "Buying Options:";
			// 
			// ListBoxBuyingOptions
			// 
			this.ListBoxBuyingOptions.FormattingEnabled = true;
			this.ListBoxBuyingOptions.Location = new System.Drawing.Point(10, 356);
			this.ListBoxBuyingOptions.Name = "ListBoxBuyingOptions";
			this.ListBoxBuyingOptions.Size = new System.Drawing.Size(196, 95);
			this.ListBoxBuyingOptions.TabIndex = 3;
			this.ListBoxBuyingOptions.SelectedIndexChanged += new System.EventHandler(this.ListBoxBuyingOptions_SelectedIndexChanged);
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 33;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(211, 339);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(60, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Upgrades:";
			// 
			// ListBoxUpgrades
			// 
			this.ListBoxUpgrades.FormattingEnabled = true;
			this.ListBoxUpgrades.Location = new System.Drawing.Point(214, 356);
			this.ListBoxUpgrades.Name = "ListBoxUpgrades";
			this.ListBoxUpgrades.Size = new System.Drawing.Size(158, 95);
			this.ListBoxUpgrades.TabIndex = 5;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(384, 461);
			this.Controls.Add(this.ListBoxUpgrades);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.ListBoxBuyingOptions);
			this.Controls.Add(this.StaticLabelBuyingOptions);
			this.Controls.Add(this.TextBoxGameInformation);
			this.Controls.Add(this.StaticLabelGameInformation);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.Text = "Supermarket Simulator";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label StaticLabelGameInformation;
		private System.Windows.Forms.TextBox TextBoxGameInformation;
		private System.Windows.Forms.Label StaticLabelBuyingOptions;
		private System.Windows.Forms.ListBox ListBoxBuyingOptions;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListBox ListBoxUpgrades;
	}
}

