#region LICENSE

// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Project owner: 
//		Email: edwardnutting@outlook.com
//		For paper mail address, please contact via email for details.
//
// ------------------------------------------------------------------------------ //

#endregion

using Kernel.Consoles;
using Kernel.Devices.Timers;
using Kernel.Framework;
using Kernel.Framework.Collections;
using Kernel.Framework.Stubs;

namespace Kernel
{
    /// <summary>
    ///     Represents a virtual Console input/output display. Also provides static methods for initialising the default
    ///     console.
    /// </summary>
    public abstract class Console : Object
    {
        /// <summary>
        ///     The default console for the core kernel.
        /// </summary>
        public static Console Default;

        // Features:
        //  - Writing/reading
        //  - Scrolling
        //  - Foreground/Background Colour
        //  - Beep
        //  - Cursor

        /// <summary>
        ///     The buffer of lines of text.
        /// </summary>
        protected List Buffer = new List(100);

        /// <summary>
        ///     The current character attribute.
        /// </summary>
        protected ushort CurrentAttr;

        /// <summary>
        ///     The current character in the current line where the cursor is.
        /// </summary>
        protected int CurrentChar;

        /// <summary>
        ///     The current line where the cursor is in the buffer.
        /// </summary>
        protected int CurrentLine;

        /// <summary>
        ///     The maximum length of a line.
        /// </summary>
        public int LineLength = 80;

        /// <summary>
        ///     The maximum size of the buffer (in lines).
        /// </summary>
        protected int MaxBufferSize = 200;

        /// <summary>
        ///     Screen-relative height of the console (in number of lines).
        /// </summary>
        public int ScreenHeight = 25;

        /// <summary>
        ///     Screen-relative width of the console (in number of characters).
        /// </summary>
        public int ScreenLineWidth = 80;

        /// <summary>
        ///     Screen-relative 0-based index of the line the console starts on.
        /// </summary>
        public int ScreenStartLine = 0;

        /// <summary>
        ///     Screen-relative 0-based index of the character in a line the console starts on.
        /// </summary>
        public int ScreenStartLineOffset = 0;

        /// <summary>
        ///     Whether to update the screen's cursor for this console or not.
        /// </summary>
        /// <remarks>
        ///     Should be set to true for at most one instance of a given type of console.
        /// </remarks>
        public bool UpdateScreenCursor = true;

        /// <summary>
        ///     Initialises a new instance of a Console.
        /// </summary>
        public Console(int ScreenLineWidth, int ScreenHeight)
        {
            this.ScreenLineWidth = ScreenLineWidth;
            this.ScreenHeight = ScreenHeight;

            Buffer.Add(CreateBlankLine());
            DefaultColour();
        }

        /// <summary>
        ///     Creates a blank line (a line filled with spaces set with the current attribute).
        /// </summary>
        /// <returns>The new line.</returns>
        protected String CreateBlankLine()
        {
            //Create a blank line (all characters set to 0s)
            String str = String.New(LineLength);

            //Set the attribute of all characters in the new blank line to
            //  the current character.
            //This is so that typed characters at least appear in current 
            //  colour otherwise they wouldn't show at all.
            for (int i = 0; i < str.Length; i++)
            {
                str[i] |= (char)CurrentAttr;
            }

            //Return the new blank line.
            return str;
        }

        /// <summary>
        ///     When overridden in a derived class, updates the display output.
        /// </summary>
        public abstract void Update();

        /// <summary>
        ///     When overridden in a derived class, gets the offset from the current line to the actual line position of
        ///     the cursor.
        /// </summary>
        /// <returns>The offset to be subtracted from the current line.</returns>
        protected abstract int GetDisplayOffset_Line();

        /// <summary>
        ///     When overridden in a derived class, gets the offset from the current character to the actual character
        ///     position of the cursor.
        /// </summary>
        /// <returns>The offset to be subtracted from the current character.</returns>
        protected abstract int GetDisplayOffset_Char();

        /// <summary>
        ///     Writes the specified text to the output. Overflows to the next line if necessary.
        ///     "\n" characters will result in a new line being started.
        /// </summary>
        /// <param name="str">The string to write.</param>
        public virtual void Write(String str)
        {
            //Loop through each character, outputting them.
            for (int i = 0; i < str.Length; i++)
            {
                //If we have reached the end of the current line,
                //  create a new one using WriteLine()
                if (CurrentChar == LineLength)
                {
                    WriteLine();
                }

                // This take cares if the text has CRLF line termination instead of expected LF
                if (str[i] == '\r') // Checks for Carriage return (\r)
                {
                    if (str[i + 1] == '\n') // Checks that the next character is a Line feed
                    {
                        i++;
                    }
                }
                //If a \n (newline) character is found,
                //  create a new line using WriteLine()
                if (str[i] == '\n')
                {
                    WriteLine();
                }
                //Else if \b (backspace) character is found
                //  delete a character or line
                else if (str[i] == '\b')
                {
                    //If at very start of a line
                    if (CurrentChar == 0)
                    {
                        //Delete a line
                        Buffer.RemoveAt(CurrentLine);
                        CurrentLine--;
                        CurrentChar = LineLength;
                    }

                    //Check there is still a line to edit
                    if (CurrentLine > -1)
                    {
                        //Always delete a visible character
                        ((String)Buffer[CurrentLine])[--CurrentChar] = (char)((' ' & 0xFF) | CurrentAttr);
                    }
                }
                //Else if upwards arrow character is found
                //  scroll up one line
                else if (str[i] == '\u2191')
                {
                    Scroll(-1);
                }
                //Else if downwards arrow character is found
                //  scroll down one line
                else if (str[i] == '\u2193')
                {
                    Scroll(1);
                }
                //Otherwise, just output the character to the current position
                //  and move current position to the next character.
                else
                {
                    // Check for tab character - if so, output four spaces (rudimentary tab support)
                    if (str[i] == '\t')
                    {
                        Write("    ");
                    }
                    else
                    {
                        //The character must also be or'ed with the current attribute so it appears the correct
                        //  colour. 
                        //Strings in the core kernel are stored as 2-byte unicode but we output only ASCII
                        //  so the character must be and'ed with 0xFF to clear the top byte else it would
                        //  interfere with the attribute (colour).
                        ((String)Buffer[CurrentLine])[CurrentChar++] = (char)((str[i] & 0xFF) | CurrentAttr);
                    }
                }
            }
            //Call update to update the screen with the new text.
            Update();
        }

        /// <summary>
        ///     Writes the specified number as an unsigned decimal string.
        /// </summary>
        /// <param name="num">The number to write as a decimal.</param>
        public virtual void Write_AsDecimal(uint num)
        {
            //Write the resulting number
            Write(UInt32.ToDecimalString(num));
        }

        /// <summary>
        ///     Writes the specified number as an signed decimal string.
        /// </summary>
        /// <param name="num">The number to write as a decimal.</param>
        public virtual void Write_AsDecimal(int num)
        {
            Write(Int32.ToDecimalString(num));
        }

        /// <summary>
        ///     Writes a new line to the output.
        /// </summary>
        public virtual void WriteLine()
        {
            String line = null;
            //If we've reached the maximum number of lines
            //  to store in the buffer
            if (Buffer.Count == MaxBufferSize)
            {
                // Remove the first line (oldest line - appears at the top
                //   of the screen if the user scrolls all the way to the top) 
                //   to create space.
                line = (String)Buffer[0];
                Buffer.RemoveAt(0);

                // And make it into a new blank line
                for (int i = 0; i < line.Length; i++)
                {
                    line[i] = ' ';
                }
            }
            else
            {
                //Create a new blank line
                line = CreateBlankLine();
            }
            // Add the blank line to the bottom of the buffer
            Buffer.Add(line);

            //Update the current line / character
            CurrentLine = Buffer.Count - 1;
            CurrentChar = 0;

            //Update the screen
            Update();
        }

        /// <summary>
        ///     Writes the specified string followed by a new line.
        /// </summary>
        /// <param name="str">The string to write before the new line.</param>
        public virtual void WriteLine(String str)
        {
            //Write the string followed by a new line
            Write(str);
            WriteLine();
        }

        /// <summary>
        ///     Writes the specified number as an unsigned decimal followed by a new line.
        /// </summary>
        /// <param name="num">The number to write as a decimal.</param>
        public virtual void WriteLine_AsDecimal(uint num)
        {
            //Write the number followed by a new line
            Write_AsDecimal(num);
            WriteLine();
        }

        /// <summary>
        ///     Writes the specified number as an ssigned decimal followed by a new line.
        /// </summary>
        /// <param name="num">The number to write as a decimal.</param>
        public virtual void WriteLine_AsDecimal(int num)
        {
            //Write the number followed by a new line
            Write_AsDecimal(num);
            WriteLine();
        }

        /// <summary>
        ///     Clears the entire output.
        /// </summary>
        public virtual void Clear()
        {
            //Empty the buffer
            Buffer.Empty();
            //Reset the current position
            CurrentLine = 0;
            CurrentChar = 0;
            //Write a new blank line - required as we always expect to have at
            //  least 1 blank line to write to.
            WriteLine();
        }

        /// <summary>
        ///     Plays the PC speaker for 500ms at (approx.) the frequency of low-B.
        /// </summary>
        public virtual void Beep()
        {
            //Default beep - 245Hz for 0.5s
            Beep(245, 500);
        }

        /// <summary>
        ///     Plsy the PC speaker for the specified length of time (in milliseconds) at (approx.) the specified
        ///     integer frequency.
        /// </summary>
        /// <param name="freq">The frequency to play at.</param>
        /// <param name="duration">The duration of the beep (in milliseconds).</param>
        public virtual void Beep(int freq, uint duration)
        {
            PIT.ThePIT.PlaySound(freq);
            PIT.ThePIT.Wait(duration);
            PIT.ThePIT.MuteSound();
        }

        /// <summary>
        ///     Sets the cursor position.
        /// </summary>
        /// <param name="character">The offset from the start of the line to the cursor.</param>
        /// <param name="line">The line number (from the display perspective, not the buffer perspective) of the cursor.</param>
        public abstract void SetCursorPosition(ushort character, ushort line);

        /// <summary>
        ///     Scrolls the display the specified distance.
        /// </summary>
        /// <param name="dist">The distance to scroll. +ve scrolls downwards, -ve scrolls upwards.</param>
        public virtual void Scroll(int dist)
        {
            //Move the current line the specified distance
            CurrentLine += dist;
            //Clamp the current line value to within the buffer limits
            if (CurrentLine < 0)
            {
                CurrentLine = 0;
            }
            else if (CurrentLine >= Buffer.Count)
            {
                CurrentLine = Buffer.Count - 1;
            }
            //Update the screen
            Update();
        }

        /// <summary>
        ///     Sets the current text (foreground) colour.
        /// </summary>
        /// <param name="col">The colour to set as the text colour.</param>
        public virtual void Colour(byte col)
        {
            CurrentAttr = (ushort)((CurrentAttr & 0x00FF) | (col << 8));

            String str = (String)Buffer[CurrentLine];

            //Set the attr of all characters in the rest of the line to
            //  the current colour.
            for (int i = CurrentChar; i < str.Length; i++)
            {
                str[i] = (char)((str[i] & 0x00FF) | CurrentAttr);
            }
        }

        /// <summary>
        ///     Sets the text colour to the default colour (white).
        /// </summary>
        public virtual void DefaultColour()
        {
            Colour(0x0F);
        }

        /// <summary>
        ///     Sets the text colour to the warning colour (yellow).
        /// </summary>
        public virtual void WarningColour()
        {
            Colour(0x0E);
        }

        /// <summary>
        ///     Sets the text colour to the error colour (red).
        /// </summary>
        public virtual void ErrorColour()
        {
            Colour(0x04);
        }

        /// <summary>
        ///     Initialises the default console.
        /// </summary>
        public static void InitDefault()
        {
            if (Default == null)
            {
                Default = new VGAConsole();
            }
        }

        /// <summary>
        ///     Cleans up the default console.
        /// </summary>
        public static void CleanDefault()
        {
            Default = null;
        }
    }
}