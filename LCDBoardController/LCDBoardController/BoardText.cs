using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCDBoardController
{
	/// <summary>
	/// Represents a block of text 16 characters wide and 2 characters high.
	/// </summary>
	internal sealed class BoardText
	{
		// The board supports the ASCII character set.
		// Bytes from 0x20 to 0x7F behave as you'd expect.
		// The byte 0xFE is the command prefix and is used
		// to set various board settings like position,
		// color, contrast, autoscroll, and so forth.
		//
		// Characters from 0x00 to 0x07 are not control
		// characters; they are custom characters that
		// the user can program.
	}
}
