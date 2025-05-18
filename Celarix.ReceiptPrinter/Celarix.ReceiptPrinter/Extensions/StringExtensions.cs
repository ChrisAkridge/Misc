using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.ReceiptPrinter.Extensions
{
    internal static class StringExtensions
    {
        public static IReadOnlyList<string> WrapToMaxLength(this string str, int maxLength)
        {
            if (string.IsNullOrEmpty(str))
            {
                return [];
            }

            str = str
                .Replace("\r\n", "\n")
                .Replace("\t", "  ")
                .ReplaceNonASCIIPrintableCharacters();
            var outputLines = new List<string>();
            var outputLineBuilder = new StringBuilder();

            // We're going to do this like we're imitating a person actually typing this out.
            foreach (char c in str)
            {
                if (c == '\n')
                {
                    // Newline character, add the current line to the output and reset the line builder
                    outputLines.Add(outputLineBuilder.ToString());
                    outputLineBuilder.Clear();
                    continue;
                }
                else if (char.IsWhiteSpace(c))
                {
                    if (outputLineBuilder.Length < maxLength)
                    {
                        // Type the space if we have room for it, but just ignore it if we don't.
                        outputLineBuilder.Append(c);
                    }
                    else if (outputLineBuilder[outputLineBuilder.Length - 1] != ' ')
                    {
                        // If we have a word ending right at the end of the line, we can move to the next line.
                        outputLines.Add(outputLineBuilder.ToString());
                        outputLineBuilder.Clear();
                    }
                }
                else
                {
                    // Do we have room for this character?
                    if (outputLineBuilder.Length < maxLength)
                    {
                        outputLineBuilder.Append(c);
                    }
                    else
                    {
                        // No room. We want to "backspace" the word we're in and append it to the next line.
                        if (outputLineBuilder.All(c => !char.IsWhiteSpace(c)))
                        {
                            // Unless the entire line is a word! Then we can't wrap, so we just want to break it
                            // and add a dash at the end of the line.
                            var lastCharacter = outputLineBuilder[outputLineBuilder.Length - 1];
                            outputLineBuilder.Remove(outputLineBuilder.Length - 1, 1);
                            outputLineBuilder.Append('-');
                            outputLines.Add(outputLineBuilder.ToString());
                            outputLineBuilder.Clear();
                            outputLineBuilder.Append(lastCharacter);
                            continue;
                        }

                        var i = outputLineBuilder.Length - 1;
                        for (; i >= 0; i--)
                        {
                            if (char.IsWhiteSpace(outputLineBuilder[i]))
                            {
                                // Found the space, we can break here.
                                break;
                            }
                        }
                        // Set i to the index of the start of the word, currently it's at the end of the space.
                        i += 1;
                        var lastWord = outputLineBuilder.ToString(i, outputLineBuilder.Length - i);
                        outputLineBuilder.Remove(i, outputLineBuilder.Length - i);
                        outputLines.Add(outputLineBuilder.ToString());
                        outputLineBuilder.Clear();
                        outputLineBuilder.Append(lastWord);
                        // Now we can add the character to the line.
                        outputLineBuilder.Append(c);
                    }
                }
            }

            // Add the last line if it has any content.
            if (outputLineBuilder.Length > 0)
            {
                outputLines.Add(outputLineBuilder.ToString());
            }
            return outputLines;
        }

        public static string ReplaceNonASCIIPrintableCharacters(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            var sb = new StringBuilder(str.Length);
            foreach (char c in str)
            {
                if ((c >= 32 && c <= 126) || c == '\r' || c == '\n') // ASCII printable range
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append('?'); // Replace non-printable characters with '?'
                }
            }
            return sb.ToString();
        }

        public static string CenterText(this string text, int width)
        {
            // Center a single-line string inside a string padded on both left and right
            // i.e. "Hello" in a 20-character wide string would be "       Hello       "
            if (string.IsNullOrEmpty(text))
            {
                return new string(' ', width);
            }
            if (width <= 0)
            {
                return string.Empty;
            }
            if (text.Length >= width)
            {
                return text.Substring(0, width);
            }
            int padding = (width - text.Length) / 2;
            string leftPadding = new string(' ', padding);
            string rightPadding = new string(' ', width - text.Length - padding);
            return leftPadding + text + rightPadding;
        }
    }
}
