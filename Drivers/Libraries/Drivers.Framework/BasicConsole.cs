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

using Drivers.Compiler.Attributes;
using Drivers.Framework;

namespace Drivers
{
    /// <summary>
    ///     Delegate type for secondary output handlers for the BasicConsole.
    /// </summary>
    /// <remarks>
    ///     Secondary output handlers can be used to copy or completely redirect all output from the BasicConsole
    ///     from the screen to an alternative destination. For example, output can be redirected from the screen
    ///     to a serial port. That output can then be saved to a text file for post-execution review.
    /// </remarks>
    /// <param name="str">The string to output.</param>
    public delegate void SecondaryOutputHandler(String str);

    /// <summary>
    ///     A basic console implementation - uses the BIOS's fixed text-video memory to output ASCII text.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This class is a very basic console. It uses the default, x86 setup for
    ///         VGA text-mode graphics and simply outputs text to graphics memory.
    ///         When a new line is required, it simply shifts the graphics memory up
    ///         one line and discards any video memory shifted off the top of the screen.
    ///         Scrolling back down is thus not possible as the information is lost.
    ///     </para>
    ///     <para>
    ///         For a better console implementation see Console and AdvancedConsole classes in
    ///         Drivers library (/namespace)
    ///     </para>
    ///     <para>
    ///         Some of the code used appears inefficient or needlessly expanded. That's
    ///         because it is. Deliberately so. The reason is that the code used uses the
    ///         minimum of IL ops and the simpler IL ops making the initial compiler work
    ///         much smaller and simpler to do. It also means that if the compiler breaks
    ///         in any way, the BasicConsole class is likely to still work thus making it
    ///         the most useful debugging tool.
    ///     </para>
    ///     <para>
    ///         All of this code has been thoroughly used and abused, which means it is
    ///         well tested i.e. reliable and robust. Do not alter the code in any way!
    ///         Really, this code does not need modifying and if you do so, you're more
    ///         likely to break it than fix or improve it.
    ///     </para>
    ///     <para>
    ///         This code is specifically designed for 80x25 VGA text-mode. In theory you
    ///         could change the "rows" and "cols" values, but this would actually break
    ///         the code because some of it has values of 80, 25, 160 and 50 hard-coded
    ///         which you would need to change. I am reluctant to go changing these hard
    ///         coded values for the reasons given in prior notes.
    ///     </para>
    /// </remarks>
    public static unsafe class BasicConsole
    {
        /// <summary>
        ///     Whether the primary output destination (the screen) is enabled or not.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Default value is true i.e. the BasicConsole will output all text to the screen.
        ///     </para>
        ///     <para>
        ///         With lots of trace code enabled the BasicConsole can end up printing a lot of text
        ///         very quickly. This is both impossible to read and untraceable as it cannot be reviewed
        ///         after it disappears from the screen. Add to that the newer multi-processing support
        ///         and the BasicConsole ceases to be useful - in fact it gets in the way.
        ///     </para>
        ///     <para>
        ///         Switching off the primary output reduces the junk outputted to the screen. To retain
        ///         (or rather, obtain) traceable output, use the secondary output to redirect BasicConsole
        ///         printing to a serial port such as COM1. VMWare has a nice option for saving serial port
        ///         output directly to a file.
        ///     </para>
        /// </remarks>
        public static bool PrimaryOutputEnabled = true;

        /// <summary>
        ///     Whether the secondary output destination is enabled or not.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Default value is true i.e. the BasicConsole will output all text to the secondary output (if it is not null).
        ///     </para>
        /// </remarks>
        /// <seealso cref="SecondaryOutput" />
        public static bool SecondaryOutputEnabled = true;

        /// <summary>
        ///     The secondary output handler.
        /// </summary>
        /// <remarks>
        ///     If <see cref="SecondaryOutputEnabled" /> is set to true, this handler will be called for all Write calls to
        ///     the BasicConsole.
        /// </remarks>
        public static SecondaryOutputHandler SecondaryOutput = null;

        /// <summary>
        ///     Used to indicate whether the Basic Console has been initialised or not.
        /// </summary>
        /// <remarks>
        ///     Useful for when fixing low-level errors in compiler which result in incorrect execution order
        ///     and thus use of Basic Console before it is ready.
        /// </remarks>
        public static bool Initialised;

        /// <summary>
        ///     The offset from the start of the memory (in characters) to write the next character to.
        /// </summary>
        /// <remarks>
        ///     This would cause an issue if you changed the line length after already having printed text
        ///     because you'd want to leave the next print location at the start of a new line.
        /// </remarks>
        private static int offset;

        /// <summary>
        ///     A pointer to the start of the (character-based) video memory.
        /// </summary>
        public static char* vidMemBasePtr = (char*)0xB8000;

        /// <summary>
        ///     Numbers of rows in the video memory.
        /// </summary>
        public static int rows = 25;

        /// <summary>
        ///     Number of columns in the video memory.
        /// </summary>
        public static int cols = 80;

        /// <summary>
        ///     The colour to print characters in. Do not set directly, use SetTextColour/SetBackgroundColour.
        /// </summary>
        public static char colour;

        /// <summary>
        ///     The background colour. Do not set directly, use SetTextColour/SetBackgroundColour.
        /// </summary>
        public static char bg_colour;

        /// <summary>
        ///     Default colour to print characters in.
        /// </summary>
        public static char default_colour;

        /// <summary>
        ///     Colour to print warning characters in.
        /// </summary>
        public static char warning_colour;

        /// <summary>
        ///     Colour to print error characters in.
        /// </summary>
        public static char error_colour;

        /// <summary>
        ///     Whether to disable the delay output method. Used when debugging is enabled.
        /// </summary>
        public static bool DisableDelayOutput = false;

        /// <summary>
        ///     The offset from the start of the memory (in characters) to write the next character to.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public static int Offset
        {
            get { return offset; }
        }

        /// <summary>
        ///     Static constructor for the Basic Console.
        /// </summary>
        /// <remarks>
        ///     This constructor should, assuming the compiler hasn't changed much, be one of the first, if not
        ///     the first, static constructor to be called. Almost all other static constructors rely on the
        ///     Basic Console being enable, one way or another.
        /// </remarks>
        static BasicConsole()
        {
        }

        /// <summary>
        ///     Initialises the BasicConsole class.
        /// </summary>
        [NoDebug]
        [NoGC]
        public static void Init()
        {
            //Colour info stored in the high byte:
            //  Hi-4-bits: Background
            //  Lo-4-bits: Foreground

            //Black background
            bg_colour = (char)0x0000;
            //White foreground
            default_colour = (char)0x0F00;
            //Yellow foreground
            warning_colour = (char)0x0E00;
            //Red foreground
            error_colour = (char)0x0400;

            //Background | Foreground
            colour = (char)(bg_colour | default_colour);

            Initialised = true;
        }

        /// <summary>
        ///     Sets the console text colour.
        /// </summary>
        /// <param name="aText_colour">The text colour to use.</param>
        [NoDebug]
        [NoGC]
        public static void SetTextColour(char aText_colour)
        {
            if (!Initialised) return;
            colour = (char)(bg_colour | (aText_colour & 0x0F00));
        }

        /// <summary>
        ///     Sets the console background colour.
        /// </summary>
        /// <param name="aBg_colour">The background colour to use.</param>
        [NoDebug]
        [NoGC]
        public static void SetBgColour(char aBg_colour)
        {
            if (!Initialised) return;
            bg_colour = (char)(aBg_colour & 0xF000);
            colour = (char)(bg_colour | (colour & 0x0F00));
        }

        /// <summary>
        ///     Clears the output to all black.
        /// </summary>
        [NoDebug]
        [NoGC]
        public static void Clear()
        {
            if (!Initialised) return;
            //Clear out every character on the screen
            int numToClear = rows*cols;
            //Start at beginning of video memory
            char* vidMemPtr = vidMemBasePtr;
            //Loop through all video memory
            while (numToClear > 0)
            {
                //Set output to no character, no foreground colour, just the 
                //  background colour.
                vidMemPtr[0] = bg_colour;
                //Then move to the next character in
                //  video memory.
                vidMemPtr++;
                //And decrement the count
                numToClear--;
            }

            //And set our offset to 0
            offset = 0;
        }

        /// <summary>
        ///     Writes the specified string to the output at the current offset.
        /// </summary>
        /// <param name="str">The string to output.</param>
        /// <remarks>
        ///     If necessary, this method will move all existing text up the necessary number of lines to fit the new text on the
        ///     bottom
        ///     of the screen.
        /// </remarks>
        [NoDebug]
        [NoGC]
        public static void Write(String str)
        {
            if (!Initialised) return;
            //If string is null, just don't write anything
            if (str == null)
            {
                //Do not make this throw an exception. The BasicConsole
                //  is largely a debugging tool - it should be reliable,
                //  robust and not throw exceptions.
                return;
            }

            if (PrimaryOutputEnabled)
            {
                int strLength = str.length;
                int maxOffset = rows*cols;

                //This block shifts the video memory up the required number of lines.
                if (offset + strLength > maxOffset)
                {
                    int amountToShift = offset + strLength - maxOffset;
                    amountToShift = amountToShift + (80 - amountToShift%80);
                    offset -= amountToShift;

                    char* vidMemPtr_Old = vidMemBasePtr;
                    char* vidMemPtr_New = vidMemBasePtr + amountToShift;
                    char* maxVidMemPtr = vidMemBasePtr + cols*rows;
                    while (vidMemPtr_New < maxVidMemPtr)
                    {
                        vidMemPtr_Old[0] = vidMemPtr_New[0];
                        vidMemPtr_Old++;
                        vidMemPtr_New++;
                    }
                }

                //This block outputs the string in the current foreground / background colours.
                char* vidMemPtr = vidMemBasePtr + offset;
                char* strPtr = str.GetCharPointer();
                while (strLength > 0)
                {
                    vidMemPtr[0] = (char)((*strPtr & 0x00FF) | colour);

                    strLength--;
                    vidMemPtr++;
                    strPtr++;
                    offset++;
                }
            }

            if (SecondaryOutput != null && SecondaryOutputEnabled)
            {
                SecondaryOutput(str);
            }
        }

        /// <summary>
        ///     Writes the specified string to the output at the current offset then moves the offset to the end of the line.
        /// </summary>
        /// <param name="str">The string to output.</param>
        /// <remarks>
        ///     This also blanks out the rest of the line to make sure no artifacts are left behind.
        /// </remarks>
        [NoDebug]
        [NoGC]
        public static void WriteLine(String str)
        {
            if (!Initialised) return;
            if (str == null)
            {
                return;
            }

            if (PrimaryOutputEnabled)
            {
                //This block shifts the video memory up the required number of lines.
                if (offset == cols*rows)
                {
                    char* vidMemPtr_Old = vidMemBasePtr;
                    char* vidMemPtr_New = vidMemBasePtr + cols;
                    char* maxVidMemPtr = vidMemBasePtr + cols*rows;
                    while (vidMemPtr_New < maxVidMemPtr)
                    {
                        vidMemPtr_Old[0] = vidMemPtr_New[0];
                        vidMemPtr_Old++;
                        vidMemPtr_New++;
                    }
                    offset -= cols;
                }
            }

            //This outputs the string
            Write(str);

            if (PrimaryOutputEnabled)
            {
                //This block "writes" the new line by filling in the remainder (if any) of the
                //  line with blank characters and correct background colour. 
                int diff = offset;
                while (diff > cols)
                {
                    diff -= cols;
                }
                diff = cols - diff;

                char* vidMemPtr = vidMemBasePtr + offset;
                while (diff > 0)
                {
                    vidMemPtr[0] = bg_colour;

                    diff--;
                    vidMemPtr++;
                    offset++;
                }
            }

            if (SecondaryOutput != null && SecondaryOutputEnabled)
            {
                SecondaryOutput("\r\n");
            }
        }

        /// <summary>
        ///     Writes a blank line (line with a space).
        /// </summary>
        [NoDebug]
        [NoGC]
        public static void WriteLine()
        {
            if (!Initialised) return;
            //We must write at least 1 character, so we just write a space since that
            //  is any empty character.
            WriteLine(" ");
        }

        /// <summary>
        ///     Prints the test string (all the keyboard characters) to the start of the output - overwrites any existing text.
        /// </summary>
        [NoGC]
        public static void PrintTestString()
        {
            if (!Initialised) return;
            //This does not use the Write functions as it is a test function to 
            //  test that strings and the video memory output work.

            String str =
                "1234567890!\"£$%^&*()qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM[];'#,./{}:@~<>?\\|`¬¦";
            int strLength = str.length;
            char* strPtr = str.GetCharPointer();
            char* vidMemPtr = vidMemBasePtr;
            while (strLength > 0)
            {
                vidMemPtr[0] = (char)((*strPtr & 0x00FF) | colour);

                strPtr++;
                vidMemPtr++;
                strLength--;
            }
        }

        /// <summary>
        ///     Synchronous processing delay.
        /// </summary>
        /// <param name="amount">The amount of time to delay (approx. 1 = 1 second).</param>
        public static void DelayOutput(int amount)
        {
            if (!Initialised) return;
            /*********************************************
             * DO NOT CHANGE THIS IMPLEMENTATION TO USE  *
             *    Devices.Timer.Wait/WaitNS functions!   *
             *********************************************/

            if (DisableDelayOutput)
            {
                return;
            }

            if (PrimaryOutputEnabled)
            {
                bool SecondaryOutputWasEnabled = SecondaryOutputEnabled;
                SecondaryOutputEnabled = false;

                //This method prints "." ".." "..." and so on until 
                //  ".........." (or some other length) is printed and
                //  then it resets the line to blank and repeats. Thus, 
                //  it creates a waiting bar.

                WriteLine();
                int a = 0;
                amount *= 5000000;
                for (int i = 0; i < amount; i++)
                {
                    if (i%500000 == 0)
                    {
                        if (a == 10)
                        {
                            a = 0;
                            offset -= 10;
                            Write("          ");
                            offset -= 10;
                        }
                        Write(".");
                        a++;
                    }
                }
                offset -= a;
                for (int i = 0; i < a; i++)
                {
                    Write(" ");
                }
                offset -= a;
                offset -= 80;

                SecondaryOutputEnabled = SecondaryOutputWasEnabled;
            }
        }

        public static void DumpMemory(void* ptr, int size)
        {
            if (!Initialised) return;
            uint* uPtr = (uint*)ptr;
            if (size%4 != 0)
            {
                size += 3;
            }
            size /= 4;
            for (int i = 0; i < size; i++)
            {
                Write(*(uPtr + i));
                Write(" ");
            }
            WriteLine();
        }
    }
}