namespace Celarix.JustForFun.LunaGalatea
{
    partial class CountdownDetailsForm
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
            components = new System.ComponentModel.Container();
            ListCountdowns = new ListBox();
            LabelCountdownName = new Label();
            LabelNextOccurrence = new Label();
            ProgressToNextOccurrence = new ProgressBar();
            LabelProgressLastOccurrence = new Label();
            LabelProgressNextOccurrence = new Label();
            LabelMainCountdown = new Label();
            TextDetails = new TextBox();
            TimerMain = new System.Windows.Forms.Timer(components);
            SuspendLayout();
            // 
            // ListCountdowns
            // 
            ListCountdowns.FormattingEnabled = true;
            ListCountdowns.ItemHeight = 15;
            ListCountdowns.Location = new Point(12, 12);
            ListCountdowns.Name = "ListCountdowns";
            ListCountdowns.Size = new Size(270, 424);
            ListCountdowns.TabIndex = 0;
            ListCountdowns.SelectedIndexChanged += ListCountdowns_SelectedIndexChanged;
            // 
            // LabelCountdownName
            // 
            LabelCountdownName.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            LabelCountdownName.Location = new Point(288, 12);
            LabelCountdownName.Name = "LabelCountdownName";
            LabelCountdownName.Size = new Size(500, 30);
            LabelCountdownName.TabIndex = 1;
            LabelCountdownName.Text = "(name)";
            LabelCountdownName.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // LabelNextOccurrence
            // 
            LabelNextOccurrence.AutoSize = true;
            LabelNextOccurrence.Location = new Point(288, 42);
            LabelNextOccurrence.Name = "LabelNextOccurrence";
            LabelNextOccurrence.Size = new Size(171, 15);
            LabelNextOccurrence.TabIndex = 2;
            LabelNextOccurrence.Text = "Next on 2026-01-01 12:00:00am";
            // 
            // ProgressToNextOccurrence
            // 
            ProgressToNextOccurrence.Location = new Point(288, 60);
            ProgressToNextOccurrence.Name = "ProgressToNextOccurrence";
            ProgressToNextOccurrence.Size = new Size(500, 23);
            ProgressToNextOccurrence.TabIndex = 3;
            // 
            // LabelProgressLastOccurrence
            // 
            LabelProgressLastOccurrence.AutoSize = true;
            LabelProgressLastOccurrence.Location = new Point(288, 86);
            LabelProgressLastOccurrence.Name = "LabelProgressLastOccurrence";
            LabelProgressLastOccurrence.Size = new Size(65, 15);
            LabelProgressLastOccurrence.TabIndex = 4;
            LabelProgressLastOccurrence.Text = "2025-01-01";
            // 
            // LabelProgressNextOccurrence
            // 
            LabelProgressNextOccurrence.AutoSize = true;
            LabelProgressNextOccurrence.Location = new Point(723, 86);
            LabelProgressNextOccurrence.Name = "LabelProgressNextOccurrence";
            LabelProgressNextOccurrence.Size = new Size(65, 15);
            LabelProgressNextOccurrence.TabIndex = 5;
            LabelProgressNextOccurrence.Text = "2026-01-01";
            // 
            // LabelMainCountdown
            // 
            LabelMainCountdown.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            LabelMainCountdown.Location = new Point(288, 126);
            LabelMainCountdown.Name = "LabelMainCountdown";
            LabelMainCountdown.Size = new Size(500, 34);
            LabelMainCountdown.TabIndex = 6;
            LabelMainCountdown.Text = "In 3 days, 2 hours, 1 minute, 0 seconds";
            LabelMainCountdown.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // TextDetails
            // 
            TextDetails.Font = new Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            TextDetails.Location = new Point(288, 163);
            TextDetails.Multiline = true;
            TextDetails.Name = "TextDetails";
            TextDetails.ReadOnly = true;
            TextDetails.Size = new Size(500, 273);
            TextDetails.TabIndex = 7;
            // 
            // TimerMain
            // 
            TimerMain.Enabled = true;
            TimerMain.Interval = 1000;
            TimerMain.Tick += TimerMain_Tick;
            // 
            // CountdownDetailsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(TextDetails);
            Controls.Add(LabelMainCountdown);
            Controls.Add(LabelProgressNextOccurrence);
            Controls.Add(LabelProgressLastOccurrence);
            Controls.Add(ProgressToNextOccurrence);
            Controls.Add(LabelNextOccurrence);
            Controls.Add(LabelCountdownName);
            Controls.Add(ListCountdowns);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "CountdownDetailsForm";
            ShowIcon = false;
            Text = "Countdown Details";
            Load += CountdownDetailsForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ListBox ListCountdowns;
        private Label LabelCountdownName;
        private Label LabelNextOccurrence;
        private ProgressBar ProgressToNextOccurrence;
        private Label LabelProgressLastOccurrence;
        private Label LabelProgressNextOccurrence;
        private Label LabelMainCountdown;
        private TextBox TextDetails;
        private System.Windows.Forms.Timer TimerMain;
    }
}