namespace SupermarketSimulatorTest
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
			this.components = new System.ComponentModel.Container();
			this.LabelBank = new System.Windows.Forms.Label();
			this.LabelStockedProducts = new System.Windows.Forms.Label();
			this.LabelCustomersPerSecond = new System.Windows.Forms.Label();
			this.LabelIncomePerSecond = new System.Windows.Forms.Label();
			this.ButtonBuyStocker = new System.Windows.Forms.Button();
			this.Timer = new System.Windows.Forms.Timer(this.components);
			this.ButtonHireCashier = new System.Windows.Forms.Button();
			this.LabelCustomersInStore = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// LabelBank
			// 
			this.LabelBank.AutoSize = true;
			this.LabelBank.Location = new System.Drawing.Point(12, 13);
			this.LabelBank.Name = "LabelBank";
			this.LabelBank.Size = new System.Drawing.Size(85, 13);
			this.LabelBank.TabIndex = 0;
			this.LabelBank.Text = "Bank: ${money}";
			// 
			// LabelStockedProducts
			// 
			this.LabelStockedProducts.AutoSize = true;
			this.LabelStockedProducts.Location = new System.Drawing.Point(12, 26);
			this.LabelStockedProducts.Name = "LabelStockedProducts";
			this.LabelStockedProducts.Size = new System.Drawing.Size(197, 13);
			this.LabelStockedProducts.TabIndex = 1;
			this.LabelStockedProducts.Text = "Stocked Products: {stocked products}";
			// 
			// LabelCustomersPerSecond
			// 
			this.LabelCustomersPerSecond.AutoSize = true;
			this.LabelCustomersPerSecond.Location = new System.Drawing.Point(12, 39);
			this.LabelCustomersPerSecond.Name = "LabelCustomersPerSecond";
			this.LabelCustomersPerSecond.Size = new System.Drawing.Size(160, 13);
			this.LabelCustomersPerSecond.TabIndex = 2;
			this.LabelCustomersPerSecond.Text = "Customers Per Second: {value}";
			// 
			// LabelIncomePerSecond
			// 
			this.LabelIncomePerSecond.AutoSize = true;
			this.LabelIncomePerSecond.Location = new System.Drawing.Point(12, 65);
			this.LabelIncomePerSecond.Name = "LabelIncomePerSecond";
			this.LabelIncomePerSecond.Size = new System.Drawing.Size(143, 13);
			this.LabelIncomePerSecond.TabIndex = 3;
			this.LabelIncomePerSecond.Text = "Income Per Second: {value}";
			// 
			// ButtonBuyStocker
			// 
			this.ButtonBuyStocker.Location = new System.Drawing.Point(13, 87);
			this.ButtonBuyStocker.Name = "ButtonBuyStocker";
			this.ButtonBuyStocker.Size = new System.Drawing.Size(318, 23);
			this.ButtonBuyStocker.TabIndex = 4;
			this.ButtonBuyStocker.Text = "Stockers: 0";
			this.ButtonBuyStocker.UseVisualStyleBackColor = true;
			this.ButtonBuyStocker.Click += new System.EventHandler(this.ButtonBuyStocker_Click);
			// 
			// Timer
			// 
			this.Timer.Enabled = true;
			this.Timer.Interval = 33;
			this.Timer.Tick += new System.EventHandler(this.Timer_Tick);
			// 
			// ButtonHireCashier
			// 
			this.ButtonHireCashier.Location = new System.Drawing.Point(12, 116);
			this.ButtonHireCashier.Name = "ButtonHireCashier";
			this.ButtonHireCashier.Size = new System.Drawing.Size(318, 23);
			this.ButtonHireCashier.TabIndex = 5;
			this.ButtonHireCashier.Text = "Cashiers: 0";
			this.ButtonHireCashier.UseVisualStyleBackColor = true;
			// 
			// LabelCustomersInStore
			// 
			this.LabelCustomersInStore.AutoSize = true;
			this.LabelCustomersInStore.Location = new System.Drawing.Point(12, 52);
			this.LabelCustomersInStore.Name = "LabelCustomersInStore";
			this.LabelCustomersInStore.Size = new System.Drawing.Size(143, 13);
			this.LabelCustomersInStore.TabIndex = 6;
			this.LabelCustomersInStore.Text = "Customers In Store: {value}";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(343, 151);
			this.Controls.Add(this.LabelCustomersInStore);
			this.Controls.Add(this.ButtonHireCashier);
			this.Controls.Add(this.ButtonBuyStocker);
			this.Controls.Add(this.LabelIncomePerSecond);
			this.Controls.Add(this.LabelCustomersPerSecond);
			this.Controls.Add(this.LabelStockedProducts);
			this.Controls.Add(this.LabelBank);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "Form1";
			this.Text = "Supermarket Simulator TEST";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label LabelBank;
		private System.Windows.Forms.Label LabelStockedProducts;
		private System.Windows.Forms.Label LabelCustomersPerSecond;
		private System.Windows.Forms.Label LabelIncomePerSecond;
		private System.Windows.Forms.Button ButtonBuyStocker;
		private System.Windows.Forms.Timer Timer;
		private System.Windows.Forms.Button ButtonHireCashier;
		private System.Windows.Forms.Label LabelCustomersInStore;
	}
}

