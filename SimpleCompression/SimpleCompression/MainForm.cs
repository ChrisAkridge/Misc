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

namespace SimpleCompression
{
	public partial class MainForm : Form
	{
		private BinaryRLECompressor rleCompressor;
		public MainForm()
		{
			InitializeComponent();
		}

		private void TSBOpenFile_Click(object sender, EventArgs e)
		{
			if (OFDOpenFile.ShowDialog() == DialogResult.OK)
			{
				string filePath = OFDOpenFile.FileName;
				byte[] fileBytes = File.ReadAllBytes(filePath);

				if (RBTextFile.Checked)
				{
					TextBoxFileContents.Text = File.ReadAllText(filePath);
				}
				else if (RBBinaryFile.Checked)
				{
					TextBoxFileContents.Text = fileBytes.ToHexString();
				}
				else if (RadioDontDisplay.Checked)
				{

				}

				rleCompressor = new BinaryRLECompressor(fileBytes);

				LabelStatus.Text = $"File loaded. Size: {fileBytes.Length}.";
			}
		}

		private void TSBCompressOneStep_Click(object sender, EventArgs e)
		{
			switch (rleCompressor.Stage)
			{
				case BinaryRLECompressionStage.Input:
					rleCompressor.NextCompressionStep();
					TextCompressionResult.Text = rleCompressor.BinaryString;
					LabelStatus.Text = $"Converted to binary. Size: {rleCompressor.InputFile.Length}. Bits: {TextCompressionResult.Text.Length}.";
					break;
				case BinaryRLECompressionStage.ConvertToBinary:
					rleCompressor.NextCompressionStep();
					TextCompressionResult.Text = rleCompressor.RunsString;
					LabelStatus.Text = $"Compressed to binary runs. Total Runs: {rleCompressor.Runs.Count}";
					break;
				case BinaryRLECompressionStage.BinaryRunSequences:
					rleCompressor.NextCompressionStep();
					TextCompressionResult.Text = rleCompressor.Compressed.ToHexString();
					float compressionAmount = ((float)rleCompressor.Compressed.Length / (float)rleCompressor.InputFile.Length) * 100f;
					LabelStatus.Text = $"Compressed. Original Size: {rleCompressor.InputFile.Length}. Compressed Size: {rleCompressor.Compressed.Length}. Compression Amount: {compressionAmount:F4}%";
					break;
				case BinaryRLECompressionStage.RLEEncodedBytes:
					break;
				default:
					break;
			}
		}

		private void ButtonConvert_Click(object sender, EventArgs e)
		{
			string binary = this.TextCompressionResult.Text;
			StringBuilder result = new StringBuilder();
			int position = 0;

			foreach (char c in binary)
			{
				if (position == 1440)
				{
					result.Append("\r\n\r\n");
					position = 0;
				}

				if (c == '0')
				{
					result.Append('A');
				}
				else
				{
					result.Append('B');
				}

				position++;
			}

			TextCompressionResult.Text = result.ToString();
		}

		private void ButtonQuickCompress_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < 3; i++)
			{
				rleCompressor.NextCompressionStep();
			}

			SaveFileDialog sfd = new SaveFileDialog();
			if (sfd.ShowDialog() == DialogResult.OK)
			{
				string filePath = sfd.FileName;
				File.WriteAllBytes(filePath, rleCompressor.Compressed);
			}
		}
	}
}
