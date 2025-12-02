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

namespace Celarix.IO.CustomCodepageHexView
{
    public partial class MainForm : Form
    {
        private const int BytesPerLine = 16;
        private const string StandardHeader = "                    x0 x1 x2 x3 x4 x5 x6 x7 x8 x9 xA xB xC xD xE xF";

        private int linesPerPage;
        private ChrisAkridge.Common.Text.CustomCodepages.CodepageConverter celarianAllPrintable;

        private string? currentOpenedFilePath;
        private long? currentFileLength;
        private long? currentFileAddressOffset;

        private System.Windows.Forms.Timer? scrollDebounceTimer;
        private long pendingScrollOffset = 0;
        private const int ScrollDebounceMs = 500;

        public MainForm()
        {
            InitializeComponent();
            linesPerPage = CalculateLinesPerPage();
            celarianAllPrintable = new ChrisAkridge.Common.Text.CustomCodepages.CodepageConverter(0);

            // Initialize debounce timer for trackbar scrolling
            scrollDebounceTimer = new System.Windows.Forms.Timer();
            scrollDebounceTimer.Interval = ScrollDebounceMs;
            scrollDebounceTimer.Tick += ScrollDebounceTimer_Tick;

            // Apply scroll-on-mouse-up so we render immediately when user finishes dragging
            TrackFilePosition.MouseUp += TrackFilePosition_MouseUp;
        }

        private void ButtonOpenFile_Click(object sender, EventArgs e)
        {
            if (OFDMain.ShowDialog() == DialogResult.OK)
            {
                currentOpenedFilePath = OFDMain.FileName;
                currentFileLength = new FileInfo(currentOpenedFilePath).Length;
                currentFileAddressOffset = 0L;
                Text = $"Custom Codepage Hex Viewer - {Path.GetFileName(currentOpenedFilePath)}";
            }

            UpdateControls();
            RenderHexView();
        }

        private void UpdateControls()
        {
            if (currentOpenedFilePath == null || currentFileLength == null || currentFileAddressOffset == null)
            {
                return;
            }

            TrackFilePosition.Enabled = true;
            TrackFilePosition.Minimum = 0;
            TrackFilePosition.Maximum = currentFileLength < 1024 ? (int)currentFileLength : 1024;
            TrackFilePosition.Value = currentFileLength < 1024
                ? (int)currentFileAddressOffset
                : (int)((currentFileAddressOffset.Value * 1024) / currentFileLength.Value);

            var maximumOffsetHex = currentFileLength.Value.ToString("X");
            var hexDigits = maximumOffsetHex.Length;
            LabelFileMinimumAddress.Text = $"0x{new string('0', hexDigits)}";
            LabelFileMaximumAddress.Text = $"0x{maximumOffsetHex}";

            // Move LabelFileMaximumAddress back inside the form if it's too wide
            var labelRight = LabelFileMaximumAddress.Location.X + LabelFileMaximumAddress.Width;
            if (labelRight > ClientSize.Width)
            {
                LabelFileMaximumAddress.Location = new Point(ClientSize.Width - LabelFileMaximumAddress.Width - 10,
                    LabelFileMaximumAddress.Location.Y);
            }

            TextBoxSeekAddress.Enabled = true;
            TextBoxSeekAddress.Text = $"0x{new string('0', hexDigits)}";
            ButtonSeek.Enabled = true;
            ButtonMinus4Kilobytes.Enabled = true;
            ButtonPlus4Kilobytes.Enabled = true;
            ButtonMinus256Bytes.Enabled = true;
            ButtonPlus256Bytes.Enabled = true;
            ButtonMinus1Page.Enabled = true;
            ButtonPlus1Page.Enabled = true;
        }

        private void RenderHexView()
        {
            if (currentOpenedFilePath == null || currentFileLength == null || currentFileAddressOffset == null)
            {
                return;
            }

            try
            {
                using var fileStream = File.OpenRead(currentOpenedFilePath);
                fileStream.Seek(currentFileAddressOffset.Value, SeekOrigin.Begin);
                var buffer = new byte[BytesPerLine * linesPerPage];
                var bytesRead = fileStream.Read(buffer, 0, buffer.Length);
                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(StandardHeader);

                for (var i = 0; i < bytesRead; i += BytesPerLine)
                {
                    var address = GetAddressForRender(currentFileAddressOffset.Value + i);
                    var hexBytes = GetHexBytesForRender(buffer, i);
                    var textBytes = GetTextForRender(buffer, i);
                    stringBuilder.AppendLine($"{address} {hexBytes} {textBytes}");
                }

                TextHexView.Text = stringBuilder.ToString();
            }
            catch (Exception ex)
            {
                var messageBuilder = new StringBuilder();
                messageBuilder.AppendLine("An error occurred while rendering the hex view:");
                messageBuilder.AppendLine(ex.GetType().Name);
                messageBuilder.AppendLine(ex.Message);
                messageBuilder.AppendLine();
                messageBuilder.AppendLine("Stack Trace:");
                messageBuilder.AppendLine(ex.StackTrace ?? "No stack trace available.");
                MessageBox.Show(messageBuilder.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int CalculateLinesPerPage()
        {
            // Given the font of the hex viewer, calculate how many lines fit in one page
            using var g = TextHexView.CreateGraphics();
            var fontHeight = g.MeasureString("A", TextHexView.Font).Height;
            var visibleHeight = TextHexView.ClientSize.Height;

            // Minus 1 to account for the header line
            return (int)(visibleHeight / fontHeight) - 1;
        }

        private static string GetAddressForRender(long address)
        {
            int highPart = (int)(address >> 32);
            int lowPart = (int)(address & 0xFFFFFFFF);
            return $"0x{highPart:X8}`{lowPart:X8}";
        }

        private static string GetHexBytesForRender(byte[] buffer, int offset)
        {
            var lineBuilder = new StringBuilder();
            for (var i = offset; i < offset + BytesPerLine; i++)
            {
                if (i < buffer.Length)
                {
                    lineBuilder.AppendFormat("{0:X2} ", buffer[i]);
                }
                else
                {
                    lineBuilder.Append("   ");
                }
            }
            return lineBuilder.ToString();
        }

        private string GetTextForRender(byte[] buffer, int offset)
        {
            var selectedIndex = ComboCodepages.SelectedIndex;
            var builder = new StringBuilder();

            if (selectedIndex == 0)
            {
                // Unicode Latin-1
                for (var i = offset; i < offset + BytesPerLine; i++)
                {
                    if (i < buffer.Length)
                    {
                        var b = buffer[i];
                        var c = b switch
                        {
                            < 0x20 => '.',
                            >= 0x20 and <= 0x7E => (char)b,
                            >= 0x7F and <= 0xA0 => '.',
                            _ => (char)b
                        };
                        builder.Append(c);
                    }
                    else
                    {
                        builder.Append(' ');
                    }
                }
            }
            else if (selectedIndex == 1)
            {
                var lineBytes = new byte[BytesPerLine];
                Array.Copy(buffer, offset, lineBytes, 0, Math.Min(BytesPerLine, buffer.Length - offset));
                var convertedString = celarianAllPrintable.Convert(lineBytes);
                builder.Append(convertedString);
            }
            return builder.ToString();
        }

        private void MainForm_ResizeEnd(object sender, EventArgs e)
        {
            linesPerPage = CalculateLinesPerPage();
            RenderHexView();
        }

        private void ComboCodepages_SelectedIndexChanged(object sender, EventArgs e)
        {
            RenderHexView();
        }

        private void TextBoxSeekAddress_TextChanged(object sender, EventArgs e)
        {
            // Filter out any characters that are not valid hex digits (also no 0x prefix)
            var filteredText = new string([.. TextBoxSeekAddress.Text.Where(c =>
                (c >= '0' && c <= '9') ||
                (c >= 'a' && c <= 'f') ||
                (c >= 'A' && c <= 'F'))]);
            TextBoxSeekAddress.Text = filteredText;
        }

        private void ButtonSeek_Click(object sender, EventArgs e)
        {
            var seekAddressText = TextBoxSeekAddress.Text;
            var seekAddress = Convert.ToInt64(seekAddressText, 16);
            if (seekAddress < 0 || (currentFileLength != null && seekAddress >= currentFileLength.Value))
            {
                // Play an error sound and do nothing.
                System.Media.SystemSounds.Beep.Play();
                return;
            }
            currentFileAddressOffset = seekAddress;
            UpdateControls();
            RenderHexView();
        }

        private void ButtonPlus1Page_Click(object sender, EventArgs e)
        {
            if (currentFileLength != null)
            {
                var newOffset = currentFileAddressOffset + (BytesPerLine * linesPerPage);
                if (newOffset >= currentFileLength)
                {
                    newOffset = currentFileLength.Value - (BytesPerLine * linesPerPage);
                    if (newOffset < 0)
                    {
                        newOffset = 0;
                    }
                }
                currentFileAddressOffset = newOffset;
                UpdateControls();
                RenderHexView();
            }
        }

        private void ButtonMinus1Page_Click(object sender, EventArgs e)
        {
            if (currentFileLength != null)
            {
                var newOffset = currentFileAddressOffset - (BytesPerLine * linesPerPage);
                if (newOffset < 0)
                {
                    newOffset = 0;
                }
                currentFileAddressOffset = newOffset;
                UpdateControls();
                RenderHexView();
            }
        }

        private void ButtonMinus256Bytes_Click(object sender, EventArgs e)
        {
            if (currentFileLength != null)
            {
                var newOffset = currentFileAddressOffset - 256;
                if (newOffset < 0)
                {
                    newOffset = 0;
                }
                currentFileAddressOffset = newOffset;
                UpdateControls();
                RenderHexView();
                RenderHexView();
            }
        }

        private void ButtonPlus256Bytes_Click(object sender, EventArgs e)
        {
            if (currentFileLength != null)
            {
                var newOffset = currentFileAddressOffset + 256;
                if (newOffset >= currentFileLength)
                {
                    newOffset = currentFileLength.Value - 1;
                }
                currentFileAddressOffset = newOffset;
                UpdateControls();
                RenderHexView();
            }
        }

        private void ButtonMinus4Kilobytes_Click(object sender, EventArgs e)
        {
            if (currentFileLength != null)
            {
                var newOffset = currentFileAddressOffset - 4096;
                if (newOffset < 0)
                {
                    newOffset = 0;
                }
                currentFileAddressOffset = newOffset;
                UpdateControls();
                RenderHexView();
            }
        }

        private void ButtonPlus4Kilobytes_Click(object sender, EventArgs e)
        {
            if (currentFileLength != null)
            {
                var newOffset = currentFileAddressOffset + 4096;
                if (newOffset >= currentFileLength)
                {
                    newOffset = currentFileLength.Value - 1;
                }
                currentFileAddressOffset = newOffset;
                UpdateControls();
                RenderHexView();
            }
        }

        private void TrackFilePosition_Scroll(object sender, EventArgs e)
        {
            if (currentFileLength == null)
            {
                return;
            }

            // Calculate the offset but don't apply it immediately to avoid frequent file reads.
            var newOffset = (long)((TrackFilePosition.Value / 1024.0) * currentFileLength.Value);
            pendingScrollOffset = newOffset;

            // Restart debounce timer: wait for ScrollDebounceMs of inactivity before applying.
            if (scrollDebounceTimer != null)
            {
                scrollDebounceTimer.Stop();
                scrollDebounceTimer.Start();
            }
        }

        // Timer tick applies the pending offset after debounce interval.
        private void ScrollDebounceTimer_Tick(object? sender, EventArgs e)
        {
            if (scrollDebounceTimer != null)
            {
                scrollDebounceTimer.Stop();
            }

            // If nothing changed, there's nothing to do.
            if (currentFileAddressOffset == pendingScrollOffset)
            {
                return;
            }

            currentFileAddressOffset = pendingScrollOffset;
            UpdateControls();
            RenderHexView();
        }

        // When the user releases the mouse button after dragging the trackbar, apply immediately.
        private void TrackFilePosition_MouseUp(object? sender, MouseEventArgs e)
        {
            if (scrollDebounceTimer != null)
            {
                scrollDebounceTimer.Stop();
            }

            if (currentFileLength == null)
            {
                return;
            }

            // Ensure pendingScrollOffset is up-to-date in case MouseUp occurs without a recent Scroll event.
            var newOffset = (long)((TrackFilePosition.Value / 1024.0) * currentFileLength.Value);
            pendingScrollOffset = newOffset;

            if (currentFileAddressOffset != pendingScrollOffset)
            {
                currentFileAddressOffset = pendingScrollOffset;
                UpdateControls();
                RenderHexView();
            }
        }
    }
}
