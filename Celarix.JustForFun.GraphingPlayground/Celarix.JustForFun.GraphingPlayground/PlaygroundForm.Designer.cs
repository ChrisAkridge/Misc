namespace Celarix.JustForFun.GraphingPlayground
{
	partial class PlaygroundForm
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
			var resources = new System.ComponentModel.ComponentResourceManager(typeof(PlaygroundForm));
			OFDMain = new OpenFileDialog();
			SFDMain = new SaveFileDialog();
			PlotMain = new ScottPlot.WinForms.FormsPlot();
			TSMain = new ToolStrip();
			TSBOpenFile = new ToolStripButton();
			TSBSaveAs = new ToolStripButton();
			TSSeparator1 = new ToolStripSeparator();
			TSDDBViews = new ToolStripDropDownButton();
			GroupChartControls = new GroupBox();
			CheckLockYAxis = new CheckBox();
			CheckLockXAxis = new CheckBox();
			NUDRollingAveragePeriod = new NumericUpDown();
			StaticLabelRollingAveragePeriod = new Label();
			CheckRollingAverage = new CheckBox();
			CheckLinearRegression = new CheckBox();
			TSMain.SuspendLayout();
			GroupChartControls.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)NUDRollingAveragePeriod).BeginInit();
			SuspendLayout();
			// 
			// OFDMain
			// 
			OFDMain.Filter = "All files|*.*";
			OFDMain.Title = "Open File...";
			// 
			// SFDMain
			// 
			SFDMain.Filter = "PNG Image|*.png|All files|*.*";
			SFDMain.Title = "Save Graph As...";
			// 
			// PlotMain
			// 
			PlotMain.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			PlotMain.DisplayScale = 1F;
			PlotMain.Location = new Point(12, 28);
			PlotMain.Name = "PlotMain";
			PlotMain.Size = new Size(731, 469);
			PlotMain.TabIndex = 2;
			// 
			// TSMain
			// 
			TSMain.Items.AddRange(new ToolStripItem[] { TSBOpenFile, TSBSaveAs, TSSeparator1, TSDDBViews });
			TSMain.Location = new Point(0, 0);
			TSMain.Name = "TSMain";
			TSMain.Size = new Size(1033, 25);
			TSMain.TabIndex = 3;
			TSMain.Text = "toolStrip1";
			// 
			// TSBOpenFile
			// 
			TSBOpenFile.DisplayStyle = ToolStripItemDisplayStyle.Text;
			TSBOpenFile.Image = (Image)resources.GetObject("TSBOpenFile.Image");
			TSBOpenFile.ImageTransparentColor = Color.Magenta;
			TSBOpenFile.Name = "TSBOpenFile";
			TSBOpenFile.Size = new Size(70, 22);
			TSBOpenFile.Text = "Open File...";
			TSBOpenFile.Click += TSBOpenFile_Click;
			// 
			// TSBSaveAs
			// 
			TSBSaveAs.DisplayStyle = ToolStripItemDisplayStyle.Text;
			TSBSaveAs.Image = (Image)resources.GetObject("TSBSaveAs.Image");
			TSBSaveAs.ImageTransparentColor = Color.Magenta;
			TSBSaveAs.Name = "TSBSaveAs";
			TSBSaveAs.Size = new Size(60, 22);
			TSBSaveAs.Text = "Save As...";
			// 
			// TSSeparator1
			// 
			TSSeparator1.Name = "TSSeparator1";
			TSSeparator1.Size = new Size(6, 25);
			// 
			// TSDDBViews
			// 
			TSDDBViews.DisplayStyle = ToolStripItemDisplayStyle.Text;
			TSDDBViews.Image = (Image)resources.GetObject("TSDDBViews.Image");
			TSDDBViews.ImageTransparentColor = Color.Magenta;
			TSDDBViews.Name = "TSDDBViews";
			TSDDBViews.Size = new Size(85, 22);
			TSDDBViews.Text = "Graph Views";
			// 
			// GroupChartControls
			// 
			GroupChartControls.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
			GroupChartControls.Controls.Add(CheckLockYAxis);
			GroupChartControls.Controls.Add(CheckLockXAxis);
			GroupChartControls.Controls.Add(NUDRollingAveragePeriod);
			GroupChartControls.Controls.Add(StaticLabelRollingAveragePeriod);
			GroupChartControls.Controls.Add(CheckRollingAverage);
			GroupChartControls.Controls.Add(CheckLinearRegression);
			GroupChartControls.Location = new Point(749, 28);
			GroupChartControls.Name = "GroupChartControls";
			GroupChartControls.Size = new Size(272, 469);
			GroupChartControls.TabIndex = 5;
			GroupChartControls.TabStop = false;
			GroupChartControls.Text = "Controls";
			// 
			// CheckLockYAxis
			// 
			CheckLockYAxis.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			CheckLockYAxis.AutoSize = true;
			CheckLockYAxis.Location = new Point(98, 444);
			CheckLockYAxis.Name = "CheckLockYAxis";
			CheckLockYAxis.Size = new Size(86, 19);
			CheckLockYAxis.TabIndex = 5;
			CheckLockYAxis.Text = "Lock Y Axis";
			CheckLockYAxis.UseVisualStyleBackColor = true;
			CheckLockYAxis.CheckedChanged += CheckLockYAxis_CheckedChanged;
			// 
			// CheckLockXAxis
			// 
			CheckLockXAxis.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			CheckLockXAxis.AutoSize = true;
			CheckLockXAxis.Location = new Point(6, 444);
			CheckLockXAxis.Name = "CheckLockXAxis";
			CheckLockXAxis.Size = new Size(86, 19);
			CheckLockXAxis.TabIndex = 4;
			CheckLockXAxis.Text = "Lock X Axis";
			CheckLockXAxis.UseVisualStyleBackColor = true;
			CheckLockXAxis.CheckedChanged += CheckLockXAxis_CheckedChanged;
			// 
			// NUDRollingAveragePeriod
			// 
			NUDRollingAveragePeriod.Enabled = false;
			NUDRollingAveragePeriod.Location = new Point(56, 67);
			NUDRollingAveragePeriod.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
			NUDRollingAveragePeriod.Name = "NUDRollingAveragePeriod";
			NUDRollingAveragePeriod.Size = new Size(68, 23);
			NUDRollingAveragePeriod.TabIndex = 3;
			NUDRollingAveragePeriod.Value = new decimal(new int[] { 1, 0, 0, 0 });
			NUDRollingAveragePeriod.ValueChanged += NUDRollingAveragePeriod_ValueChanged;
			// 
			// StaticLabelRollingAveragePeriod
			// 
			StaticLabelRollingAveragePeriod.AutoSize = true;
			StaticLabelRollingAveragePeriod.Enabled = false;
			StaticLabelRollingAveragePeriod.Location = new Point(6, 69);
			StaticLabelRollingAveragePeriod.Name = "StaticLabelRollingAveragePeriod";
			StaticLabelRollingAveragePeriod.Size = new Size(44, 15);
			StaticLabelRollingAveragePeriod.TabIndex = 2;
			StaticLabelRollingAveragePeriod.Text = "Period:";
			// 
			// CheckRollingAverage
			// 
			CheckRollingAverage.AutoSize = true;
			CheckRollingAverage.Enabled = false;
			CheckRollingAverage.Location = new Point(6, 47);
			CheckRollingAverage.Name = "CheckRollingAverage";
			CheckRollingAverage.Size = new Size(109, 19);
			CheckRollingAverage.TabIndex = 1;
			CheckRollingAverage.Text = "Rolling Average";
			CheckRollingAverage.UseVisualStyleBackColor = true;
			CheckRollingAverage.CheckedChanged += CheckRollingAverage_CheckedChanged;
			// 
			// CheckLinearRegression
			// 
			CheckLinearRegression.AutoSize = true;
			CheckLinearRegression.Enabled = false;
			CheckLinearRegression.Location = new Point(6, 22);
			CheckLinearRegression.Name = "CheckLinearRegression";
			CheckLinearRegression.Size = new Size(118, 19);
			CheckLinearRegression.TabIndex = 0;
			CheckLinearRegression.Text = "Linear Regression";
			CheckLinearRegression.UseVisualStyleBackColor = true;
			CheckLinearRegression.CheckedChanged += CheckLinearRegression_CheckedChanged;
			// 
			// PlaygroundForm
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1033, 509);
			Controls.Add(GroupChartControls);
			Controls.Add(TSMain);
			Controls.Add(PlotMain);
			Name = "PlaygroundForm";
			Text = "{title}";
			TSMain.ResumeLayout(false);
			TSMain.PerformLayout();
			GroupChartControls.ResumeLayout(false);
			GroupChartControls.PerformLayout();
			((System.ComponentModel.ISupportInitialize)NUDRollingAveragePeriod).EndInit();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion
		private OpenFileDialog OFDMain;
		private SaveFileDialog SFDMain;
		private ScottPlot.WinForms.FormsPlot PlotMain;
		private ToolStrip TSMain;
		private ToolStripButton TSBOpenFile;
		private ToolStripButton TSBSaveAs;
		private ToolStripSeparator TSSeparator1;
		private ToolStripDropDownButton TSDDBViews;
		private GroupBox GroupChartControls;
		private CheckBox CheckLinearRegression;
		private NumericUpDown NUDRollingAveragePeriod;
		private Label StaticLabelRollingAveragePeriod;
		private CheckBox CheckRollingAverage;
		private CheckBox CheckLockYAxis;
		private CheckBox CheckLockXAxis;
	}
}