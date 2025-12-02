namespace Celarix.IO.CustomCodepageHexView
{
    partial class OldMainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OldMainForm));
            this.ToolStripMain = new System.Windows.Forms.ToolStrip();
            this.TSBOpen = new System.Windows.Forms.ToolStripButton();
            this.TSSeparator0 = new System.Windows.Forms.ToolStripSeparator();
            this.TSLCodepage = new System.Windows.Forms.ToolStripLabel();
            this.TSCBCodepages = new System.Windows.Forms.ToolStripComboBox();
            this.HexMain = new WpfHexaEditor.HexEditor();
            this.HostHexEditor = new System.Windows.Forms.Integration.ElementHost();
            this.OFDMain = new System.Windows.Forms.OpenFileDialog();
            this.ToolStripMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // ToolStripMain
            // 
            this.ToolStripMain.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.ToolStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.TSBOpen,
            this.TSSeparator0,
            this.TSLCodepage,
            this.TSCBCodepages});
            this.ToolStripMain.Location = new System.Drawing.Point(0, 0);
            this.ToolStripMain.Name = "ToolStripMain";
            this.ToolStripMain.Size = new System.Drawing.Size(1071, 33);
            this.ToolStripMain.TabIndex = 0;
            this.ToolStripMain.Text = "toolStrip1";
            // 
            // TSBOpen
            // 
            this.TSBOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.TSBOpen.Image = ((System.Drawing.Image)(resources.GetObject("TSBOpen.Image")));
            this.TSBOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.TSBOpen.Name = "TSBOpen";
            this.TSBOpen.Size = new System.Drawing.Size(34, 28);
            this.TSBOpen.Text = "toolStripButton1";
            this.TSBOpen.Click += new System.EventHandler(this.TSBOpen_Click);
            // 
            // TSSeparator0
            // 
            this.TSSeparator0.Name = "TSSeparator0";
            this.TSSeparator0.Size = new System.Drawing.Size(6, 33);
            // 
            // TSLCodepage
            // 
            this.TSLCodepage.Name = "TSLCodepage";
            this.TSLCodepage.Size = new System.Drawing.Size(98, 28);
            this.TSLCodepage.Text = "Codepage:";
            // 
            // TSCBCodepages
            // 
            this.TSCBCodepages.Name = "TSCBCodepages";
            this.TSCBCodepages.Size = new System.Drawing.Size(350, 33);
            // 
            // HostHexEditor
            // 
            this.HostHexEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HostHexEditor.Location = new System.Drawing.Point(0, 0);
            this.HostHexEditor.Name = "HostHexEditor";
            this.HostHexEditor.Size = new System.Drawing.Size(868, 412);
            this.HostHexEditor.TabIndex = 3;
            this.HostHexEditor.Text = "HostHexEditor";
            this.HostHexEditor.Child = HexMain;
            // 
            // OFDMain
            // 
            this.OFDMain.Title = "Open File...";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1071, 772);
            this.Controls.Add(this.ToolStripMain);
            this.Controls.Add(this.HostHexEditor);
            this.Name = "MainForm";
            this.Text = "Custom Codepage Hex Viewer";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ToolStripMain.ResumeLayout(false);
            this.ToolStripMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip ToolStripMain;
        private System.Windows.Forms.ToolStripButton TSBOpen;
        private System.Windows.Forms.ToolStripSeparator TSSeparator0;
        private System.Windows.Forms.ToolStripLabel TSLCodepage;
        private System.Windows.Forms.ToolStripComboBox TSCBCodepages;
        private System.Windows.Forms.Integration.ElementHost HostHexEditor;
        private WpfHexaEditor.HexEditor HexMain;
        private System.Windows.Forms.OpenFileDialog OFDMain;
    }
}
