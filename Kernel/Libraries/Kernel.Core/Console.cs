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
            //Create a blank line (all characters set to 0s)
            FOS_System.String str = FOS_System.String.New(LineLength);

            //Set the attr of all characters in the new blank line to
            //  the current character.
            //This is so that typed characters at least appear in current 
            //  colour otherwise they wouldn't show at all.
            for (int i = 0; i < str.length; i++)
            {
                str[i] |= (char)CurrentAttr;
            }

            //Return the new blank line.
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
            //Loop through each character, outputting them.
            for (int i = 0; i < str.length; i++)
            {
                //If we have reached the end of the current line,
                //  create a new one using WriteLine()
                if (CurrentChar == LineLength)
                {
                    WriteLine();
                }
                //If a \n (newline) character is found,
                //  create a new line using WriteLine()
                if (str[i] == '\n')
                {
                    WriteLine();
                }
                //Otherwise, just output the character to the current position
                //  and move current position to the next character.
                else
                {
                    //The character must also be or'ed with the current attribute so it appears the correct
                    //  colour. 
                    //Strings in the core kernel are stored as 2-byte unicode but we output only ASCII
                    //  so the character must be and'ed with 0xFF to clear the top byte else it would
                    //  interfere with the attribute (colour).
                    ((FOS_System.String)Buffer[CurrentLine])[CurrentChar++] = (char)((str[i] & 0xFF) | CurrentAttr);
                }
            }
            //Call update to update the screen with the new text.
            Update();
            //Update the cursor position.
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
            //If the number is already 0, just output 0
            //  straight off. The algorithm below does not
            //  work if num is 0.
            if (num != 0)
            {
                //Loop through outputting the units value (base 10)
                //  and then dividing by 10 to move to the next digit.
                while (num > 0)
                {
                    //Get the units
                    uint rem = num % 10;
                    //Output the units character
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
                    //Divide by 10 to move to the next digit.
                    num /= 10;
                }
            }
            else
            {
                result = "0";
            }
            //Write the resulting number
            Write(result);
        }
        /// <summary>
        /// Writes the specified number as an signed decimal string.
        /// </summary>
        /// <param name="num">The number to write as a decimal.</param>
        public virtual void Write_AsDecimal(Int32 num)
        {
            //This functions exactly the same as its unsigned 
            //  counterpart but it adds a minus sign if the number
            //  is negative.
            FOS_System.String result = "";
            if (num != 0)
            {
                bool neg = num < 0;
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

                if (neg)
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
            //If we've reached the maximum number of lines
            //  to store in the buffer
            if (Buffer.Count == MaxBufferSize)
            {
                //Remove the first line (oldest line - appears at the top
                //  of the screen if the user scrolls all the way to the top) 
                //  to create space.
                Buffer.RemoveAt(0);
            }

            //Add a new blank line at the bottom of the buffer
            Buffer.Add(CreateBlankLine());

            //Update the current line / character
            CurrentLine = Buffer.Count - 1;
            CurrentChar = 0;
            
            //Update the screen
            Update();

            //Update the cursor position
            SetCursorPosition((ushort)(CurrentChar - GetDisplayOffset_Char()),
                              (ushort)(CurrentLine - GetDisplayOffset_Line()));
        }
        /// <summary>
        /// Writes the specified string followed by a new line.
        /// </summary>
        /// <param name="str">The string to write before the new line.</param>
        public virtual void WriteLine(FOS_System.String str)
        {
            //Write the string followed by a new line
            Write(str);
            WriteLine();
        }
        /// <summary>
        /// Writes the specified number as an unsigned decimal followed by a new line.
        /// </summary>
        /// <param name="num">The number to write as a decimal.</param>
        public virtual void WriteLine_AsDecimal(UInt32 num)
        {
            //Write the number followed by a new line
            Write_AsDecimal(num);
            WriteLine();
        }
        /// <summary>
        /// Writes the specified number as an ssigned decimal followed by a new line.
        /// </summary>
        /// <param name="num">The number to write as a decimal.</param>
        public virtual void WriteLine_AsDecimal(Int32 num)
        {
            //Write the number followed by a new line
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
            //Read the key and output it to the screen
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
            //Use the blocking call to get the next recognised key pressed
            Hardware.Devices.KeyMapping mapping = Hardware.Devices.Keyboard.Default.ReadMapping();
            //If the key has a character representation and we should output the character
            if (mapping.Value != '\0' && output)
            {
                //Write the character representation of the key
                Write(mapping.Value);
            }
            //Return the key
            return mapping.Key;
        }

        /// <summary>
        /// Blocking. Reads the next valid (i.e. not \0) character from the keyboard and outputs it.
        /// </summary>
        /// <returns>The character that was read.</returns>
        public virtual char Read()
        {
            //Use the blocking call to get the next recognised character entered by the user
            char result = Hardware.Devices.Keyboard.Default.ReadChar();
            //Output the character
            Write(result);        
            //Return the character
            return result;
        }
        /// <summary>
        /// Blocking. Reads all the next valid (i.e. not \0) characters from the keyboard and outputs them
        /// until a new line is entered (using the Enter key). Also supports backspace and escape keys.
        /// </summary>
        /// <returns>The line of text.</returns>
        public virtual FOS_System.String ReadLine()
        {
            //Temp store for the result
            FOS_System.String result = "";
            //Used to store the last key pressed
            Hardware.Devices.KeyMapping c;
            //Used to store the index and position of the typing position when the 
            //  user started inputting. This allows us to reset the display of the 
            //  current input if the user presses the escape key.
            Int32 StartLine = CurrentLine;
            Int32 StartChar = CurrentChar;
            //Loop through getting characters until the enter key is pressed
            while((c = Hardware.Devices.Keyboard.Default.ReadMapping()).Key != Hardware.Devices.KeyboardKey.Enter)
            {
                //If backspace was pressed:
                if (c.Key == Hardware.Devices.KeyboardKey.Backspace)
                {
                    //If we actually have something to delete:
                    if (result.length > 0)
                    {
                        //Remove the last character
                        result = result.Substring(0, result.length - 1);
                        //Move backwards 1 character
                        CurrentChar--;
                        //If we have moved past the beginning of a line
                        if (CurrentChar < 0)
                        {
                            //Remove a line
                            Buffer.RemoveAt(CurrentLine);

                            //Move to end of previous line
                            CurrentChar = LineLength - 1;
                            //Move to previous line
                            CurrentLine--;
                        }
                        //Set the current character (the last one to be typed) to a blank character
                        ((FOS_System.String)Buffer[CurrentLine])[CurrentChar] = (char)(' ' | CurrentAttr);
                        //Update the screen
                        Update();
                        //Update the cursor position
                        SetCursorPosition((ushort)(CurrentChar - GetDisplayOffset_Char()),
                                          (ushort)(CurrentLine - GetDisplayOffset_Line()));
                    }
                }
                else if(c.Key == Hardware.Devices.KeyboardKey.Escape)
                {
                    //Clear out the result
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

                    //Update the screen
                    Update();
                    //Update the cursor position
                    SetCursorPosition((ushort)(CurrentChar - GetDisplayOffset_Char()),
                                      (ushort)(CurrentLine - GetDisplayOffset_Line()));
                }
                else if (c.Key == Hardware.Devices.KeyboardKey.UpArrow)
                {
                    //Scroll up the screen 1 line
                    Scroll(-1);
                }
                else if (c.Key == Hardware.Devices.KeyboardKey.DownArrow)
                {
                    //Scroll down the screen 1 line
                    Scroll(1);
                }
                //If the key has a character representation
                else if (c.Value != '\0')
                {
                    //Add the character to the result
                    result += c.Value;
                    //Output the character
                    Write(c.Value);
                }
            }
            //Enter key was pressed, which is what caused us to exit the loop, 
            //  so type a new line
            WriteLine();

            //Return the resulting line
            return result;
        }

        /// <summary>
        /// Clears the entire output.
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
        /// Plays the PC speaker for 500ms at (approx.) the frequency of low-B.
        /// </summary>
        public virtual void Beep()
        {
            //Default beep - 245Hz for 0.5s
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
        /// Sets the current text (foreground) colour.
        /// </summary>
        /// <param name="col">The colour to set as the text colour.</param>
        public virtual void Colour(byte col)
        {
            CurrentAttr = (ushort)((CurrentAttr & 0x00FF) | (col << 8));

            FOS_System.String str = (FOS_System.String)Buffer[CurrentLine];

            //Set the attr of all characters in the rest of the line to
            //  the current colour.
            for (int i = CurrentChar; i < str.length; i++)
            {
                str[i] = (char)((str[i] & 0x00FF) | CurrentAttr);
            }
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
