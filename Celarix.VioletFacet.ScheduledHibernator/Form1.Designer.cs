namespace Celarix.VioletFacet.ScheduledHibernator
{
    partial class Form1
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
            ListViewItem listViewItem1 = new ListViewItem(new string[] { "Sunday", "12:00am" }, -1);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            label1 = new Label();
            listView1 = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            button1 = new Button();
            label2 = new Label();
            groupBox1 = new GroupBox();
            label3 = new Label();
            label4 = new Label();
            textBox1 = new TextBox();
            button2 = new Button();
            label5 = new Label();
            label6 = new Label();
            button3 = new Button();
            notifyIcon1 = new NotifyIcon(components);
            contextMenuStrip1 = new ContextMenuStrip(components);
            openOverrideToolStripMenuItem = new ToolStripMenuItem();
            timer1 = new System.Windows.Forms.Timer(components);
            label7 = new Label();
            label8 = new Label();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(159, 15);
            label1.TabIndex = 0;
            label1.Text = "Hibernation is scheduled for:";
            // 
            // listView1
            // 
            listView1.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2 });
            listView1.Items.AddRange(new ListViewItem[] { listViewItem1 });
            listView1.Location = new Point(12, 27);
            listView1.Name = "listView1";
            listView1.Size = new Size(159, 122);
            listView1.TabIndex = 1;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Weekday";
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Time";
            // 
            // button1
            // 
            button1.Enabled = false;
            button1.Location = new Point(12, 155);
            button1.Name = "button1";
            button1.Size = new Size(159, 23);
            button1.TabIndex = 2;
            button1.Text = "Modify Schedule";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 181);
            label2.Name = "label2";
            label2.Size = new Size(156, 75);
            label2.TabIndex = 3;
            label2.Text = "Schedule changes will be\r\napplied at 12:00pm the\r\nday after they are made.\r\nClosing this app will prevent\r\nchanges from being saved!";
            // 
            // groupBox1
            // 
            groupBox1.Location = new Point(177, 9);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(12, 429);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label3.Location = new Point(195, 9);
            label3.Name = "label3";
            label3.Size = new Size(394, 21);
            label3.TabIndex = 5;
            label3.Text = "Override: Type the below text exactly and click Override.";
            // 
            // label4
            // 
            label4.Location = new Point(195, 30);
            label4.Name = "label4";
            label4.Size = new Size(408, 182);
            label4.TabIndex = 6;
            label4.Text = resources.GetString("label4.Text");
            // 
            // textBox1
            // 
            textBox1.Location = new Point(195, 215);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.ScrollBars = ScrollBars.Vertical;
            textBox1.Size = new Size(408, 135);
            textBox1.TabIndex = 7;
            textBox1.KeyUp += textBox1_KeyUp;
            // 
            // button2
            // 
            button2.Location = new Point(195, 356);
            button2.Name = "button2";
            button2.Size = new Size(155, 23);
            button2.TabIndex = 8;
            button2.Text = "Try Override";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(356, 360);
            label5.Name = "label5";
            label5.Size = new Size(76, 15);
            label5.TabIndex = 9;
            label5.Text = "Keypresses: 0";
            // 
            // label6
            // 
            label6.Font = new Font("Segoe UI", 18F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label6.ForeColor = Color.Red;
            label6.Location = new Point(195, 382);
            label6.Name = "label6";
            label6.Size = new Size(410, 56);
            label6.TabIndex = 10;
            label6.Text = "System is Armed";
            label6.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // button3
            // 
            button3.Enabled = false;
            button3.Location = new Point(12, 259);
            button3.Name = "button3";
            button3.Size = new Size(159, 23);
            button3.TabIndex = 11;
            button3.Text = "Skip Tonight";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // notifyIcon1
            // 
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            notifyIcon1.Icon = (Icon)resources.GetObject("notifyIcon1.Icon");
            notifyIcon1.Text = "Scheduled Hibernator";
            notifyIcon1.Visible = true;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { openOverrideToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(152, 26);
            // 
            // openOverrideToolStripMenuItem
            // 
            openOverrideToolStripMenuItem.Name = "openOverrideToolStripMenuItem";
            openOverrideToolStripMenuItem.Size = new Size(151, 22);
            openOverrideToolStripMenuItem.Text = "Open Override";
            openOverrideToolStripMenuItem.Click += openOverrideToolStripMenuItem_Click;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 1000;
            timer1.Tick += timer1_Tick;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(12, 408);
            label7.Name = "label7";
            label7.Size = new Size(152, 15);
            label7.TabIndex = 12;
            label7.Text = "Watchdog status unknown.";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(12, 285);
            label8.Name = "label8";
            label8.Size = new Size(128, 30);
            label8.TabIndex = 13;
            label8.Text = "Next Event:\r\nHibernation in 00:00:00";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(617, 450);
            Controls.Add(label8);
            Controls.Add(label7);
            Controls.Add(button3);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(button2);
            Controls.Add(textBox1);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(groupBox1);
            Controls.Add(label2);
            Controls.Add(button1);
            Controls.Add(listView1);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Form1";
            Text = "Scheduled Hibernator";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            Resize += Form1_Resize;
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private ListView listView1;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private Button button1;
        private Label label2;
        private GroupBox groupBox1;
        private Label label3;
        private Label label4;
        private TextBox textBox1;
        private Button button2;
        private Label label5;
        private Label label6;
        private Button button3;
        private NotifyIcon notifyIcon1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem openOverrideToolStripMenuItem;
        private System.Windows.Forms.Timer timer1;
        private Label label7;
        private Label label8;
    }
}
