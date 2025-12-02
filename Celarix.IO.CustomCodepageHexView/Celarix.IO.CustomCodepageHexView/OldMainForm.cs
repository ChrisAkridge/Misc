using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using ChrisAkridge.Common.Text.CustomCodepages;
using WpfHexaEditor;
using WpfHexaEditor.Core;
using WpfHexaEditor.Core.CharacterTable;
using FlowDirection = System.Windows.FlowDirection;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using StreamWriter = System.IO.StreamWriter;

namespace Celarix.IO.CustomCodepageHexView
{
    public partial class OldMainForm : Form
    {
        private readonly string[] customCodepageNames;
        private readonly string[] customCodepageTBLFilePaths;
        
        public OldMainForm()
        {
            InitializeComponent();

            var converter = new ChrisAkridge.Common.Text.CustomCodepages.CodepageConverter(0);
            customCodepageNames = new[]
            {
                converter.CodepageName
            };
            customCodepageTBLFilePaths = new[]
            {
                CreateCodepageTBLFile(converter)
            };
        }

        private static string CreateCodepageTBLFile(CodepageConverter converter)
        {
            var tblFilePath = Path.GetTempFileName();
            using var fileStream = new StreamWriter(File.OpenWrite(tblFilePath));
            for (int i = 0; i < 256; i++)
            {
                fileStream.WriteLine($"{i:X2}={converter.Convert(new[] { (byte)i })}");
            }
            fileStream.Close();
            return tblFilePath;
        }
        
        private void MainForm_Load(object sender, EventArgs e)
        {
            HexMain.ReadOnlyMode = true;
            
            // Cheat here because HexEditor doesn't actually take Unicode TBLs
            var tblFile = File.ReadAllText(customCodepageTBLFilePaths[0], Encoding.UTF8);
            HexMain.LoadTblFile(customCodepageTBLFilePaths[0]);
            var hexEditorType = typeof(HexEditor);
            var tblCharacterTableField =
                hexEditorType.GetField("_tblCharacterTable", BindingFlags.Instance | BindingFlags.NonPublic);
            var unicodeTbl = new TblStream(customCodepageTBLFilePaths[0]);
            unicodeTbl.Load(tblFile);
            tblCharacterTableField.SetValue(HexMain, unicodeTbl);

            var updateTblBookMarkMethod =
                hexEditorType.GetMethod("UpdateTblBookMark", BindingFlags.Instance | BindingFlags.NonPublic);
            updateTblBookMarkMethod.Invoke(HexMain, null);

            var maxVisibleLineProperty =
                hexEditorType.GetProperty("MaxVisibleLine", BindingFlags.Instance | BindingFlags.NonPublic);
            var buildDataLinesMethod =
                hexEditorType.GetMethod("BuildDataLines", BindingFlags.Instance | BindingFlags.NonPublic);
            buildDataLinesMethod.Invoke(HexMain, new[]
            {
                maxVisibleLineProperty.GetValue(HexMain),
                true
            });
            HexMain.RefreshView(true);
        }

        private void TSBOpen_Click(object sender, EventArgs e)
        {
            if (OFDMain.ShowDialog() == DialogResult.OK)
            {
                HexMain.FileName = OFDMain.FileName;
            }
        }
    }
}
