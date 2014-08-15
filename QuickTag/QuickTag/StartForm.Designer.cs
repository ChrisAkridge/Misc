namespace QuickTag
{
	partial class StartForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StartForm));
			this.StaticLabelWelcome = new System.Windows.Forms.Label();
			this.ButtonCreateDatabase = new System.Windows.Forms.Button();
			this.ButtonOpenExistingDatabase = new System.Windows.Forms.Button();
			this.OFDOpenDatabase = new System.Windows.Forms.OpenFileDialog();
			this.SFDCreateDatabase = new System.Windows.Forms.SaveFileDialog();
			this.SuspendLayout();
			// 
			// StaticLabelWelcome
			// 
			this.StaticLabelWelcome.AutoSize = true;
			this.StaticLabelWelcome.Location = new System.Drawing.Point(13, 13);
			this.StaticLabelWelcome.Name = "StaticLabelWelcome";
			this.StaticLabelWelcome.Size = new System.Drawing.Size(269, 78);
			this.StaticLabelWelcome.TabIndex = 0;
			this.StaticLabelWelcome.Text = resources.GetString("StaticLabelWelcome.Text");
			// 
			// ButtonCreateDatabase
			// 
			this.ButtonCreateDatabase.Location = new System.Drawing.Point(16, 95);
			this.ButtonCreateDatabase.Name = "ButtonCreateDatabase";
			this.ButtonCreateDatabase.Size = new System.Drawing.Size(256, 23);
			this.ButtonCreateDatabase.TabIndex = 1;
			this.ButtonCreateDatabase.Text = "&Create Database...";
			this.ButtonCreateDatabase.UseVisualStyleBackColor = true;
			this.ButtonCreateDatabase.Click += new System.EventHandler(this.ButtonCreateDatabase_Click);
			// 
			// ButtonOpenExistingDatabase
			// 
			this.ButtonOpenExistingDatabase.Location = new System.Drawing.Point(16, 125);
			this.ButtonOpenExistingDatabase.Name = "ButtonOpenExistingDatabase";
			this.ButtonOpenExistingDatabase.Size = new System.Drawing.Size(256, 23);
			this.ButtonOpenExistingDatabase.TabIndex = 2;
			this.ButtonOpenExistingDatabase.Text = "&Open Existing Database...";
			this.ButtonOpenExistingDatabase.UseVisualStyleBackColor = true;
			this.ButtonOpenExistingDatabase.Click += new System.EventHandler(this.ButtonOpenExistingDatabase_Click);
			// 
			// OFDOpenDatabase
			// 
			this.OFDOpenDatabase.DefaultExt = "json";
			this.OFDOpenDatabase.Filter = "JSON Files (*.json)|*.json|All files|*.*";
			this.OFDOpenDatabase.ShowHelp = true;
			this.OFDOpenDatabase.Title = "Open Existing Database...";
			// 
			// SFDCreateDatabase
			// 
			this.SFDCreateDatabase.DefaultExt = "json";
			this.SFDCreateDatabase.Filter = "JSON Files (*.json)|*.json|All files|*.*";
			this.SFDCreateDatabase.Title = "Create Database...";
			// 
			// StartForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 159);
			this.Controls.Add(this.ButtonOpenExistingDatabase);
			this.Controls.Add(this.ButtonCreateDatabase);
			this.Controls.Add(this.StaticLabelWelcome);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "StartForm";
			this.Text = "Quick Tag - Start";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label StaticLabelWelcome;
		private System.Windows.Forms.Button ButtonCreateDatabase;
		private System.Windows.Forms.Button ButtonOpenExistingDatabase;
		private System.Windows.Forms.OpenFileDialog OFDOpenDatabase;
		private System.Windows.Forms.SaveFileDialog SFDCreateDatabase;
	}
}

