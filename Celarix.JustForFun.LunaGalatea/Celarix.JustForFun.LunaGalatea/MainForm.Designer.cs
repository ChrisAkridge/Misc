using Timer = System.Windows.Forms.Timer;

namespace Celarix.JustForFun.LunaGalatea
{
    partial class MainForm
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
            this.MainPanel = new System.Windows.Forms.Panel();
            this.TimerMain = new Timer();
            this.SuspendLayout();
            // 
            // MainPanel
            // 
            this.MainPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MainPanel.Location = new System.Drawing.Point(12, 12);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.Size = new System.Drawing.Size(554, 720);
            this.MainPanel.TabIndex = 0;
            this.MainPanel.BorderStyle = BorderStyle.Fixed3D;
            this.MainPanel.AutoScroll = true;
            //
            // TimerMain
            //
            this.TimerMain.Interval = 1000;
            this.TimerMain.Enabled = true;
            this.TimerMain.Tick += TimerMain_Tick;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(578, 744);
            this.Controls.Add(this.MainPanel);
            this.Name = "MainForm";
            this.Text = "LunaGalatea";
            this.ResumeLayout(false);

        }

        #endregion

        private Panel MainPanel;
        private Timer TimerMain;
    }
}