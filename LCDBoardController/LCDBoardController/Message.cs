using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CharacterManager;

namespace LCDBoardController
{
	public sealed class Message
	{
		private sealed class BoardLine
		{
			public const string BlankLine = "                "; // 16 spaces

			private string[] characters = new string[16];
			private int position = 0;

			public int Length => position;

			public bool CheckCapacity(int lengthToAdd)
			{
				return position + lengthToAdd < 15;
			}

			public void Write(char asciiChar)
			{
				if (asciiChar > 0x7f) { throw new ArgumentOutOfRangeException(); }
				if (!CheckCapacity(1)) { throw new System.IO.InternalBufferOverflowException(); }

				characters[position] = asciiChar.ToString();
				position++;
			}

			public void Write(string word)
			{
				if (!CheckCapacity(word.Length)) { throw new System.IO.InternalBufferOverflowException(); }

				foreach (char c in word)
				{
					Write(c);
				}
			}

			public void WriteCustomCharacter(string word)
			{
				if (!CheckCapacity(1)) { throw new System.IO.InternalBufferOverflowException(); }

				characters[position] = word;
				position++;
			}

			public override string ToString()
			{
				return string.Concat(characters);
			}
		}

		private CharacterBank bank;
		private string input;

		// How to parse a message:
		// 1. Receive a string of standard text. Custom characters are represented
		//    through the sequence \{charname}\. Separate \ and {} characters are
		//    printed as is.
		// 2. Every \{ must have a matching }\. No nesting of \{ and }\ is allowed.
		// 3. Create a list of every custom character to be used. A message can
		//    consist of more than eight characters so long as there are only eight
		//    custom characters in every 32 (-ish, because of justification).
		// 4. Create a list of board lines. We'll add completed lines as we go along.
		// 5. Each line is constructed using 16 characters represented as strings.
		//    ASCII characters are represented with a string with length of 1, just
		//    containing the character. Custom characters are represented with the
		//    sequence \{charname}\ and will always have a length larger than 1.
		// 6. Justification of each line is fairly trivial, and not very good, but
		//    workable. We'll just take words, split by spaces, until the next word
		//    won't fit on the line. Words of length longer than 16 will be written
		//    if the line is blank.
		// 7. Split the input into lines. Convert all tabs to one space, and then
		//    split each line on spaces into a list of words.
		// 8. The parser is something of a state machine. We take characters from
		//	  the input, one by one, and add them to the current board line. Once
		//	  the line is full, we add it to the list of lines and start with a
		//	  new board line.
		// 9. Custom character sequences begin with \{. As such, when we come
		//	  across a \ character, we need to check if the next character (if
		//	  there is one) is a {, and if so, search for the next matching }\
		//	  Having no }\ to match a \{, or having a stray }\ when we're not
		//	  in a custom character block means the input is malformed.
		//	  Malformed input is ignored, passed raw to the board lines. It
		//	  will look wrong, but at least the message persists.

		// Sending characters to the board:
		// 1. Characters between U+0020 and U+007F will be sent as is.
		// 2. Characters in the range U+0000-U+001F and anything higher than
		//	  U+007F will be converted to '?' U+003F.
		// 3. Custom characters (in the form of \{charname}\) will be converted
		//	  to a byte between 0x00 and 0x07 inclusive. Messages will allow
		//	  more than eight custom characters so long as no more than eight
		//	  custom characters appear in two lines (32 characters) of text.
		//	  Between sending of each board line will be a character load
		//	  directive that will load new custom characters into unused slots.
		//	  To generate a directive:
		//		1. Count the unique custom characters on the next board line.
		//		   If there are more than eight, replace all custom characters
		//		   with '?' U+003F. If there are eight or less, continue to 2.
		//		2. Count the unique custom characters on the last two lines.
		//		   If there the sum of old and new custom characters is greater
		//		   than 8, replace ALL new custom characters with '?' U+003F.
		//		3. Otherwise, given a list of which slots are assigned which
		//		   custom characters, load new characters into unused slots.
		//	  Before sending a board line, send the directive to the board as a
		//	  set of custom character creation commands. Generate a byte
		//	  sequence for the board line to send with custom characters
		//	  replaced with their slot numbers.

		private const char InvalidSubstitute = '?';
		private const string CustomCharStart = "\\{";
		private const string CustomCharEnd = "}\\";

		private bool inputParsed = false;
		private CustomCharacter[] loadedCustomCharacters = new CustomCharacter[8];
		private List<BoardLine> boardLines = new List<BoardLine>();
		private int lastSentBoardLineIndex = -1;
		private bool MessageComplete => lastSentBoardLineIndex == boardLines.Count - 1;

		public Message(string input, CharacterBank bank)
		{
			if (string.IsNullOrEmpty(input))
			{
				inputParsed = true;

				// Send two blank lines to clear the board.
				BoardLine blankLine = new BoardLine();
				blankLine.Write(BoardLine.BlankLine);
				boardLines.Add(blankLine);
				boardLines.Add(blankLine);
			}

			if (bank == null) { throw new ArgumentNullException(nameof(bank), "The provided character bank was null."); }

			this.input = input;
			this.bank = bank;

			ParseInput();
		}

		private void ParseInput()
		{
			if (inputParsed) { return; }

			BoardLine currentLine = new BoardLine();

			input = ReplaceOutOfRangeCharacters(input);
			List<string> splitInput = SplitInput(input);

			foreach (string block in splitInput)
			{
				if (block.StartsWith(CustomCharStart))
				{
					if (!currentLine.CheckCapacity(1))
					{
						// Make a new board line
					}
					else
					{
						// Write to board line
					}
				}
				else
				{
					// Split into words
					// Foreach word, check capacity, clear if needed, write
				}
			}
		}

		private static string ReplaceOutOfRangeCharacters(string input)
		{
			StringBuilder resultBuilder = new StringBuilder(input.Length);

			foreach (char c in input)
			{
				if (c >= 0x20 && c <= 0x7f) { resultBuilder.Append(c); }
				else { resultBuilder.Append(InvalidSubstitute); }
			}

			return resultBuilder.ToString();
		}

		private static List<string> SplitInput(string input)
		{
			// Splits the input into two kinds of strings: standard characters
			// and custom characters.

			StringBuilder currentBuilder = new StringBuilder();
			List<string> result = new List<string>();

			for (int i = 0; i < input.Length; i++)
			{
				char current = input[i];
				char next = (i + 1 < input.Length) ? input[i + 1] : '\0';
				
				if (current == '\\')
				{
					if (next == '{')
					{
						int indexOfEnd = input.IndexOf(CustomCharEnd, i);
						if (indexOfEnd > 0)
						{
							result.Add(currentBuilder.ToString());
							currentBuilder = new StringBuilder();
							currentBuilder.Append(input.Substring(i, (indexOfEnd + 2 - i)));
							result.Add(currentBuilder.ToString());
							currentBuilder = new StringBuilder();
						}
					}
				}
				else { currentBuilder.Append(current); }
			}

			return result;
		}
	}
}
