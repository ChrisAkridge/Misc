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
		// 8. Begin a current board line. Write a word into the buffer. Whenever
		//    CheckCapacity returns false, add the current board line into the
		//    buffer. If you have a word longer than 16 characters and your
		//    current line is empty, 
	}
}
