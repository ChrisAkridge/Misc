using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCDBoardController
{
    public class LCDBoard : IDisposable
    {
		private const byte InitialRedColor = 0x00;
		private const byte InitialGreenColor = 0x80;
		private const byte InitialBlueColor = 0xFF;
		private const byte InitialBrightnessLevel = 0xC0;
		private const byte InitialContrastLevel = 0xC0;

		/// <summary>
		/// The serial port connecting to the LCD board.
		/// </summary>
		private SerialPort board;

		public bool AutoscrollEnabled { get; private set; }

		public bool BoardIsOn { get; private set; }

		/// <summary>
		/// A 16x2 array containing the characters in each position on the board.
		/// </summary>
		private byte[,] boardState;

		/// <summary>
		/// The zero-based position of the cursor on its row (X-position).
		/// </summary>
		private int cursorX;

		/// <summary>
		/// The zero-based position of the cursor on its column (Y-position).
		/// </summary>
		private int cursorY;

		/// <summary>
		/// The color of the backlight.
		/// </summary>
		private Color backlightColor;

		/// <summary>
		/// The brightness of the backlight.
		/// </summary>
		private byte brightness;

		/// <summary>
		/// The contrast of the board.
		/// </summary>
		private byte contrast;
		
		public LCDBoard()
		{
			// Find the LCD board on its COM port.
			// For now, we'll just pick the first COM port we find.
			// We'll fix that later.

			string portName = SerialPort.GetPortNames().FirstOrDefault();
			if (portName == null) { throw new ArgumentNullException(nameof(portName), "Are you sure the board is connected?"); }

			board = new SerialPort(portName);
			board.Open();

			BoardIsOn = true;

			cursorX = 0;
			cursorY = 0;

			boardState = new byte[16, 2];

			// The only way to know the backlight color, brightness, and contrast is to set it ourselves.
			SetBacklightColor(new Color(InitialRedColor, InitialGreenColor, InitialBlueColor));
			SetBrightness(InitialBrightnessLevel);
			SetContrast(InitialContrastLevel);
		}

		private void WriteToBoardState(byte[] bytes)
		{
			if (bytes == null || bytes.Length == 0) return;

			int positionInMessage = 0;
			while (positionInMessage < bytes.Length)
			{
				if (!AutoscrollEnabled)
				{
					if (cursorY == 1 && cursorX == 15) // end of board
					{
						cursorX = cursorY = 0;
					}
					else if (cursorX == 15) // end of row
					{
						cursorX = 0;
						cursorY++;
					}
				}
				else
				{
					if (cursorY == 1 && cursorX == 15) // end of board
					{
						// Assign the bytes of the second row to the first. Clear the second row bytes as we go.
						for (int i = 0; i < 15; i++)
						{
							boardState[i, 0] = boardState[i, 1];
							boardState[i, 1] = 0x00;
						}

						cursorX = 0; // As we're autoscrolling, whenever we hit the bottom row, we'll always keep writing to it as we go.
					}
					else if (cursorY == 0 && cursorX == 15) // end of row
					{
						// Move to the next row
						cursorX = 0;
						cursorY = 1;
					}
				}

				boardState[cursorX, cursorY] = bytes[positionInMessage];
				cursorX++;

				positionInMessage++;
			}
		}

		public string GetBoardState()
		{
			StringBuilder result = new StringBuilder();
			result.Append("Board state: [ ");

			for (int x = 0; x < 15; x++)
			{
				result.Append($"0x{boardState[x, 0]:X2}");
			}
			result.Append("]\r\n             [");
			
			for (int x = 0; x < 15; x++)
			{
				result.Append($"0x{boardState[x, 1]:X2}");
			}
			result.Append("]\r\n");

			result.Append("[");
			for (int x = 0; x < 15; x++)
			{
				result.Append((char)boardState[x, 0]);
			}
			result.Append("]");

			result.Append("[");
			for (int x = 0; x < 15; x++)
			{
				result.Append((char)boardState[x, 1]);
			}
			result.Append("]\r\n");

			return result.ToString();
		}

		public void Write(string message)
		{
			byte[] messageBytes = Encoding.ASCII.GetBytes(message);

			board.Write(messageBytes, 0, messageBytes.Length); // TODO: replace this and all other calls to board.Write with a call to Send(byte[]).
			WriteToBoardState(messageBytes);
		}

		public void Send(byte[] bytes)
		{
			board.Write(bytes, 0, bytes.Length);
		}

		public void TurnOn()
		{
			// Command: 0xFE 0x42
			byte[] commandBytes = new byte[] { 0xFE, 0x42 };
			Send(commandBytes);
			BoardIsOn = true;
		}

		public void TurnOff()
		{
			// Command: 0xFE 0x46
			byte[] commandBytes = new byte[] { 0xFE, 0x46 };
			Send(commandBytes);
			BoardIsOn = false;
		}

		public void ClearBoard()
		{
			// Command: 0xFE 0x58
			byte[] commandBytes = new byte[] { 0xFE, 0x58 };
			board.Write(commandBytes, 0, 2);

			for (int y = 0; y < 2; y++)
			{
				for (int x = 0; x < 16; x++)
				{
					boardState[x, y] = 0x00;
				}
			}

			cursorX = 0;
			cursorY = 0;
		}
		
		public void SetBacklightColor(Color color)
		{
			// Command: 0xFE 0xD0 0x{red} 0x{green} 0x{blue}; 5 bytes
			byte[] command = new byte[] { 0xFE, 0xD0, color.Red, color.Green, color.Blue };

			board.Write(command, 0, 5);
			backlightColor = color;
		}

		public void SetBrightness(byte level)
		{
			// Command: 0xFE 0x98 0x{level}; 3 bytes
			byte[] command = new byte[] { 0xFE, 0x98, level };

			board.Write(command, 0, 3);
			brightness = level;
		}

		public void SetContrast(byte level)
		{
			// Command: 0xFE 0x91 0x{level}; 3 bytes
			byte[] command = new byte[] { 0xFE, 0x91, level };

			board.Write(command, 0, 3);
			contrast = level;
		}

		public void EnableAutoScroll()
		{
			// Command: 0xFE 0x51
			byte[] commandBytes = new byte[] { 0xFE, 0x51 };
			Send(commandBytes);
			AutoscrollEnabled = true;
		}

		public void DisableAutoScroll()
		{
			// Command: 0xFE 0x52
			byte[] commandBytes = new byte[] { 0xFE, 0x52 };
			Send(commandBytes);
			AutoscrollEnabled = false;
		}

		public void SetCursorPosition(int x, int y)
		{
			if ((x < 1 || x > 16) || (y < 1 || y > 2))
			{
				throw new ArgumentOutOfRangeException(nameof(x), "The cursor must be set to somewhere between 1, 1 and 16, 2.");
			}

			// Command: 0xFE, 0x47, 0x(x-pos), 0x(y-pos)
			byte[] commandBytes = new byte[] {0xFE, 0x47, (byte)x, (byte)y};
			Send(commandBytes);
			cursorX = x - 1;
			cursorY = y - 1;
		}

		public void Dispose()
		{
			board.Close();
			board.Dispose();
		}
	}
}
