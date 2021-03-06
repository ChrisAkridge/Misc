﻿//-----------------------------------------------------------------------
// <copyright file="DataReader.cs" company="The Limitless Development Team">
//     Copyrighted unter the MIT Public License.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SomeDungeonGame.Extensions;

namespace SomeDungeonGame.IO
{
    /// <summary>
    /// Provides helper methods for reading data files in an INI-like format.
    /// </summary>
    public class DataReader
    {
        /// <summary>
        /// A string array containing the loaded file.
        /// </summary>
        private string[] file;
        
        /// <summary>
        /// The line index that the reader is currently at in the file.
        /// </summary>
        private int linePos;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataReader"/> class.
        /// </summary>
        /// <param name="filePath">The file to use.</param>
        public DataReader(string filePath)
        {
            if (File.Exists(filePath))
            {
                this.file = File.ReadAllLines(filePath);
                this.file = this.file.RemoveComments();
                this.FilePath = filePath;
                this.linePos = 0;
            }
            else
            {
                throw new FileNotFoundException(string.Format("The file at {0} does not exist.  Please check your paths.", filePath));
            }
        }

        /// <summary>
        /// Gets the path to the file.
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Returns the line in the file at the given index.
        /// </summary>
        /// <param name="index">The index of the line in the file to return.</param>
        /// <returns>The line in the file at the given index.</returns>
        public string this[int index]
        {
            get
            {
                return this.file[index];
            }
        }

        /// <summary>
        /// Reads a line in the file.
        /// </summary>
        /// <param name="index">The index of the line to read.</param>
        /// <returns>The specified line.</returns>
        /// <remarks>This method will not change the reader's index.</remarks>
        public string ReadLine(int index)
        {
            if (!(index < 0 || index > this.file.Length))
            {
                return this.file[index];
            }
            else
            {
                throw new ArgumentOutOfRangeException("index", string.Format("Index {0} is out of the range: {1} to {2}", index, this.file.GetLowerBound(0), this.file.GetUpperBound(1)));
            }
        }

        /// <summary>
        /// Reads the next line of the file and increments the index.
        /// </summary>
        /// <returns>The next line of the file.  Null if we're beyond the end.</returns>
        public string ReadNextLine()
        {
            if (this.linePos < 0)
            {
                this.linePos = 0;
            }

            if (this.linePos > this.file.GetUpperBound(0))
            {
                return null;
            }

            string result = this.file[this.linePos];
            this.linePos++;
            return result;
        }

        /// <summary>
        /// Reads the previous lines of the file.
        /// </summary>
        /// <returns>The previous line of the file, or null if the index is at the beginning.</returns>
        public string ReadPreviousLine()
        {
            if (this.linePos > this.file.GetUpperBound(0))
            {
                this.linePos = this.file.GetUpperBound(0);
            }

            if (this.linePos < 0)
            {
                return null;
            }

            string result = this.file[this.linePos];
            this.linePos--;
            return result;
        }

        /// <summary>
        /// Reads all the lines of a given section.
        /// </summary>
        /// <param name="sectionName">The name of the section.  Brackets optional.</param>
        /// <returns>The lines of the section.</returns>
        /// <remarks>This method will not change the reader's index.</remarks>
        public string[] ReadAllLinesInSection(string sectionName)
        {
            sectionName = this.CompleteSectionName(sectionName);
            if (this.SectionExists(sectionName))
            {
                List<string> result = new List<string>();
                int index = Array.IndexOf(this.file, sectionName) + 1;
                while (!this.file[index].StartsWith("[") && this.file[index].Trim() != string.Empty)
                {
                    result.Add(this.file[index]);
                    index++;
                }

                return result.ToArray();
            }
            else
            {
                throw new ArgumentException(string.Format("No section labeled {0} exists in this file.", sectionName));
            }
        }

        #region Collapsed Data Readers
        /*
         * Collapsed data example:
         * 1,string,true,46.432
         */

        /// <summary>
        /// Reads the next collapsed data entry.
        /// </summary>
        /// <returns>The next collapsed data entry, or null if
        /// there is no next entry.</returns>
        public string[] ReadNextEntry()
        {
            string entry = this.ReadNextLine();
            if (this.IsCollapsedDataEntry(entry))
            {
                return entry.Split(',');
            }

            return null;
        }

        /// <summary>
        /// Reads the previous collapsed data entry.
        /// </summary>
        /// <returns>The previous collapsed data entry, or null if
        /// there is no previous entry.</returns>
        public string[] ReadPreviousEntry()
        {
            string entry = this.ReadPreviousLine();
            if (this.IsCollapsedDataEntry(entry))
            {
                return entry.Split(',');
            }

            return null;
        }
        #endregion

        #region Full Data Readers
        /* Full data example:
         * value = 1
         * text = string
         * bool = true
         * float = 46.432
         */

        /// <summary>
        /// Reads a section containing one full data entry (i.e. settings).
        /// </summary>
        /// <param name="sectionName">The name of the section from which to read the data.</param>
        /// <returns>A dictionary of the keys and their data.</returns>
        public Dictionary<string, string> ReadFullSection(string sectionName)
        {
            sectionName = this.CompleteSectionName(sectionName);
            if (this.SectionExists(sectionName))
            {
                var result = new Dictionary<string, string>();
                int index = Array.IndexOf(this.file, sectionName) + 1;
                while (index < this.file.Length && !string.IsNullOrEmpty(this.file[index]))
                {
                    string[] entry = this.file[index].Split('=');
                    entry.TrimStringArray();
                    result.Add(entry[0], entry[1]);
                    index++;
                }

                return result;
            }

            return null;
        }

        /// <summary>
        /// Reads a section of multiple full data entries (i.e. world level tiles).
        /// </summary>
        /// <param name="sectionName">The name of the section from which to read.</param>
        /// <returns>A list of dictionaries containing keys and their data.</returns>
        public List<Dictionary<string, string>> ReadFullMultiSection(string sectionName)
        {
            sectionName = this.CompleteSectionName(sectionName);
            if (this.SectionExists(sectionName))
            {
                var result = new List<Dictionary<string, string>>();
                int index = Array.IndexOf(this.file, sectionName) + 1;
                if (index == this.file.Length)
                {
                    return null;
                }

                int listIndex = 0;
                result.Add(new Dictionary<string, string>());
                while (!(index == this.file.Length) && !this.file[index].StartsWith("["))
                {
                    if (!string.IsNullOrEmpty(this.file[index].Trim()))
                    {
                        string[] entry = this.file[index].Split('=');
                        entry.TrimStringArray();
                        result[listIndex].Add(entry[0], entry[1]);
                        index++;
                    }
                    else
                    {
                        listIndex++;
                        index++;
                        result.Add(new Dictionary<string, string>());
                    }
                }

                result.RemoveAll(item => item.Count == 0);
                return result;
            }

            return null;
        }

        /// <summary>
        /// Reads a single entry of a single full data entry.
        /// </summary>
        /// <param name="sectionName">The section of the entry.</param>
        /// <param name="key">Which key to use.</param>
        /// <returns>The data relating to the key, or null if there is no matching key or section.</returns>
        public string ReadFullEntry(string sectionName, string key)
        {
            sectionName = this.CompleteSectionName(sectionName);
            if (this.SectionExists(sectionName))
            {
                var section = this.ReadFullSection(sectionName);
                if (section.ContainsKey(key))
                {
                    return section[key];
                }
                else
                { 
                    return null; 
                }
            }

            return null;
        }

        #endregion

        /// <summary>
        /// Sets the reading index to the start of a section.
        /// </summary>
        /// <param name="sectionName">The section to move the index to.</param>
        public void SetIndexToSection(string sectionName)
        {
            sectionName = this.CompleteSectionName(sectionName);
            if (this.SectionExists(sectionName))
            {
                this.linePos = Array.IndexOf(this.file, sectionName) + 1;
            }
            else
            {
                throw new ArgumentException(string.Format("There is no section {0} in this file.", sectionName));
            }
        }

        /// <summary>
        /// Determines if a section exists.
        /// </summary>
        /// <param name="sectionName">The name of the section to check for.</param>
        /// <returns>True if the section exists, false if it doesn't.</returns>
        public bool SectionExists(string sectionName)
        {
            sectionName = this.CompleteSectionName(sectionName);
            if (Array.IndexOf(this.file, sectionName) == -1)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if a given section has any entries.
        /// </summary>
        /// <param name="sectionName">The name of the section to check.</param>
        /// <returns>True if the section if empty, false if it is not.</returns>
        public bool SectionEmpty(string sectionName)
        {
            sectionName = this.CompleteSectionName(sectionName);
            int index = Array.IndexOf(this.file, sectionName) + 1;
            if (index == this.file.Length)
            {
                return true;
            }

            int entries = 0;
            while (!this.file[index].StartsWith("["))
            {
                if (index >= this.file.Length - 1)
                {
                    continue;
                }

                index++;
                if (!string.IsNullOrEmpty(this.file[index].Trim()))
                {
                    entries++;
                }
            }

            if (entries > 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Adds square brackets to any section name that lacks them.
        /// </summary>
        /// <param name="sectionName">The original section name.</param>
        /// <returns>The section name, but with brackets.</returns>
        private string CompleteSectionName(string sectionName)
        {
            if (!(sectionName.StartsWith("[") && sectionName.EndsWith("]")))
            {
                string.Concat("[", sectionName, "]");
            }

            return sectionName;
        }

        /// <summary>
        /// Determines if a value is a collapsed data entry.
        /// Collapsed data entries contain commas but no equal signs.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>True if a comma is present (but no equal signs), false if otherwise.</returns>
        private bool IsCollapsedDataEntry(string value)
        {
            return !string.IsNullOrEmpty(value) && value.Contains(',') && !value.Contains('=');
        }

        /// <summary>
        /// Determines if a string array is a full data entry.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>True if the value is a full data entry, false if otherwise.</returns>
        private bool IsFullDataEntry(string[] value)
        {
            return value.Length == 1 && !string.IsNullOrEmpty(value[0]) && !string.IsNullOrEmpty(value[1]);
        }
    }
}
