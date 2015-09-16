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
using Be;
using Be.Windows;
using Be.Windows.Forms;

namespace FileTools
{
	public partial class CompressorGUI : Form
	{
		private StringCompressor compressor;
		private KeyValuePair<int, string>? newDictionaryEntry = null;
		
		public CompressorGUI()
		{
			InitializeComponent();
		}

		private void TSBOpenFile_Click(object sender, EventArgs e)
		{
			if (OFDOpenFile.ShowDialog() == DialogResult.OK)
			{
				string filePath = OFDOpenFile.FileName;
				byte[] fileBytes = File.ReadAllBytes(filePath);
				string fileString = Encoding.GetEncoding(1252).GetString(fileBytes);

				compressor = new StringCompressor(fileString, 4);
				HexCurrentData.ByteProvider = new DynamicByteProvider(fileBytes);

				ListViewDictionary.Items.Clear();

				compressor.DictionaryEntryAddedEvent += Compressor_DictionaryEntryAddedEvent;
				compressor.KeyLengthExpansionOccurred += Compressor_KeyLengthExpansionOccurred;
            }
		}

		private void Compressor_KeyLengthExpansionOccurred(object sender, EventArgs e)
		{
			ListViewDictionary.Invoke(new MethodInvoker(delegate { ListViewDictionary.Items.Clear(); }));
			ListViewDictionary.Invoke(new MethodInvoker(delegate { HexCurrentData.ByteProvider = new DynamicByteProvider(Encoding.GetEncoding(1252).GetBytes(compressor.Data)); }));
		}

		private void Compressor_DictionaryEntryAddedEvent(object sender, KeyValuePair<int, string> newEntry)
		{
			newDictionaryEntry = newEntry;
		}

		private void ButtonCompressOneStep_Click(object sender, EventArgs e)
		{
			Worker.RunWorkerAsync(0);
		}

		private void ButtonCompressFull_Click(object sender, EventArgs e)
		{
			Worker.RunWorkerAsync(1);
		}

		private void Worker_DoWork(object sender, DoWorkEventArgs e)
		{
			if ((int)e.Argument == 0)
			{
				// Compress one step.
				compressor.PerformCompressionStep();
				UpdateUI();
			}
			else if ((int)e.Argument == 1)
			{
				// Compress fully.
				while (!compressor.PerformCompressionStep())
				{
					UpdateUI();
				}
			}
		}

		private void UpdateUI()
		{
			HexCurrentData.Invoke(new MethodInvoker(delegate
			{
				HexCurrentData.ByteProvider = new DynamicByteProvider(Encoding.GetEncoding(1252).GetBytes(compressor.Data));
			}));
		

			if (newDictionaryEntry != null)
			{
				int keyLength = compressor.KeyLength;
				ListViewItem item = new ListViewItem(new[] { FormatInt(newDictionaryEntry.Value.Key, keyLength), newDictionaryEntry.Value.Value, FormatStringAsHex(newDictionaryEntry.Value.Value) });
				if (ListViewDictionary.InvokeRequired)
				{
					ListViewDictionary.Invoke(new MethodInvoker(delegate
					{
						ListViewDictionary.Items.Add(item);
						ListViewDictionary.Items[ListViewDictionary.Items.Count - 1].EnsureVisible();
					}));
				}
				else
				{
					ListViewDictionary.Items.Add(item);
				}

				newDictionaryEntry = null;
			}
		}

		private string FormatInt(int value, int length)
		{
			string hexValue = $"{value:X}";
			hexValue = hexValue.PadLeft(length * 2, '0');
			return $"0x{hexValue}";
		}

		private string FormatStringAsHex(string value)
		{
			byte[] bytes = Encoding.GetEncoding(1252).GetBytes(value);
			StringBuilder resultBuilder = new StringBuilder();

			foreach (byte b in bytes)
			{
				resultBuilder.Append($"{b:X2}");
			}

			return resultBuilder.Insert(0, "0x").ToString();
		}
	}
}
