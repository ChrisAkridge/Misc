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
            components = new System.ComponentModel.Container();
            MainPanel = new Panel();
            TimerMain = new Timer(components);
            TimerAsync = new Timer(components);
            tabControl1 = new TabControl();
            TabPageUtilities = new TabPage();
            LabelQuotient = new Label();
            NUDOneIn = new NumericUpDown();
            StaticLabelOneIn = new Label();
            ButtonRollRandomDouble = new Button();
            LabelRandomDouble = new Label();
            LabelRNGResult = new Label();
            ButtonRNGRoll = new Button();
            NUDRNGMax = new NumericUpDown();
            StaticLabelRNGMax = new Label();
            NUDRNGMin = new NumericUpDown();
            StaticLabelRNGMin = new Label();
            TabPageDisplays = new TabPage();
            tabControl1.SuspendLayout();
            TabPageUtilities.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)NUDOneIn).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NUDRNGMax).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NUDRNGMin).BeginInit();
            TabPageDisplays.SuspendLayout();
            SuspendLayout();
            // 
            // MainPanel
            // 
            MainPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            MainPanel.AutoScroll = true;
            MainPanel.BorderStyle = BorderStyle.Fixed3D;
            MainPanel.Location = new Point(6, 6);
            MainPanel.Name = "MainPanel";
            MainPanel.Size = new Size(659, 670);
            MainPanel.TabIndex = 0;
            // 
            // TimerMain
            // 
            TimerMain.Enabled = true;
            TimerMain.Interval = 1000;
            TimerMain.Tick += TimerMain_Tick;
            // 
            // TimerAsync
            // 
            TimerAsync.Enabled = true;
            TimerAsync.Interval = 1000;
            TimerAsync.Tick += TimerAsync_Tick;
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Controls.Add(TabPageUtilities);
            tabControl1.Controls.Add(TabPageDisplays);
            tabControl1.Location = new Point(12, 12);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(679, 720);
            tabControl1.TabIndex = 1;
            // 
            // TabPageUtilities
            // 
            TabPageUtilities.Controls.Add(LabelQuotient);
            TabPageUtilities.Controls.Add(NUDOneIn);
            TabPageUtilities.Controls.Add(StaticLabelOneIn);
            TabPageUtilities.Controls.Add(ButtonRollRandomDouble);
            TabPageUtilities.Controls.Add(LabelRandomDouble);
            TabPageUtilities.Controls.Add(LabelRNGResult);
            TabPageUtilities.Controls.Add(ButtonRNGRoll);
            TabPageUtilities.Controls.Add(NUDRNGMax);
            TabPageUtilities.Controls.Add(StaticLabelRNGMax);
            TabPageUtilities.Controls.Add(NUDRNGMin);
            TabPageUtilities.Controls.Add(StaticLabelRNGMin);
            TabPageUtilities.Location = new Point(4, 34);
            TabPageUtilities.Name = "TabPageUtilities";
            TabPageUtilities.Padding = new Padding(3);
            TabPageUtilities.Size = new Size(671, 682);
            TabPageUtilities.TabIndex = 0;
            TabPageUtilities.Text = "Utilities";
            TabPageUtilities.UseVisualStyleBackColor = true;
            // 
            // LabelQuotient
            // 
            LabelQuotient.AutoSize = true;
            LabelQuotient.Location = new Point(422, 66);
            LabelQuotient.Name = "LabelQuotient";
            LabelQuotient.Size = new Size(73, 25);
            LabelQuotient.TabIndex = 10;
            LabelQuotient.Text = "= 1.000";
            // 
            // NUDOneIn
            // 
            NUDOneIn.Location = new Point(333, 64);
            NUDOneIn.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            NUDOneIn.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            NUDOneIn.Name = "NUDOneIn";
            NUDOneIn.Size = new Size(83, 31);
            NUDOneIn.TabIndex = 9;
            NUDOneIn.Value = new decimal(new int[] { 1, 0, 0, 0 });
            NUDOneIn.ValueChanged += NUDOneIn_ValueChanged;
            // 
            // StaticLabelOneIn
            // 
            StaticLabelOneIn.AutoSize = true;
            StaticLabelOneIn.Location = new Point(287, 66);
            StaticLabelOneIn.Name = "StaticLabelOneIn";
            StaticLabelOneIn.Size = new Size(50, 25);
            StaticLabelOneIn.TabIndex = 8;
            StaticLabelOneIn.Text = "1-in-";
            // 
            // ButtonRollRandomDouble
            // 
            ButtonRollRandomDouble.Location = new Point(208, 61);
            ButtonRollRandomDouble.Name = "ButtonRollRandomDouble";
            ButtonRollRandomDouble.Size = new Size(62, 34);
            ButtonRollRandomDouble.TabIndex = 7;
            ButtonRollRandomDouble.Text = "Roll";
            ButtonRollRandomDouble.UseVisualStyleBackColor = true;
            ButtonRollRandomDouble.Click += ButtonRollRandomDouble_Click;
            // 
            // LabelRandomDouble
            // 
            LabelRandomDouble.AutoSize = true;
            LabelRandomDouble.Location = new Point(6, 66);
            LabelRandomDouble.Name = "LabelRandomDouble";
            LabelRandomDouble.Size = new Size(196, 25);
            LabelRandomDouble.TabIndex = 6;
            LabelRandomDouble.Text = "Random Double: 0.000";
            // 
            // LabelRNGResult
            // 
            LabelRNGResult.AutoSize = true;
            LabelRNGResult.Location = new Point(352, 13);
            LabelRNGResult.Name = "LabelRNGResult";
            LabelRNGResult.Size = new Size(78, 25);
            LabelRNGResult.TabIndex = 5;
            LabelRNGResult.Text = "Result: 0";
            // 
            // ButtonRNGRoll
            // 
            ButtonRNGRoll.Location = new Point(284, 11);
            ButtonRNGRoll.Name = "ButtonRNGRoll";
            ButtonRNGRoll.Size = new Size(62, 31);
            ButtonRNGRoll.TabIndex = 4;
            ButtonRNGRoll.Text = "Roll";
            ButtonRNGRoll.UseVisualStyleBackColor = true;
            ButtonRNGRoll.Click += ButtonRNGRoll_Click;
            // 
            // NUDRNGMax
            // 
            NUDRNGMax.Location = new Point(193, 11);
            NUDRNGMax.Maximum = new decimal(new int[] { 99999, 0, 0, 0 });
            NUDRNGMax.Name = "NUDRNGMax";
            NUDRNGMax.Size = new Size(85, 31);
            NUDRNGMax.TabIndex = 3;
            NUDRNGMax.Value = new decimal(new int[] { 100, 0, 0, 0 });
            NUDRNGMax.ValueChanged += NUDRNGMax_ValueChanged;
            // 
            // StaticLabelRNGMax
            // 
            StaticLabelRNGMax.AutoSize = true;
            StaticLabelRNGMax.Location = new Point(145, 13);
            StaticLabelRNGMax.Name = "StaticLabelRNGMax";
            StaticLabelRNGMax.Size = new Size(45, 25);
            StaticLabelRNGMax.TabIndex = 2;
            StaticLabelRNGMax.Text = "Max";
            // 
            // NUDRNGMin
            // 
            NUDRNGMin.Location = new Point(54, 11);
            NUDRNGMin.Name = "NUDRNGMin";
            NUDRNGMin.Size = new Size(85, 31);
            NUDRNGMin.TabIndex = 1;
            NUDRNGMin.Value = new decimal(new int[] { 1, 0, 0, 0 });
            NUDRNGMin.ValueChanged += NUDRNGMin_ValueChanged;
            // 
            // StaticLabelRNGMin
            // 
            StaticLabelRNGMin.AutoSize = true;
            StaticLabelRNGMin.Location = new Point(6, 13);
            StaticLabelRNGMin.Name = "StaticLabelRNGMin";
            StaticLabelRNGMin.Size = new Size(42, 25);
            StaticLabelRNGMin.TabIndex = 0;
            StaticLabelRNGMin.Text = "Min";
            // 
            // TabPageDisplays
            // 
            TabPageDisplays.Controls.Add(MainPanel);
            TabPageDisplays.Location = new Point(4, 34);
            TabPageDisplays.Name = "TabPageDisplays";
            TabPageDisplays.Padding = new Padding(3);
            TabPageDisplays.Size = new Size(671, 682);
            TabPageDisplays.TabIndex = 1;
            TabPageDisplays.Text = "Displays";
            TabPageDisplays.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(703, 744);
            Controls.Add(tabControl1);
            Name = "MainForm";
            Text = "LunaGalatea";
            tabControl1.ResumeLayout(false);
            TabPageUtilities.ResumeLayout(false);
            TabPageUtilities.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)NUDOneIn).EndInit();
            ((System.ComponentModel.ISupportInitialize)NUDRNGMax).EndInit();
            ((System.ComponentModel.ISupportInitialize)NUDRNGMin).EndInit();
            TabPageDisplays.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel MainPanel;
        private Timer TimerMain;
        private Timer TimerAsync;
        private TabControl tabControl1;
        private TabPage TabPageUtilities;
        private NumericUpDown NUDRNGMax;
        private Label StaticLabelRNGMax;
        private NumericUpDown NUDRNGMin;
        private Label StaticLabelRNGMin;
        private TabPage TabPageDisplays;
        private Label LabelRNGResult;
        private Button ButtonRNGRoll;
        private Button ButtonRollRandomDouble;
        private Label LabelRandomDouble;
        private Label LabelQuotient;
        private NumericUpDown NUDOneIn;
        private Label StaticLabelOneIn;
    }
}