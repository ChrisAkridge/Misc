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
			TSMain.SuspendLayout();
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
			PlotMain.Size = new Size(725, 463);
			PlotMain.TabIndex = 2;
			// 
			// TSMain
			// 
			TSMain.Items.AddRange(new ToolStripItem[] { TSBOpenFile, TSBSaveAs, TSSeparator1, TSDDBViews });
			TSMain.Location = new Point(0, 0);
			TSMain.Name = "TSMain";
			TSMain.Size = new Size(1027, 25);
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
			GroupChartControls.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			GroupChartControls.Location = new Point(743, 57);
			GroupChartControls.Name = "GroupChartControls";
			GroupChartControls.Size = new Size(272, 434);
			GroupChartControls.TabIndex = 5;
			GroupChartControls.TabStop = false;
			GroupChartControls.Text = "Controls";
			// 
			// PlaygroundForm
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1027, 503);
			Controls.Add(GroupChartControls);
			Controls.Add(TSMain);
			Controls.Add(PlotMain);
			Name = "PlaygroundForm";
			Text = "{title}";
			TSMain.ResumeLayout(false);
			TSMain.PerformLayout();
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
	}
}