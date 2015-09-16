namespace FileTools
{
	partial class CompressorGUI
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CompressorGUI));
			this.TSMain = new System.Windows.Forms.ToolStrip();
			this.TSBOpenFile = new System.Windows.Forms.ToolStripButton();
			this.TSBSaveAs = new System.Windows.Forms.ToolStripButton();
			this.StaticLabelCurrentData = new System.Windows.Forms.Label();
			this.HexCurrentData = new Be.Windows.Forms.HexBox();
			this.StaticLabelDictionary = new System.Windows.Forms.Label();
			this.ListViewDictionary = new System.Windows.Forms.ListView();
			this.ColumnKey = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.ColumnValueText = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.ColumnValueHex = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.ButtonDecompressFull = new System.Windows.Forms.Button();
			this.ButtonDecompressOneStep = new System.Windows.Forms.Button();
			this.ButtonCompressOneStep = new System.Windows.Forms.Button();
			this.ButtonCompressFull = new System.Windows.Forms.Button();
			this.Worker = new System.ComponentModel.BackgroundWorker();
			this.OFDOpenFile = new System.Windows.Forms.OpenFileDialog();
			this.TSMain.SuspendLayout();
			this.SuspendLayout();
			// 
			// TSMain
			// 
			this.TSMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TSBOpenFile,
            this.TSBSaveAs});
			this.TSMain.Location = new System.Drawing.Point(0, 0);
			this.TSMain.Name = "TSMain";
			this.TSMain.Size = new System.Drawing.Size(444, 25);
			this.TSMain.TabIndex = 0;
			this.TSMain.Text = "toolStrip1";
			// 
			// TSBOpenFile
			// 
			this.TSBOpenFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.TSBOpenFile.Image = ((System.Drawing.Image)(resources.GetObject("TSBOpenFile.Image")));
			this.TSBOpenFile.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.TSBOpenFile.Name = "TSBOpenFile";
			this.TSBOpenFile.Size = new System.Drawing.Size(49, 22);
			this.TSBOpenFile.Text = "&Open...";
			this.TSBOpenFile.Click += new System.EventHandler(this.TSBOpenFile_Click);
			// 
			// TSBSaveAs
			// 
			this.TSBSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.TSBSaveAs.Image = ((System.Drawing.Image)(resources.GetObject("TSBSaveAs.Image")));
			this.TSBSaveAs.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.TSBSaveAs.Name = "TSBSaveAs";
			this.TSBSaveAs.Size = new System.Drawing.Size(60, 22);
			this.TSBSaveAs.Text = "&Save As...";
			// 
			// StaticLabelCurrentData
			// 
			this.StaticLabelCurrentData.AutoSize = true;
			this.StaticLabelCurrentData.Location = new System.Drawing.Point(13, 29);
			this.StaticLabelCurrentData.Name = "StaticLabelCurrentData";
			this.StaticLabelCurrentData.Size = new System.Drawing.Size(76, 13);
			this.StaticLabelCurrentData.TabIndex = 1;
			this.StaticLabelCurrentData.Text = "Current Data:";
			// 
			// HexCurrentData
			// 
			this.HexCurrentData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.HexCurrentData.ColumnInfoVisible = true;
			this.HexCurrentData.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.HexCurrentData.Location = new System.Drawing.Point(16, 46);
			this.HexCurrentData.Name = "HexCurrentData";
			this.HexCurrentData.ReadOnly = true;
			this.HexCurrentData.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
			this.HexCurrentData.Size = new System.Drawing.Size(416, 172);
			this.HexCurrentData.StringViewVisible = true;
			this.HexCurrentData.TabIndex = 2;
			this.HexCurrentData.VScrollBarVisible = true;
			// 
			// StaticLabelDictionary
			// 
			this.StaticLabelDictionary.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.StaticLabelDictionary.AutoSize = true;
			this.StaticLabelDictionary.Location = new System.Drawing.Point(13, 221);
			this.StaticLabelDictionary.Name = "StaticLabelDictionary";
			this.StaticLabelDictionary.Size = new System.Drawing.Size(62, 13);
			this.StaticLabelDictionary.TabIndex = 3;
			this.StaticLabelDictionary.Text = "&Dictionary:";
			// 
			// ListViewDictionary
			// 
			this.ListViewDictionary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ListViewDictionary.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnKey,
            this.ColumnValueText,
            this.ColumnValueHex});
			this.ListViewDictionary.Location = new System.Drawing.Point(16, 239);
			this.ListViewDictionary.Name = "ListViewDictionary";
			this.ListViewDictionary.Size = new System.Drawing.Size(416, 183);
			this.ListViewDictionary.TabIndex = 4;
			this.ListViewDictionary.UseCompatibleStateImageBehavior = false;
			this.ListViewDictionary.View = System.Windows.Forms.View.Details;
			// 
			// ColumnKey
			// 
			this.ColumnKey.Text = "Key";
			// 
			// ColumnValueText
			// 
			this.ColumnValueText.Text = "Value (text)";
			this.ColumnValueText.Width = 74;
			// 
			// ColumnValueHex
			// 
			this.ColumnValueHex.Text = "Value (hex)";
			this.ColumnValueHex.Width = 81;
			// 
			// ButtonDecompressFull
			// 
			this.ButtonDecompressFull.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.ButtonDecompressFull.Enabled = false;
			this.ButtonDecompressFull.Location = new System.Drawing.Point(16, 428);
			this.ButtonDecompressFull.Name = "ButtonDecompressFull";
			this.ButtonDecompressFull.Size = new System.Drawing.Size(102, 23);
			this.ButtonDecompressFull.TabIndex = 5;
			this.ButtonDecompressFull.Text = "&Decompress Full";
			this.ButtonDecompressFull.UseVisualStyleBackColor = true;
			// 
			// ButtonDecompressOneStep
			// 
			this.ButtonDecompressOneStep.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.ButtonDecompressOneStep.Enabled = false;
			this.ButtonDecompressOneStep.Location = new System.Drawing.Point(124, 428);
			this.ButtonDecompressOneStep.Name = "ButtonDecompressOneStep";
			this.ButtonDecompressOneStep.Size = new System.Drawing.Size(105, 23);
			this.ButtonDecompressOneStep.TabIndex = 6;
			this.ButtonDecompressOneStep.Text = "D&ecompress One";
			this.ButtonDecompressOneStep.UseVisualStyleBackColor = true;
			// 
			// ButtonCompressOneStep
			// 
			this.ButtonCompressOneStep.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.ButtonCompressOneStep.Location = new System.Drawing.Point(235, 428);
			this.ButtonCompressOneStep.Name = "ButtonCompressOneStep";
			this.ButtonCompressOneStep.Size = new System.Drawing.Size(93, 23);
			this.ButtonCompressOneStep.TabIndex = 7;
			this.ButtonCompressOneStep.Text = "&Compress One";
			this.ButtonCompressOneStep.UseVisualStyleBackColor = true;
			this.ButtonCompressOneStep.Click += new System.EventHandler(this.ButtonCompressOneStep_Click);
			// 
			// ButtonCompressFull
			// 
			this.ButtonCompressFull.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.ButtonCompressFull.Location = new System.Drawing.Point(334, 428);
			this.ButtonCompressFull.Name = "ButtonCompressFull";
			this.ButtonCompressFull.Size = new System.Drawing.Size(99, 23);
			this.ButtonCompressFull.TabIndex = 8;
			this.ButtonCompressFull.Text = "&Compress Full";
			this.ButtonCompressFull.UseVisualStyleBackColor = true;
			this.ButtonCompressFull.Click += new System.EventHandler(this.ButtonCompressFull_Click);
			// 
			// Worker
			// 
			this.Worker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.Worker_DoWork);
			// 
			// OFDOpenFile
			// 
			this.OFDOpenFile.AddExtension = false;
			this.OFDOpenFile.Filter = "All files|*.*";
			this.OFDOpenFile.Title = "Compressor";
			// 
			// CompressorGUI
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(444, 454);
			this.Controls.Add(this.ButtonCompressFull);
			this.Controls.Add(this.ButtonCompressOneStep);
			this.Controls.Add(this.ButtonDecompressOneStep);
			this.Controls.Add(this.ButtonDecompressFull);
			this.Controls.Add(this.ListViewDictionary);
			this.Controls.Add(this.StaticLabelDictionary);
			this.Controls.Add(this.HexCurrentData);
			this.Controls.Add(this.StaticLabelCurrentData);
			this.Controls.Add(this.TSMain);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Name = "CompressorGUI";
			this.ShowIcon = false;
			this.Text = "Compressor";
			this.TSMain.ResumeLayout(false);
			this.TSMain.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip TSMain;
		private System.Windows.Forms.ToolStripButton TSBOpenFile;
		private System.Windows.Forms.ToolStripButton TSBSaveAs;
		private System.Windows.Forms.Label StaticLabelCurrentData;
		private Be.Windows.Forms.HexBox HexCurrentData;
		private System.Windows.Forms.Label StaticLabelDictionary;
		private System.Windows.Forms.ListView ListViewDictionary;
		private System.Windows.Forms.ColumnHeader ColumnKey;
		private System.Windows.Forms.ColumnHeader ColumnValueText;
		private System.Windows.Forms.ColumnHeader ColumnValueHex;
		private System.Windows.Forms.Button ButtonDecompressFull;
		private System.Windows.Forms.Button ButtonDecompressOneStep;
		private System.Windows.Forms.Button ButtonCompressOneStep;
		private System.Windows.Forms.Button ButtonCompressFull;
		private System.ComponentModel.BackgroundWorker Worker;
		private System.Windows.Forms.OpenFileDialog OFDOpenFile;
	}
}