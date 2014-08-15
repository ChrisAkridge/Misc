using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuickTag.Data;

namespace QuickTag
{
	public partial class StartForm : Form
	{
		public StartForm()
		{
			InitializeComponent();
		}

		private void ButtonCreateDatabase_Click(object sender, EventArgs e)
		{
			if (this.SFDCreateDatabase.ShowDialog() == DialogResult.OK)
			{
				string dbPath = this.SFDCreateDatabase.FileName;
				Database database = Database.Empty;
				File.WriteAllText(dbPath, database.Serialize());
				this.OpenDatabase(dbPath);
			}
		}

		private void ButtonOpenExistingDatabase_Click(object sender, EventArgs e)
		{
			if (this.OFDOpenDatabase.ShowDialog() == DialogResult.OK)
			{
				string dbPath = this.OFDOpenDatabase.FileName;
				this.OpenDatabase(dbPath);
			}
		}

		private void OpenDatabase(string dbPath)
		{
			if (!File.Exists(dbPath))
			{
				MessageBox.Show(string.Format("The database at the path below does not exist:/r/n/r/n{0}", dbPath), "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			MainForm mainForm = new MainForm(this);
			mainForm.Initialize(dbPath);
			mainForm.Show();
			this.Hide();
		}
	}
}
