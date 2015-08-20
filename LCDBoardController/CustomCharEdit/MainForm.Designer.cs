namespace CustomCharEdit
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
			this.StaticLabelDescription = new System.Windows.Forms.Label();
			this.StaticLabelSeparator1 = new System.Windows.Forms.Label();
			this.StaticLabelBytes = new System.Windows.Forms.Label();
			this.TextboxPatternBytes = new System.Windows.Forms.TextBox();
			this.StaticLabelConstructor = new System.Windows.Forms.Label();
			this.TextConstructor = new System.Windows.Forms.TextBox();
			this.ButtonLoadPattern = new System.Windows.Forms.Button();
			this.StaticLabelSeparator2 = new System.Windows.Forms.Label();
			this.StaticLabelEditor = new System.Windows.Forms.Label();
			this.ButtonSave = new System.Windows.Forms.Button();
			this.TextCharacterName = new System.Windows.Forms.TextBox();
			this.StaticLabelCharacterName = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// StaticLabelDescription
			// 
			this.StaticLabelDescription.AutoSize = true;
			this.StaticLabelDescription.Location = new System.Drawing.Point(13, 13);
			this.StaticLabelDescription.Name = "StaticLabelDescription";
			this.StaticLabelDescription.Size = new System.Drawing.Size(305, 13);
			this.StaticLabelDescription.TabIndex = 0;
			this.StaticLabelDescription.Text = "Create new custom board characters or edit existing ones.";
			// 
			// StaticLabelSeparator1
			// 
			this.StaticLabelSeparator1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.StaticLabelSeparator1.Location = new System.Drawing.Point(16, 30);
			this.StaticLabelSeparator1.Name = "StaticLabelSeparator1";
			this.StaticLabelSeparator1.Size = new System.Drawing.Size(457, 1);
			this.StaticLabelSeparator1.TabIndex = 1;
			// 
			// StaticLabelBytes
			// 
			this.StaticLabelBytes.AutoSize = true;
			this.StaticLabelBytes.Location = new System.Drawing.Point(13, 36);
			this.StaticLabelBytes.Name = "StaticLabelBytes";
			this.StaticLabelBytes.Size = new System.Drawing.Size(130, 13);
			this.StaticLabelBytes.TabIndex = 2;
			this.StaticLabelBytes.Text = "Character pattern bytes:";
			// 
			// TextboxPatternBytes
			// 
			this.TextboxPatternBytes.Location = new System.Drawing.Point(16, 52);
			this.TextboxPatternBytes.Name = "TextboxPatternBytes";
			this.TextboxPatternBytes.Size = new System.Drawing.Size(408, 22);
			this.TextboxPatternBytes.TabIndex = 3;
			this.TextboxPatternBytes.Text = "0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00";
			// 
			// StaticLabelConstructor
			// 
			this.StaticLabelConstructor.AutoSize = true;
			this.StaticLabelConstructor.Location = new System.Drawing.Point(13, 81);
			this.StaticLabelConstructor.Name = "StaticLabelConstructor";
			this.StaticLabelConstructor.Size = new System.Drawing.Size(268, 13);
			this.StaticLabelConstructor.TabIndex = 4;
			this.StaticLabelConstructor.Text = "LCDBoardController CustomCharacter Constructor:";
			// 
			// TextConstructor
			// 
			this.TextConstructor.Location = new System.Drawing.Point(16, 97);
			this.TextConstructor.Name = "TextConstructor";
			this.TextConstructor.ReadOnly = true;
			this.TextConstructor.Size = new System.Drawing.Size(456, 22);
			this.TextConstructor.TabIndex = 5;
			this.TextConstructor.Text = "new CustomCharacter(0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00);";
			// 
			// ButtonLoadPattern
			// 
			this.ButtonLoadPattern.Location = new System.Drawing.Point(433, 50);
			this.ButtonLoadPattern.Name = "ButtonLoadPattern";
			this.ButtonLoadPattern.Size = new System.Drawing.Size(40, 23);
			this.ButtonLoadPattern.TabIndex = 6;
			this.ButtonLoadPattern.Text = "&Load";
			this.ButtonLoadPattern.UseVisualStyleBackColor = true;
			// 
			// StaticLabelSeparator2
			// 
			this.StaticLabelSeparator2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.StaticLabelSeparator2.Location = new System.Drawing.Point(16, 122);
			this.StaticLabelSeparator2.Name = "StaticLabelSeparator2";
			this.StaticLabelSeparator2.Size = new System.Drawing.Size(457, 1);
			this.StaticLabelSeparator2.TabIndex = 7;
			// 
			// StaticLabelEditor
			// 
			this.StaticLabelEditor.AutoSize = true;
			this.StaticLabelEditor.Location = new System.Drawing.Point(13, 123);
			this.StaticLabelEditor.Name = "StaticLabelEditor";
			this.StaticLabelEditor.Size = new System.Drawing.Size(245, 13);
			this.StaticLabelEditor.TabIndex = 8;
			this.StaticLabelEditor.Text = "Editor (left click to set pixel, right click to clear)";
			// 
			// ButtonSave
			// 
			this.ButtonSave.Location = new System.Drawing.Point(372, 153);
			this.ButtonSave.Name = "ButtonSave";
			this.ButtonSave.Size = new System.Drawing.Size(100, 23);
			this.ButtonSave.TabIndex = 9;
			this.ButtonSave.Text = "&Save";
			this.ButtonSave.UseVisualStyleBackColor = true;
			this.ButtonSave.Click += new System.EventHandler(this.ButtonSave_Click);
			// 
			// TextCharacterName
			// 
			this.TextCharacterName.Location = new System.Drawing.Point(372, 125);
			this.TextCharacterName.Name = "TextCharacterName";
			this.TextCharacterName.Size = new System.Drawing.Size(100, 22);
			this.TextCharacterName.TabIndex = 10;
			// 
			// StaticLabelCharacterName
			// 
			this.StaticLabelCharacterName.AutoSize = true;
			this.StaticLabelCharacterName.Location = new System.Drawing.Point(275, 128);
			this.StaticLabelCharacterName.Name = "StaticLabelCharacterName";
			this.StaticLabelCharacterName.Size = new System.Drawing.Size(91, 13);
			this.StaticLabelCharacterName.TabIndex = 11;
			this.StaticLabelCharacterName.Text = "Character Name:";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(484, 361);
			this.Controls.Add(this.StaticLabelCharacterName);
			this.Controls.Add(this.TextCharacterName);
			this.Controls.Add(this.ButtonSave);
			this.Controls.Add(this.StaticLabelEditor);
			this.Controls.Add(this.StaticLabelSeparator2);
			this.Controls.Add(this.ButtonLoadPattern);
			this.Controls.Add(this.TextConstructor);
			this.Controls.Add(this.StaticLabelConstructor);
			this.Controls.Add(this.TextboxPatternBytes);
			this.Controls.Add(this.StaticLabelBytes);
			this.Controls.Add(this.StaticLabelSeparator1);
			this.Controls.Add(this.StaticLabelDescription);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "MainForm";
			this.Text = "LCD Board Character Edit";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label StaticLabelDescription;
		private System.Windows.Forms.Label StaticLabelSeparator1;
		private System.Windows.Forms.Label StaticLabelBytes;
		private System.Windows.Forms.TextBox TextboxPatternBytes;
		private System.Windows.Forms.Label StaticLabelConstructor;
		private System.Windows.Forms.TextBox TextConstructor;
		private System.Windows.Forms.Button ButtonLoadPattern;
		private System.Windows.Forms.Label StaticLabelSeparator2;
		private System.Windows.Forms.Label StaticLabelEditor;
		private System.Windows.Forms.Button ButtonSave;
		private System.Windows.Forms.TextBox TextCharacterName;
		private System.Windows.Forms.Label StaticLabelCharacterName;
	}
}

