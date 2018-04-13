namespace BudgetAmountCalculator
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
			this.StaticLabelTotalAmount = new System.Windows.Forms.Label();
			this.TextTotalAmount = new System.Windows.Forms.TextBox();
			this.TextCurrentAmount = new System.Windows.Forms.TextBox();
			this.DTPByDate = new System.Windows.Forms.DateTimePicker();
			this.StaticLabelCurrentAmount = new System.Windows.Forms.Label();
			this.StaticLabelByDate = new System.Windows.Forms.Label();
			this.ButtonCalculate = new System.Windows.Forms.Button();
			this.LabelBudgetAmount = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// StaticLabelTotalAmount
			// 
			this.StaticLabelTotalAmount.AutoSize = true;
			this.StaticLabelTotalAmount.Location = new System.Drawing.Point(29, 16);
			this.StaticLabelTotalAmount.Name = "StaticLabelTotalAmount";
			this.StaticLabelTotalAmount.Size = new System.Drawing.Size(78, 13);
			this.StaticLabelTotalAmount.TabIndex = 0;
			this.StaticLabelTotalAmount.Text = "Total Amount:";
			// 
			// TextTotalAmount
			// 
			this.TextTotalAmount.Location = new System.Drawing.Point(113, 13);
			this.TextTotalAmount.Name = "TextTotalAmount";
			this.TextTotalAmount.Size = new System.Drawing.Size(207, 22);
			this.TextTotalAmount.TabIndex = 1;
			this.TextTotalAmount.TextChanged += new System.EventHandler(this.Amount_TextChanged);
			// 
			// TextCurrentAmount
			// 
			this.TextCurrentAmount.Location = new System.Drawing.Point(113, 42);
			this.TextCurrentAmount.Name = "TextCurrentAmount";
			this.TextCurrentAmount.Size = new System.Drawing.Size(207, 22);
			this.TextCurrentAmount.TabIndex = 2;
			this.TextCurrentAmount.TextChanged += new System.EventHandler(this.Amount_TextChanged);
			// 
			// DTPByDate
			// 
			this.DTPByDate.Location = new System.Drawing.Point(113, 71);
			this.DTPByDate.Name = "DTPByDate";
			this.DTPByDate.Size = new System.Drawing.Size(207, 22);
			this.DTPByDate.TabIndex = 3;
			// 
			// StaticLabelCurrentAmount
			// 
			this.StaticLabelCurrentAmount.AutoSize = true;
			this.StaticLabelCurrentAmount.Location = new System.Drawing.Point(14, 45);
			this.StaticLabelCurrentAmount.Name = "StaticLabelCurrentAmount";
			this.StaticLabelCurrentAmount.Size = new System.Drawing.Size(93, 13);
			this.StaticLabelCurrentAmount.TabIndex = 4;
			this.StaticLabelCurrentAmount.Text = "Current Amount:";
			// 
			// StaticLabelByDate
			// 
			this.StaticLabelByDate.AutoSize = true;
			this.StaticLabelByDate.Location = new System.Drawing.Point(58, 75);
			this.StaticLabelByDate.Name = "StaticLabelByDate";
			this.StaticLabelByDate.Size = new System.Drawing.Size(49, 13);
			this.StaticLabelByDate.TabIndex = 5;
			this.StaticLabelByDate.Text = "By Date:";
			// 
			// ButtonCalculate
			// 
			this.ButtonCalculate.Location = new System.Drawing.Point(245, 100);
			this.ButtonCalculate.Name = "ButtonCalculate";
			this.ButtonCalculate.Size = new System.Drawing.Size(75, 23);
			this.ButtonCalculate.TabIndex = 6;
			this.ButtonCalculate.Text = "&Calculate";
			this.ButtonCalculate.UseVisualStyleBackColor = true;
			this.ButtonCalculate.Click += new System.EventHandler(this.ButtonCalculate_Click);
			// 
			// LabelBudgetAmount
			// 
			this.LabelBudgetAmount.AutoSize = true;
			this.LabelBudgetAmount.Location = new System.Drawing.Point(12, 105);
			this.LabelBudgetAmount.Name = "LabelBudgetAmount";
			this.LabelBudgetAmount.Size = new System.Drawing.Size(75, 13);
			this.LabelBudgetAmount.TabIndex = 7;
			this.LabelBudgetAmount.Text = "Budget $0.00";
			// 
			// MainForm
			// 
			this.AcceptButton = this.ButtonCalculate;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(332, 132);
			this.Controls.Add(this.LabelBudgetAmount);
			this.Controls.Add(this.ButtonCalculate);
			this.Controls.Add(this.StaticLabelByDate);
			this.Controls.Add(this.StaticLabelCurrentAmount);
			this.Controls.Add(this.DTPByDate);
			this.Controls.Add(this.TextCurrentAmount);
			this.Controls.Add(this.TextTotalAmount);
			this.Controls.Add(this.StaticLabelTotalAmount);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.ShowIcon = false;
			this.Text = "Budget Amount Calculator";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label StaticLabelTotalAmount;
		private System.Windows.Forms.TextBox TextTotalAmount;
		private System.Windows.Forms.TextBox TextCurrentAmount;
		private System.Windows.Forms.DateTimePicker DTPByDate;
		private System.Windows.Forms.Label StaticLabelCurrentAmount;
		private System.Windows.Forms.Label StaticLabelByDate;
		private System.Windows.Forms.Button ButtonCalculate;
		private System.Windows.Forms.Label LabelBudgetAmount;
	}
}

