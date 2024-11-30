namespace Celarix.VioletFacet.RandomRewardTimer
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            StaticLabelMinDelay = new Label();
            NUDMinimumDelay = new NumericUpDown();
            NUDMaximumDelay = new NumericUpDown();
            StaticLabelMaxDelay = new Label();
            TimerPlaySound = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)NUDMinimumDelay).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NUDMaximumDelay).BeginInit();
            SuspendLayout();
            // 
            // StaticLabelMinDelay
            // 
            StaticLabelMinDelay.AutoSize = true;
            StaticLabelMinDelay.Location = new Point(12, 9);
            StaticLabelMinDelay.Name = "StaticLabelMinDelay";
            StaticLabelMinDelay.Size = new Size(123, 15);
            StaticLabelMinDelay.TabIndex = 0;
            StaticLabelMinDelay.Text = "Minimum Delay (sec):";
            // 
            // NUDMinimumDelay
            // 
            NUDMinimumDelay.Location = new Point(12, 27);
            NUDMinimumDelay.Maximum = new decimal(new int[] { 9998, 0, 0, 0 });
            NUDMinimumDelay.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            NUDMinimumDelay.Name = "NUDMinimumDelay";
            NUDMinimumDelay.Size = new Size(123, 23);
            NUDMinimumDelay.TabIndex = 1;
            NUDMinimumDelay.Value = new decimal(new int[] { 240, 0, 0, 0 });
            NUDMinimumDelay.ValueChanged += NUDMinimumDelay_ValueChanged;
            // 
            // NUDMaximumDelay
            // 
            NUDMaximumDelay.Location = new Point(141, 27);
            NUDMaximumDelay.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            NUDMaximumDelay.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            NUDMaximumDelay.Name = "NUDMaximumDelay";
            NUDMaximumDelay.Size = new Size(123, 23);
            NUDMaximumDelay.TabIndex = 3;
            NUDMaximumDelay.Value = new decimal(new int[] { 480, 0, 0, 0 });
            NUDMaximumDelay.ValueChanged += NUDMaximumDelay_ValueChanged;
            // 
            // StaticLabelMaxDelay
            // 
            StaticLabelMaxDelay.AutoSize = true;
            StaticLabelMaxDelay.Location = new Point(141, 9);
            StaticLabelMaxDelay.Name = "StaticLabelMaxDelay";
            StaticLabelMaxDelay.Size = new Size(125, 15);
            StaticLabelMaxDelay.TabIndex = 2;
            StaticLabelMaxDelay.Text = "Maximum Delay (sec):";
            // 
            // TimerPlaySound
            // 
            TimerPlaySound.Enabled = true;
            TimerPlaySound.Interval = 240000;
            TimerPlaySound.Tick += TimerPlaySound_Tick;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(270, 58);
            Controls.Add(NUDMaximumDelay);
            Controls.Add(StaticLabelMaxDelay);
            Controls.Add(NUDMinimumDelay);
            Controls.Add(StaticLabelMinDelay);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "MainForm";
            Text = "Random Reward Timer";
            Load += MainForm_Load;
            ((System.ComponentModel.ISupportInitialize)NUDMinimumDelay).EndInit();
            ((System.ComponentModel.ISupportInitialize)NUDMaximumDelay).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label StaticLabelMinDelay;
        private NumericUpDown NUDMinimumDelay;
        private NumericUpDown NUDMaximumDelay;
        private Label StaticLabelMaxDelay;
        private System.Windows.Forms.Timer TimerPlaySound;
    }
}