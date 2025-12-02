namespace Celarix.IO.CustomCodepageHexView
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
            StaticLabelCodepage = new System.Windows.Forms.Label();
            ComboCodepages = new System.Windows.Forms.ComboBox();
            TrackFilePosition = new System.Windows.Forms.TrackBar();
            LabelFileMinimumAddress = new System.Windows.Forms.Label();
            LabelFileMaximumAddress = new System.Windows.Forms.Label();
            TextBoxSeekAddress = new System.Windows.Forms.TextBox();
            StaticLabelSeekAddress = new System.Windows.Forms.Label();
            ButtonSeek = new System.Windows.Forms.Button();
            ButtonMinus4Kilobytes = new System.Windows.Forms.Button();
            ButtonMinus256Bytes = new System.Windows.Forms.Button();
            ButtonMinus1Page = new System.Windows.Forms.Button();
            ButtonPlus1Page = new System.Windows.Forms.Button();
            ButtonPlus256Bytes = new System.Windows.Forms.Button();
            ButtonPlus4Kilobytes = new System.Windows.Forms.Button();
            TextHexView = new System.Windows.Forms.TextBox();
            ButtonOpenFile = new System.Windows.Forms.Button();
            OFDMain = new System.Windows.Forms.OpenFileDialog();
            ((System.ComponentModel.ISupportInitialize)TrackFilePosition).BeginInit();
            SuspendLayout();
            // 
            // StaticLabelCodepage
            // 
            StaticLabelCodepage.AutoSize = true;
            StaticLabelCodepage.Location = new System.Drawing.Point(12, 9);
            StaticLabelCodepage.Name = "StaticLabelCodepage";
            StaticLabelCodepage.Size = new System.Drawing.Size(64, 15);
            StaticLabelCodepage.TabIndex = 0;
            StaticLabelCodepage.Text = "Codepage:";
            // 
            // ComboCodepages
            // 
            ComboCodepages.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            ComboCodepages.FormattingEnabled = true;
            ComboCodepages.Items.AddRange(new object[] { "Unicode Latin-1", "Celarian All-Printable" });
            ComboCodepages.Location = new System.Drawing.Point(82, 6);
            ComboCodepages.Name = "ComboCodepages";
            ComboCodepages.Size = new System.Drawing.Size(752, 23);
            ComboCodepages.TabIndex = 1;
            ComboCodepages.SelectedIndexChanged += ComboCodepages_SelectedIndexChanged;
            // 
            // TrackFilePosition
            // 
            TrackFilePosition.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            TrackFilePosition.Enabled = false;
            TrackFilePosition.Location = new System.Drawing.Point(12, 35);
            TrackFilePosition.Name = "TrackFilePosition";
            TrackFilePosition.Size = new System.Drawing.Size(820, 45);
            TrackFilePosition.TabIndex = 2;
            TrackFilePosition.Scroll += TrackFilePosition_Scroll;
            // 
            // LabelFileMinimumAddress
            // 
            LabelFileMinimumAddress.AutoSize = true;
            LabelFileMinimumAddress.Location = new System.Drawing.Point(12, 65);
            LabelFileMinimumAddress.Name = "LabelFileMinimumAddress";
            LabelFileMinimumAddress.Size = new System.Drawing.Size(30, 15);
            LabelFileMinimumAddress.TabIndex = 3;
            LabelFileMinimumAddress.Text = "0x00";
            // 
            // LabelFileMaximumAddress
            // 
            LabelFileMaximumAddress.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            LabelFileMaximumAddress.AutoSize = true;
            LabelFileMaximumAddress.Location = new System.Drawing.Point(804, 65);
            LabelFileMaximumAddress.Name = "LabelFileMaximumAddress";
            LabelFileMaximumAddress.Size = new System.Drawing.Size(30, 15);
            LabelFileMaximumAddress.TabIndex = 4;
            LabelFileMaximumAddress.Text = "0xFF";
            LabelFileMaximumAddress.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // TextBoxSeekAddress
            // 
            TextBoxSeekAddress.Location = new System.Drawing.Point(123, 86);
            TextBoxSeekAddress.Name = "TextBoxSeekAddress";
            TextBoxSeekAddress.Size = new System.Drawing.Size(170, 23);
            TextBoxSeekAddress.TabIndex = 5;
            TextBoxSeekAddress.TextChanged += TextBoxSeekAddress_TextChanged;
            // 
            // StaticLabelSeekAddress
            // 
            StaticLabelSeekAddress.AutoSize = true;
            StaticLabelSeekAddress.Location = new System.Drawing.Point(12, 89);
            StaticLabelSeekAddress.Name = "StaticLabelSeekAddress";
            StaticLabelSeekAddress.Size = new System.Drawing.Size(105, 15);
            StaticLabelSeekAddress.TabIndex = 6;
            StaticLabelSeekAddress.Text = "Seek to address: 0x";
            // 
            // ButtonSeek
            // 
            ButtonSeek.Enabled = false;
            ButtonSeek.Location = new System.Drawing.Point(299, 85);
            ButtonSeek.Name = "ButtonSeek";
            ButtonSeek.Size = new System.Drawing.Size(47, 23);
            ButtonSeek.TabIndex = 7;
            ButtonSeek.Text = "&Seek";
            ButtonSeek.UseVisualStyleBackColor = true;
            ButtonSeek.Click += ButtonSeek_Click;
            // 
            // ButtonMinus4Kilobytes
            // 
            ButtonMinus4Kilobytes.Enabled = false;
            ButtonMinus4Kilobytes.Location = new System.Drawing.Point(514, 85);
            ButtonMinus4Kilobytes.Name = "ButtonMinus4Kilobytes";
            ButtonMinus4Kilobytes.Size = new System.Drawing.Size(75, 23);
            ButtonMinus4Kilobytes.TabIndex = 8;
            ButtonMinus4Kilobytes.Text = "-0x1000";
            ButtonMinus4Kilobytes.UseVisualStyleBackColor = true;
            ButtonMinus4Kilobytes.Click += ButtonMinus4Kilobytes_Click;
            // 
            // ButtonMinus256Bytes
            // 
            ButtonMinus256Bytes.Enabled = false;
            ButtonMinus256Bytes.Location = new System.Drawing.Point(433, 85);
            ButtonMinus256Bytes.Name = "ButtonMinus256Bytes";
            ButtonMinus256Bytes.Size = new System.Drawing.Size(75, 23);
            ButtonMinus256Bytes.TabIndex = 9;
            ButtonMinus256Bytes.Text = "-0x100";
            ButtonMinus256Bytes.UseVisualStyleBackColor = true;
            ButtonMinus256Bytes.Click += ButtonMinus256Bytes_Click;
            // 
            // ButtonMinus1Page
            // 
            ButtonMinus1Page.Enabled = false;
            ButtonMinus1Page.Location = new System.Drawing.Point(352, 85);
            ButtonMinus1Page.Name = "ButtonMinus1Page";
            ButtonMinus1Page.Size = new System.Drawing.Size(75, 23);
            ButtonMinus1Page.TabIndex = 10;
            ButtonMinus1Page.Text = "-1 Page";
            ButtonMinus1Page.UseVisualStyleBackColor = true;
            ButtonMinus1Page.Click += ButtonMinus1Page_Click;
            // 
            // ButtonPlus1Page
            // 
            ButtonPlus1Page.Enabled = false;
            ButtonPlus1Page.Location = new System.Drawing.Point(757, 85);
            ButtonPlus1Page.Name = "ButtonPlus1Page";
            ButtonPlus1Page.Size = new System.Drawing.Size(75, 23);
            ButtonPlus1Page.TabIndex = 13;
            ButtonPlus1Page.Text = "+1 Page";
            ButtonPlus1Page.UseVisualStyleBackColor = true;
            ButtonPlus1Page.Click += ButtonPlus1Page_Click;
            // 
            // ButtonPlus256Bytes
            // 
            ButtonPlus256Bytes.Enabled = false;
            ButtonPlus256Bytes.Location = new System.Drawing.Point(676, 85);
            ButtonPlus256Bytes.Name = "ButtonPlus256Bytes";
            ButtonPlus256Bytes.Size = new System.Drawing.Size(75, 23);
            ButtonPlus256Bytes.TabIndex = 12;
            ButtonPlus256Bytes.Text = "+0x100";
            ButtonPlus256Bytes.UseVisualStyleBackColor = true;
            ButtonPlus256Bytes.Click += ButtonPlus256Bytes_Click;
            // 
            // ButtonPlus4Kilobytes
            // 
            ButtonPlus4Kilobytes.Enabled = false;
            ButtonPlus4Kilobytes.Location = new System.Drawing.Point(595, 85);
            ButtonPlus4Kilobytes.Name = "ButtonPlus4Kilobytes";
            ButtonPlus4Kilobytes.Size = new System.Drawing.Size(75, 23);
            ButtonPlus4Kilobytes.TabIndex = 11;
            ButtonPlus4Kilobytes.Text = "+0x1000";
            ButtonPlus4Kilobytes.UseVisualStyleBackColor = true;
            ButtonPlus4Kilobytes.Click += ButtonPlus4Kilobytes_Click;
            // 
            // TextHexView
            // 
            TextHexView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            TextHexView.BackColor = System.Drawing.Color.White;
            TextHexView.Font = new System.Drawing.Font("Unifont", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            TextHexView.Location = new System.Drawing.Point(12, 117);
            TextHexView.Multiline = true;
            TextHexView.Name = "TextHexView";
            TextHexView.ReadOnly = true;
            TextHexView.ScrollBars = System.Windows.Forms.ScrollBars.Horizontal;
            TextHexView.Size = new System.Drawing.Size(822, 359);
            TextHexView.TabIndex = 14;
            TextHexView.WordWrap = false;
            // 
            // ButtonOpenFile
            // 
            ButtonOpenFile.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            ButtonOpenFile.Location = new System.Drawing.Point(12, 486);
            ButtonOpenFile.Name = "ButtonOpenFile";
            ButtonOpenFile.Size = new System.Drawing.Size(75, 23);
            ButtonOpenFile.TabIndex = 15;
            ButtonOpenFile.Text = "&Open...";
            ButtonOpenFile.UseVisualStyleBackColor = true;
            ButtonOpenFile.Click += ButtonOpenFile_Click;
            // 
            // OFDMain
            // 
            OFDMain.Filter = "All files|*.*";
            OFDMain.Title = "Custom Codepage Hex View";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(846, 521);
            Controls.Add(ButtonOpenFile);
            Controls.Add(TextHexView);
            Controls.Add(ButtonPlus1Page);
            Controls.Add(ButtonPlus256Bytes);
            Controls.Add(ButtonPlus4Kilobytes);
            Controls.Add(ButtonMinus1Page);
            Controls.Add(ButtonMinus256Bytes);
            Controls.Add(ButtonMinus4Kilobytes);
            Controls.Add(ButtonSeek);
            Controls.Add(StaticLabelSeekAddress);
            Controls.Add(TextBoxSeekAddress);
            Controls.Add(LabelFileMaximumAddress);
            Controls.Add(LabelFileMinimumAddress);
            Controls.Add(TrackFilePosition);
            Controls.Add(ComboCodepages);
            Controls.Add(StaticLabelCodepage);
            MinimumSize = new System.Drawing.Size(862, 500);
            Name = "MainForm";
            Text = "Custom Codepage Hex View";
            ResizeEnd += MainForm_ResizeEnd;
            ((System.ComponentModel.ISupportInitialize)TrackFilePosition).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label StaticLabelCodepage;
        private System.Windows.Forms.ComboBox ComboCodepages;
        private System.Windows.Forms.TrackBar TrackFilePosition;
        private System.Windows.Forms.Label LabelFileMinimumAddress;
        private System.Windows.Forms.Label LabelFileMaximumAddress;
        private System.Windows.Forms.TextBox TextBoxSeekAddress;
        private System.Windows.Forms.Label StaticLabelSeekAddress;
        private System.Windows.Forms.Button ButtonSeek;
        private System.Windows.Forms.Button ButtonMinus4Kilobytes;
        private System.Windows.Forms.Button ButtonMinus256Bytes;
        private System.Windows.Forms.Button ButtonMinus1Page;
        private System.Windows.Forms.Button ButtonPlus1Page;
        private System.Windows.Forms.Button ButtonPlus256Bytes;
        private System.Windows.Forms.Button ButtonPlus4Kilobytes;
        private System.Windows.Forms.TextBox TextHexView;
        private System.Windows.Forms.Button ButtonOpenFile;
        private System.Windows.Forms.OpenFileDialog OFDMain;
    }
}