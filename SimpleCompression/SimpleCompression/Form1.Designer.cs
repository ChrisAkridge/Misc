namespace SimpleCompression
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.ToolStrip = new System.Windows.Forms.ToolStrip();
			this.TSBOpenFile = new System.Windows.Forms.ToolStripButton();
			this.TSS1 = new System.Windows.Forms.ToolStripSeparator();
			this.TSBCompressOneStep = new System.Windows.Forms.ToolStripButton();
			this.TSBCompressFull = new System.Windows.Forms.ToolStripButton();
			this.TSS2 = new System.Windows.Forms.ToolStripSeparator();
			this.TSBDecompressOneStep = new System.Windows.Forms.ToolStripButton();
			this.TSBDecompressFull = new System.Windows.Forms.ToolStripButton();
			this.StaticLabelCompressionMethod = new System.Windows.Forms.Label();
			this.RBBinaryRLE = new System.Windows.Forms.RadioButton();
			this.StaticLabelFile = new System.Windows.Forms.Label();
			this.RBTextFile = new System.Windows.Forms.RadioButton();
			this.RBBinaryFile = new System.Windows.Forms.RadioButton();
			this.TextBoxFileContents = new System.Windows.Forms.TextBox();
			this.StaticLabelResult = new System.Windows.Forms.Label();
			this.LinkLabelCopyAsBytes = new System.Windows.Forms.LinkLabel();
			this.LinkLabelCopyAsText = new System.Windows.Forms.LinkLabel();
			this.TextCompressionResult = new System.Windows.Forms.TextBox();
			this.OFDOpenFile = new System.Windows.Forms.OpenFileDialog();
			this.LabelStatus = new System.Windows.Forms.Label();
			this.ButtonConvert = new System.Windows.Forms.Button();
			this.ButtonQuickCompress = new System.Windows.Forms.Button();
			this.RadioDontDisplay = new System.Windows.Forms.RadioButton();
			this.ToolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// ToolStrip
			// 
			this.ToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TSBOpenFile,
            this.TSS1,
            this.TSBCompressOneStep,
            this.TSBCompressFull,
            this.TSS2,
            this.TSBDecompressOneStep,
            this.TSBDecompressFull});
			this.ToolStrip.Location = new System.Drawing.Point(0, 0);
			this.ToolStrip.Name = "ToolStrip";
			this.ToolStrip.Size = new System.Drawing.Size(734, 25);
			this.ToolStrip.TabIndex = 0;
			this.ToolStrip.Text = "toolStrip1";
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
			// TSS1
			// 
			this.TSS1.Name = "TSS1";
			this.TSS1.Size = new System.Drawing.Size(6, 25);
			// 
			// TSBCompressOneStep
			// 
			this.TSBCompressOneStep.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.TSBCompressOneStep.Image = ((System.Drawing.Image)(resources.GetObject("TSBCompressOneStep.Image")));
			this.TSBCompressOneStep.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.TSBCompressOneStep.Name = "TSBCompressOneStep";
			this.TSBCompressOneStep.Size = new System.Drawing.Size(115, 22);
			this.TSBCompressOneStep.Text = "Compress O&ne Step";
			this.TSBCompressOneStep.Click += new System.EventHandler(this.TSBCompressOneStep_Click);
			// 
			// TSBCompressFull
			// 
			this.TSBCompressFull.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.TSBCompressFull.Image = ((System.Drawing.Image)(resources.GetObject("TSBCompressFull.Image")));
			this.TSBCompressFull.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.TSBCompressFull.Name = "TSBCompressFull";
			this.TSBCompressFull.Size = new System.Drawing.Size(86, 22);
			this.TSBCompressFull.Text = "&Compress Full";
			// 
			// TSS2
			// 
			this.TSS2.Name = "TSS2";
			this.TSS2.Size = new System.Drawing.Size(6, 25);
			// 
			// TSBDecompressOneStep
			// 
			this.TSBDecompressOneStep.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.TSBDecompressOneStep.Image = ((System.Drawing.Image)(resources.GetObject("TSBDecompressOneStep.Image")));
			this.TSBDecompressOneStep.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.TSBDecompressOneStep.Name = "TSBDecompressOneStep";
			this.TSBDecompressOneStep.Size = new System.Drawing.Size(127, 22);
			this.TSBDecompressOneStep.Text = "Decompress On&e Step";
			// 
			// TSBDecompressFull
			// 
			this.TSBDecompressFull.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.TSBDecompressFull.Image = ((System.Drawing.Image)(resources.GetObject("TSBDecompressFull.Image")));
			this.TSBDecompressFull.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.TSBDecompressFull.Name = "TSBDecompressFull";
			this.TSBDecompressFull.Size = new System.Drawing.Size(98, 22);
			this.TSBDecompressFull.Text = "&Decompress Full";
			// 
			// StaticLabelCompressionMethod
			// 
			this.StaticLabelCompressionMethod.AutoSize = true;
			this.StaticLabelCompressionMethod.Location = new System.Drawing.Point(12, 32);
			this.StaticLabelCompressionMethod.Name = "StaticLabelCompressionMethod";
			this.StaticLabelCompressionMethod.Size = new System.Drawing.Size(121, 13);
			this.StaticLabelCompressionMethod.TabIndex = 1;
			this.StaticLabelCompressionMethod.Text = "Compression Method:";
			// 
			// RBBinaryRLE
			// 
			this.RBBinaryRLE.AutoSize = true;
			this.RBBinaryRLE.Location = new System.Drawing.Point(139, 30);
			this.RBBinaryRLE.Name = "RBBinaryRLE";
			this.RBBinaryRLE.Size = new System.Drawing.Size(78, 17);
			this.RBBinaryRLE.TabIndex = 2;
			this.RBBinaryRLE.Text = "&Binary RLE";
			this.RBBinaryRLE.UseVisualStyleBackColor = true;
			// 
			// StaticLabelFile
			// 
			this.StaticLabelFile.AutoSize = true;
			this.StaticLabelFile.Location = new System.Drawing.Point(12, 55);
			this.StaticLabelFile.Name = "StaticLabelFile";
			this.StaticLabelFile.Size = new System.Drawing.Size(28, 13);
			this.StaticLabelFile.TabIndex = 3;
			this.StaticLabelFile.Text = "File:";
			// 
			// RBTextFile
			// 
			this.RBTextFile.AutoSize = true;
			this.RBTextFile.Checked = true;
			this.RBTextFile.Location = new System.Drawing.Point(46, 53);
			this.RBTextFile.Name = "RBTextFile";
			this.RBTextFile.Size = new System.Drawing.Size(100, 17);
			this.RBTextFile.TabIndex = 4;
			this.RBTextFile.TabStop = true;
			this.RBTextFile.Text = "Text File (UTF8)";
			this.RBTextFile.UseVisualStyleBackColor = true;
			// 
			// RBBinaryFile
			// 
			this.RBBinaryFile.AutoSize = true;
			this.RBBinaryFile.Location = new System.Drawing.Point(152, 53);
			this.RBBinaryFile.Name = "RBBinaryFile";
			this.RBBinaryFile.Size = new System.Drawing.Size(78, 17);
			this.RBBinaryFile.TabIndex = 5;
			this.RBBinaryFile.Text = "Binary &File";
			this.RBBinaryFile.UseVisualStyleBackColor = true;
			// 
			// TextBoxFileContents
			// 
			this.TextBoxFileContents.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TextBoxFileContents.Location = new System.Drawing.Point(15, 72);
			this.TextBoxFileContents.Multiline = true;
			this.TextBoxFileContents.Name = "TextBoxFileContents";
			this.TextBoxFileContents.ReadOnly = true;
			this.TextBoxFileContents.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.TextBoxFileContents.Size = new System.Drawing.Size(707, 128);
			this.TextBoxFileContents.TabIndex = 6;
			// 
			// StaticLabelResult
			// 
			this.StaticLabelResult.AutoSize = true;
			this.StaticLabelResult.Location = new System.Drawing.Point(12, 207);
			this.StaticLabelResult.Name = "StaticLabelResult";
			this.StaticLabelResult.Size = new System.Drawing.Size(129, 13);
			this.StaticLabelResult.TabIndex = 7;
			this.StaticLabelResult.Text = "Resulting Compression:";
			// 
			// LinkLabelCopyAsBytes
			// 
			this.LinkLabelCopyAsBytes.AutoSize = true;
			this.LinkLabelCopyAsBytes.Location = new System.Drawing.Point(645, 207);
			this.LinkLabelCopyAsBytes.Name = "LinkLabelCopyAsBytes";
			this.LinkLabelCopyAsBytes.Size = new System.Drawing.Size(77, 13);
			this.LinkLabelCopyAsBytes.TabIndex = 8;
			this.LinkLabelCopyAsBytes.TabStop = true;
			this.LinkLabelCopyAsBytes.Text = "Co&py as Bytes";
			// 
			// LinkLabelCopyAsText
			// 
			this.LinkLabelCopyAsText.AutoSize = true;
			this.LinkLabelCopyAsText.Location = new System.Drawing.Point(569, 207);
			this.LinkLabelCopyAsText.Name = "LinkLabelCopyAsText";
			this.LinkLabelCopyAsText.Size = new System.Drawing.Size(70, 13);
			this.LinkLabelCopyAsText.TabIndex = 9;
			this.LinkLabelCopyAsText.TabStop = true;
			this.LinkLabelCopyAsText.Text = "Cop&y as Text";
			// 
			// TextCompressionResult
			// 
			this.TextCompressionResult.Location = new System.Drawing.Point(15, 224);
			this.TextCompressionResult.Multiline = true;
			this.TextCompressionResult.Name = "TextCompressionResult";
			this.TextCompressionResult.ReadOnly = true;
			this.TextCompressionResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.TextCompressionResult.Size = new System.Drawing.Size(707, 175);
			this.TextCompressionResult.TabIndex = 10;
			// 
			// OFDOpenFile
			// 
			this.OFDOpenFile.AddExtension = false;
			this.OFDOpenFile.Filter = "All files (*.*)|*.*";
			this.OFDOpenFile.Title = "Simple Compressor";
			// 
			// LabelStatus
			// 
			this.LabelStatus.AutoSize = true;
			this.LabelStatus.Location = new System.Drawing.Point(15, 406);
			this.LabelStatus.Name = "LabelStatus";
			this.LabelStatus.Size = new System.Drawing.Size(100, 13);
			this.LabelStatus.TabIndex = 11;
			this.LabelStatus.Text = "Ready to load file.";
			// 
			// ButtonConvert
			// 
			this.ButtonConvert.Location = new System.Drawing.Point(647, 45);
			this.ButtonConvert.Name = "ButtonConvert";
			this.ButtonConvert.Size = new System.Drawing.Size(75, 23);
			this.ButtonConvert.TabIndex = 12;
			this.ButtonConvert.Text = "Convert";
			this.ButtonConvert.UseVisualStyleBackColor = true;
			this.ButtonConvert.Click += new System.EventHandler(this.ButtonConvert_Click);
			// 
			// ButtonQuickCompress
			// 
			this.ButtonQuickCompress.Location = new System.Drawing.Point(566, 45);
			this.ButtonQuickCompress.Name = "ButtonQuickCompress";
			this.ButtonQuickCompress.Size = new System.Drawing.Size(75, 23);
			this.ButtonQuickCompress.TabIndex = 13;
			this.ButtonQuickCompress.Text = "&Quick";
			this.ButtonQuickCompress.UseVisualStyleBackColor = true;
			this.ButtonQuickCompress.Click += new System.EventHandler(this.ButtonQuickCompress_Click);
			// 
			// RadioDontDisplay
			// 
			this.RadioDontDisplay.AutoSize = true;
			this.RadioDontDisplay.Location = new System.Drawing.Point(236, 53);
			this.RadioDontDisplay.Name = "RadioDontDisplay";
			this.RadioDontDisplay.Size = new System.Drawing.Size(94, 17);
			this.RadioDontDisplay.TabIndex = 14;
			this.RadioDontDisplay.TabStop = true;
			this.RadioDontDisplay.Text = "&Don\'t Display";
			this.RadioDontDisplay.UseVisualStyleBackColor = true;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(734, 427);
			this.Controls.Add(this.RadioDontDisplay);
			this.Controls.Add(this.ButtonQuickCompress);
			this.Controls.Add(this.ButtonConvert);
			this.Controls.Add(this.LabelStatus);
			this.Controls.Add(this.TextCompressionResult);
			this.Controls.Add(this.LinkLabelCopyAsText);
			this.Controls.Add(this.LinkLabelCopyAsBytes);
			this.Controls.Add(this.StaticLabelResult);
			this.Controls.Add(this.TextBoxFileContents);
			this.Controls.Add(this.RBBinaryFile);
			this.Controls.Add(this.RBTextFile);
			this.Controls.Add(this.StaticLabelFile);
			this.Controls.Add(this.RBBinaryRLE);
			this.Controls.Add(this.StaticLabelCompressionMethod);
			this.Controls.Add(this.ToolStrip);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.Text = "Simple Compressor";
			this.ToolStrip.ResumeLayout(false);
			this.ToolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip ToolStrip;
		private System.Windows.Forms.ToolStripButton TSBOpenFile;
		private System.Windows.Forms.ToolStripSeparator TSS1;
		private System.Windows.Forms.ToolStripButton TSBCompressOneStep;
		private System.Windows.Forms.ToolStripButton TSBCompressFull;
		private System.Windows.Forms.ToolStripSeparator TSS2;
		private System.Windows.Forms.ToolStripButton TSBDecompressOneStep;
		private System.Windows.Forms.ToolStripButton TSBDecompressFull;
		private System.Windows.Forms.Label StaticLabelCompressionMethod;
		private System.Windows.Forms.RadioButton RBBinaryRLE;
		private System.Windows.Forms.Label StaticLabelFile;
		private System.Windows.Forms.RadioButton RBTextFile;
		private System.Windows.Forms.RadioButton RBBinaryFile;
		private System.Windows.Forms.TextBox TextBoxFileContents;
		private System.Windows.Forms.Label StaticLabelResult;
		private System.Windows.Forms.LinkLabel LinkLabelCopyAsBytes;
		private System.Windows.Forms.LinkLabel LinkLabelCopyAsText;
		private System.Windows.Forms.TextBox TextCompressionResult;
		private System.Windows.Forms.OpenFileDialog OFDOpenFile;
		private System.Windows.Forms.Label LabelStatus;
		private System.Windows.Forms.Button ButtonConvert;
		private System.Windows.Forms.Button ButtonQuickCompress;
		private System.Windows.Forms.RadioButton RadioDontDisplay;
	}
}

