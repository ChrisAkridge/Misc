namespace SupermarketSimulatorMain
{
	partial class GameForm
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
			this.GroupGameStatistics = new System.Windows.Forms.GroupBox();
			this.GroupBuyables = new System.Windows.Forms.GroupBox();
			this.GroupUpgrades = new System.Windows.Forms.GroupBox();
			this.LabelBank = new System.Windows.Forms.Label();
			this.LabelCustomerInflowPerSecond = new System.Windows.Forms.Label();
			this.LabelCustomersInStore = new System.Windows.Forms.Label();
			this.LabelCustomerOutflowPerSecond = new System.Windows.Forms.Label();
			this.GroupGameStatistics.SuspendLayout();
			this.SuspendLayout();
			// 
			// GroupGameStatistics
			// 
			this.GroupGameStatistics.Controls.Add(this.LabelCustomerOutflowPerSecond);
			this.GroupGameStatistics.Controls.Add(this.LabelCustomersInStore);
			this.GroupGameStatistics.Controls.Add(this.LabelCustomerInflowPerSecond);
			this.GroupGameStatistics.Controls.Add(this.LabelBank);
			this.GroupGameStatistics.Location = new System.Drawing.Point(12, 12);
			this.GroupGameStatistics.Name = "GroupGameStatistics";
			this.GroupGameStatistics.Size = new System.Drawing.Size(142, 286);
			this.GroupGameStatistics.TabIndex = 0;
			this.GroupGameStatistics.TabStop = false;
			this.GroupGameStatistics.Text = "Game Statistics";
			// 
			// GroupBuyables
			// 
			this.GroupBuyables.Location = new System.Drawing.Point(160, 12);
			this.GroupBuyables.Name = "GroupBuyables";
			this.GroupBuyables.Size = new System.Drawing.Size(142, 286);
			this.GroupBuyables.TabIndex = 1;
			this.GroupBuyables.TabStop = false;
			this.GroupBuyables.Text = "Buyables";
			// 
			// GroupUpgrades
			// 
			this.GroupUpgrades.Location = new System.Drawing.Point(308, 12);
			this.GroupUpgrades.Name = "GroupUpgrades";
			this.GroupUpgrades.Size = new System.Drawing.Size(142, 286);
			this.GroupUpgrades.TabIndex = 1;
			this.GroupUpgrades.TabStop = false;
			this.GroupUpgrades.Text = "Upgrades";
			// 
			// LabelBank
			// 
			this.LabelBank.AutoSize = true;
			this.LabelBank.Location = new System.Drawing.Point(7, 22);
			this.LabelBank.Name = "LabelBank";
			this.LabelBank.Size = new System.Drawing.Size(94, 13);
			this.LabelBank.TabIndex = 0;
			this.LabelBank.Text = "Bank: ${currency}";
			// 
			// LabelCustomerInflowPerSecond
			// 
			this.LabelCustomerInflowPerSecond.AutoSize = true;
			this.LabelCustomerInflowPerSecond.Location = new System.Drawing.Point(7, 35);
			this.LabelCustomerInflowPerSecond.Name = "LabelCustomerInflowPerSecond";
			this.LabelCustomerInflowPerSecond.Size = new System.Drawing.Size(116, 13);
			this.LabelCustomerInflowPerSecond.TabIndex = 1;
			this.LabelCustomerInflowPerSecond.Text = "Customers/sec: {num}";
			// 
			// LabelCustomersInStore
			// 
			this.LabelCustomersInStore.AutoSize = true;
			this.LabelCustomersInStore.Location = new System.Drawing.Point(7, 48);
			this.LabelCustomersInStore.Name = "LabelCustomersInStore";
			this.LabelCustomersInStore.Size = new System.Drawing.Size(109, 13);
			this.LabelCustomersInStore.TabIndex = 2;
			this.LabelCustomersInStore.Text = "Customers In: {num}";
			// 
			// LabelCustomerOutflowPerSecond
			// 
			this.LabelCustomerOutflowPerSecond.AutoSize = true;
			this.LabelCustomerOutflowPerSecond.Location = new System.Drawing.Point(7, 61);
			this.LabelCustomerOutflowPerSecond.Name = "LabelCustomerOutflowPerSecond";
			this.LabelCustomerOutflowPerSecond.Size = new System.Drawing.Size(111, 13);
			this.LabelCustomerOutflowPerSecond.TabIndex = 3;
			this.LabelCustomerOutflowPerSecond.Text = "Cust. Out/sec: {num}";
			// 
			// GameForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(458, 311);
			this.Controls.Add(this.GroupUpgrades);
			this.Controls.Add(this.GroupBuyables);
			this.Controls.Add(this.GroupGameStatistics);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "GameForm";
			this.ShowIcon = false;
			this.Text = "Supermarket Simulator";
			this.GroupGameStatistics.ResumeLayout(false);
			this.GroupGameStatistics.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox GroupGameStatistics;
		private System.Windows.Forms.Label LabelCustomerOutflowPerSecond;
		private System.Windows.Forms.Label LabelCustomersInStore;
		private System.Windows.Forms.Label LabelCustomerInflowPerSecond;
		private System.Windows.Forms.Label LabelBank;
		private System.Windows.Forms.GroupBox GroupBuyables;
		private System.Windows.Forms.GroupBox GroupUpgrades;
	}
}