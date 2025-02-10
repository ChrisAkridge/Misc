namespace Celarix.JustForFun.GraphingPlayground
{
	partial class PlaygroundListForm
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
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
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			ListPlaygroundOptions = new ListBox();
			ButtonLaunchPlayground = new Button();
			SuspendLayout();
			// 
			// ListPlaygroundOptions
			// 
			ListPlaygroundOptions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			ListPlaygroundOptions.FormattingEnabled = true;
			ListPlaygroundOptions.ItemHeight = 15;
			ListPlaygroundOptions.Location = new Point(12, 12);
			ListPlaygroundOptions.Name = "ListPlaygroundOptions";
			ListPlaygroundOptions.Size = new Size(776, 394);
			ListPlaygroundOptions.TabIndex = 0;
			ListPlaygroundOptions.SelectedIndexChanged += ListPlaygroundOptions_SelectedIndexChanged;
			// 
			// ButtonLaunchPlayground
			// 
			ButtonLaunchPlayground.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			ButtonLaunchPlayground.Enabled = false;
			ButtonLaunchPlayground.Location = new Point(713, 415);
			ButtonLaunchPlayground.Name = "ButtonLaunchPlayground";
			ButtonLaunchPlayground.Size = new Size(75, 23);
			ButtonLaunchPlayground.TabIndex = 1;
			ButtonLaunchPlayground.Text = "Launch...";
			ButtonLaunchPlayground.UseVisualStyleBackColor = true;
			ButtonLaunchPlayground.Click += ButtonLaunchPlayground_Click;
			// 
			// PlaygroundListForm
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(800, 450);
			Controls.Add(ButtonLaunchPlayground);
			Controls.Add(ListPlaygroundOptions);
			Name = "PlaygroundListForm";
			Text = "Graphing Playground";
			Load += PlaygroundListForm_Load;
			ResumeLayout(false);
		}

		#endregion

		private ListBox ListPlaygroundOptions;
		private Button ButtonLaunchPlayground;
	}
}
