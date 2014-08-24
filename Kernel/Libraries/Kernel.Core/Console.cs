#region Copyright Notice
// ------------------------------------------------------------------------------ //
//                                                                                //
//               All contents copyright � Edward Nutting 2014                     //
//                                                                                //
//        You may not share, reuse, redistribute or otherwise use the             //
//        contents this file outside of the Fling OS project without              //
//        the express permission of Edward Nutting or other copyright             //
//        holder. Any changes (including but not limited to additions,            //
//        edits or subtractions) made to or from this document are not            //
//        your copyright. They are the copyright of the main copyright            //
//        holder for all Fling OS files. At the time of writing, this             //
//        owner was Edward Nutting. To be clear, owner(s) do not include          //
//        developers, contributors or other project members.                      //
//                                                                                //
// ------------------------------------------------------------------------------ //
#endregion
    
using System;
using Kernel.FOS_System.Collections;

namespace Kernel.Core
{
    /// <summary>
    /// Represents a virtual Console input/output display. Also provides static methods for initialising the default
    /// console.
    /// </summary>
    public abstract class Console : FOS_System.Object
    {
        // Features:
        //  - Writing/reading
        //  - Scrolling
        //  - Foreground/Background Colour
        //  - Beep
        //  - Cursor

        /// <summary>
        /// The buffer of lines of text.
        /// </summary>
        protected List Buffer = new List(100);
        /// <summary>
        /// The current line where the cursor is in the buffer.
        /// </summary>
        protected Int32 CurrentLine = 0;
        /// <summary>
        /// Thge current character in the current line where the cursor is.
        /// </summary>
        protected Int32 CurrentChar = 0;

        /// <summary>
        /// The maximum length of a line.
        /// </summary>
        protected Int32 LineLength = 80;
        /// <summary>
        /// The maxmimum size of the buffer (in lines).
        /// </summary>
        protected Int32 MaxBufferSize = 200;
        /// <summary>
        /// The current character attribute.
        /// </summary>
        protected ushort CurrentAttr = 0;
        
        /// <summary>
        /// Initialises a new instance of a Console.
        /// </summary>
        public Console()
        {
            Buffer.Add(CreateBlankLine());
            DefaultColour();
        }

        /// <summary>
        /// Creates a blank line (a line filled with spaces set with the current attribute).
        /// </summary>
        /// <returns>The new line.</returns>
        protected FOS_System.String CreateBlankLine()
        {
            FOS_System.String str = FOS_System.String.New(LineLength);

            for (int i = 0; i < str.length; i++)
            {
                str[i] |= (char)CurrentAttr;
            }

            return str;
        }

        /// <summary>
        /// When overridden in a derived class, updates the display output.
        /// </summary>
        protected abstract void Update();
        /// <summary>
        /// When overriden in a dervied class, gets the offset from the current line to the actual line position of
        /// the cursor.
        /// </summary>
        /// <returns>The offset to be subtracted from the current line.</returns>
        protected abstract int GetDisplayOffset_Line();
        /// <summary>
        /// When overriden in a dervied class, gets the offset from the current character to the actual character 
        /// position of the cursor.
        /// </summary>
        /// <returns>The offset to be subtracted from the current character.</returns>
        protected abstract int GetDisplayOffset_Char();

        /// <summary>
        /// Writes the specified text to the output. Overflows to the next line if necessary. 
        /// "\n" characters will result in a new line being started.
        /// </summary>
        /// <param name="str">The string to write.</param>
        public virtual void Write(FOS_System.String str)
        {
            for (int i = 0; i < str.length; i++)
            {
                if (CurrentChar == LineLength)
                {
                    WriteLine();
                }
                if (str[i] == '\n')
                {
                    WriteLine();
                }
                else
                {
                    ((FOS_System.String)Buffer[CurrentLine])[CurrentChar++] = (char)((str[i] & 0xFF) | CurrentAttr);
                }
            }
            Update();
            SetCursorPosition((ushort)(CurrentChar - GetDisplayOffset_Char()),
                              (ushort)(CurrentLine - GetDisplayOffset_Line()));
        }
        /// <summary>
        /// Writes the specified number as an unsigned decimal string.
        /// </summary>
        /// <param name="num">The number to write as a decimal.</param>
        public virtual void Write_AsDecimal(UInt32 num)
        {
            FOS_System.String result = "";
            if (num != 0)
            {
                while (num > 0)
                {
                    uint rem = num % 10;
                    switch (rem)
                    {
                        case 0:
                            result = "0" + result;
                            break;
                        case 1:
                            result = "1" + result;
                            break;
                        case 2:
                            result = "2" + result;
                            break;
                        case 3:
                            result = "3" + result;
                            break;
                        case 4:
                            result = "4" + result;
                            break;
                        case 5:
                            result = "5" + result;
                            break;
                        case 6:
                            result = "6" + result;
                            break;
                        case 7:
                            result = "7" + result;
                            break;
                        case 8:
                            result = "8" + result;
                            break;
                        case 9:
                            result = "9" + result;
                            break;
                    }
                    num /= 10;
                }
            }
            else
            {
                result = "0";
            }
            Write(result);
        }
        /// <summary>
        /// Writes the specified number as an signed decimal string.
        /// </summary>
        /// <param name="num">The number to write as a decimal.</param>
        public virtual void Write_AsDecimal(Int32 num)
        {
            FOS_System.String result = "";
            if (num != 0)
            {
                while (num > 0)
                {
                    int rem = num % 10;
                    switch (rem)
                    {
                        case 0:
                            result = "0" + result;
                            break;
                        case 1:
                            result = "1" + result;
                            break;
                        case 2:
                            result = "2" + result;
                            break;
                        case 3:
                            result = "3" + result;
                            break;
                        case 4:
                            result = "4" + result;
                            break;
                        case 5:
                            result = "5" + result;
                            break;
                        case 6:
                            result = "6" + result;
                            break;
                        case 7:
                            result = "7" + result;
                            break;
                        case 8:
                            result = "8" + result;
                            break;
                        case 9:
                            result = "9" + result;
                            break;
                    }
                    num /= 10;
                }

                if (num < 0)
                {
                    result = "-" + result;
                }
            }
            else
            {
                result = "0";
            }
            Write(result);
        }

        /// <summary>
        /// Writes a new line to the output.
        /// </summary>
        public virtual void WriteLine()
        {
            if (Buffer.Count == MaxBufferSize)
            {
                Buffer.RemoveAt(0);
                Buffer.Add(CreateBlankLine());
            }
            else
            {
                Buffer.Add(CreateBlankLine());
            }
            CurrentLine = Buffer.Count - 1;
            CurrentChar = 0;
            Update();

            SetCursorPosition((ushort)(CurrentChar - GetDisplayOffset_Char()),
                              (ushort)(CurrentLine - GetDisplayOffset_Line()));
        }
        /// <summary>
        /// Writes the specified string followed by a new line.
        /// </summary>
        /// <param name="str">The string to write before the new line.</param>
        public virtual void WriteLine(FOS_System.String str)
        {
            Write(str);
            WriteLine();
        }
        /// <summary>
        /// Writes the specified number as an unsigned decimal followed by a new line.
        /// </summary>
        /// <param name="num">The number to write as a decimal.</param>
        public virtual void WriteLine_AsDecimal(UInt32 num)
        {
            Write_AsDecimal(num);
            WriteLine();
        }
        /// <summary>
        /// Writes the specified number as an ssigned decimal followed by a new line.
        /// </summary>
        /// <param name="num">The number to write as a decimal.</param>
        public virtual void WriteLine_AsDecimal(Int32 num)
        {
            Write_AsDecimal(num);
            WriteLine();
        }

        /// <summary>
        /// Blocking. Reads the next key pressed from the keyboard and outputs its character representation
        /// (if it has a representation).
        /// </summary>
        /// <returns>The keyboard key that was pressed.</returns>
        public virtual Hardware.Devices.KeyboardKey ReadKey()
        {
            return ReadKey(true);
        }
        /// <summary>
        /// Blocking. Reads the next key pressed from the keyboard and outputs its character representation
        /// (if it has a representation) if requested.
        /// </summary>
        /// <param name="output">Whether to output the character representation of the key or not.</param>
        /// <returns>The keyboard key that was pressed.</returns>
        public virtual Hardware.Devices.KeyboardKey ReadKey(bool output)
        {
            Hardware.Devices.KeyMapping mapping = Hardware.Devices.Keyboard.Default.ReadMapping();
            if (mapping.Value != '\0' && output)
            {
                Write(mapping.Value);
            }
            return mapping.Key;
        }

        /// <summary>
        /// Blocking. Reads the next valid (i.e. not \0) character from the keyboard and outputs it.
        /// </summary>
        /// <returns>The character that was read.</returns>
        public virtual char Read()
        {
            char result = Hardware.Devices.Keyboard.Default.ReadChar();
            Write(result);        
            return result;
        }
        /// <summary>
        /// Blocking. Reads all the next valid (i.e. not \0) characters from the keyboard and outputs them
        /// until a new line is entered (using the Enter key). Also supports backspace and escape keys.
        /// </summary>
        /// <returns>The line of text.</returns>
        public virtual FOS_System.String ReadLine()
        {
            FOS_System.String result = "";
            Hardware.Devices.KeyMapping c;
            Int32 StartLine = CurrentLine;
            Int32 StartChar = CurrentChar;
            while((c = Hardware.Devices.Keyboard.Default.ReadMapping()).Key != Hardware.Devices.KeyboardKey.Enter)
            {
                if (c.Key == Hardware.Devices.KeyboardKey.Backspace)
                {
                    if (result.length > 0)
                    {
                        result = result.Substring(0, result.length - 1);
                        CurrentChar--;
                        if (CurrentChar < 0)
                        {
                            Buffer.RemoveAt(CurrentLine);

                            CurrentChar = LineLength - 1;
                            CurrentLine--;
                        }
                        ((FOS_System.String)Buffer[CurrentLine])[CurrentChar] = (char)(' ' | CurrentAttr);
                        Update();
                        SetCursorPosition((ushort)(CurrentChar - GetDisplayOffset_Char()),
                                          (ushort)(CurrentLine - GetDisplayOffset_Line()));
                    }
                }
                else if(c.Key == Hardware.Devices.KeyboardKey.Escape)
                {
                    result = "";

                    //Reset to StartLine, StartChar
                    for(int i = CurrentLine; i >= StartLine; i--)
                    {
                        if (i == StartLine)
                        {
                            FOS_System.String line = ((FOS_System.String)Buffer[i]);
                            for (int j = StartChar; j < LineLength; j++)
                            {
                                line[j] = ' ';
                            }
                        }
                        else
                        {
                            Buffer.RemoveAt(i);
                        }
                    }

                    CurrentLine = StartLine;
                    CurrentChar = StartChar;

                    Update();
                    SetCursorPosition((ushort)(CurrentChar - GetDisplayOffset_Char()),
                                      (ushort)(CurrentLine - GetDisplayOffset_Line()));
                }
                else if (c.Key == Hardware.Devices.KeyboardKey.UpArrow)
                {
                    Scroll(-1);
                }
                else if (c.Key == Hardware.Devices.KeyboardKey.DownArrow)
                {
                    Scroll(1);
                }
                else if (c.Value != '\0')
                {
                    result += c.Value;
                    Write(c.Value);
                }
            }
            WriteLine();
            return result;
        }

        /// <summary>
        /// Clears the entire output.
        /// </summary>
        public virtual void Clear()
        {
            Buffer.Empty();
            CurrentLine = 0;
            CurrentChar = 0;
            WriteLine();
        }

        /// <summary>
        /// Plays the PC speaker for 500ms at (approx.) the frequency of low-B.
        /// </summary>
        public virtual void Beep()
        {
            Beep(245, 500);
        }
        /// <summary>
        /// Plsy the PC speaker for the specified length of time (in milliseconds) at (approx.) the specified 
        /// integer frequency.
        /// </summary>
        /// <param name="freq">The frequency to play at.</param>
        /// <param name="duration">The duration of the beep (in milliseconds).</param>
        public virtual void Beep(Int32 freq, UInt32 duration)
        {
            Hardware.Timers.PIT.ThePIT.PlaySound(freq);
            Hardware.Timers.PIT.ThePIT.Wait(duration);
            Hardware.Timers.PIT.ThePIT.MuteSound();
        }

        /// <summary>
        /// Sets the cursor position.
        /// </summary>
        /// <param name="character">The offset from the start of the line to the cursor.</param>
        /// <param name="line">The line number (from the display perspective, not the buffer perspective) of the cursor.</param>
        public abstract void SetCursorPosition(ushort character, ushort line);

        /// <summary>
        /// Scrolls the display the specified distance.
        /// </summary>
        /// <param name="dist">The distance to scroll. +ve scrolls downwards, -ve scrolls upwards.</param>
        public virtual void Scroll(Int32 dist)
        {
            CurrentLine += dist;
            if (CurrentLine < 0)
            {
                CurrentLine = 0;
            }
            else if (CurrentLine >= Buffer.Count)
            {
                CurrentLine = Buffer.Count - 1;
            }
            Update();
        }

        /// <summary>
        /// Sets the current text (foreground) colour.
        /// </summary>
        /// <param name="col">The colour to set as the text colour.</param>
        public virtual void Colour(byte col)
        {
            CurrentAttr = (ushort)((CurrentAttr & 0x00FF) | (col << 8));
        }
        /// <summary>
        /// Sets the text colour to the default colour (white).
        /// </summary>
        public virtual void DefaultColour()
        {
            Colour(0x0F);
        }
        /// <summary>
        /// Sets the text colour to the warning colour (yellow).
        /// </summary>
        public virtual void WarningColour()
        {
            Colour(0x0E);
        }
        /// <summary>
        /// Sets the text colour to the error colour (red).
        /// </summary>
        public virtual void ErrorColour()
        {
            Colour(0x04);
        }

        /// <summary>
        /// The default console for the core kernel.
        /// </summary>
        public static Console Default;
        /// <summary>
        /// Initialises the default console.
        /// </summary>
        public static void InitDefault()
        {
            if (Default == null)
            {
                Default = new Consoles.AdvancedConsole();
            }
        }
        /// <summary>
        /// Cleans up the default console.
        /// </summary>
        public static void CleanDefault()
        {
            Default = null;
        }
    }
}
