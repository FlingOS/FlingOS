#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
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
    
using System;

namespace Kernel
{
    /// <summary>
    /// A basic console implementation - uses the BIOS's fixed text-video memory to output ASCII text.
    /// </summary>
    /// <remarks>
    /// This class is a very basic console. It uses the default, x86 setup for
    /// VGA text-mode graphics and simply outputs text to graphics memory.
    /// When a new line is required, it simply shifts the graphics memory up
    /// one line and discards any video memory shifted off the top of the screen.
    /// Scrolling back down is thus not possible as the information is lost.

    /// For a better console implementation see Console and AdvancedConsole classes in
    /// Kernel.Core library (/namespace)

    /// Some of the code used appears inefficient or needlessly expanded. That's 
    /// because it is. Deliberately so. The reason is that the code used uses the 
    /// minimum of IL ops and the simpler IL ops making the initial compiler work
    /// much smaller and simpler to do. It also means that if the compiler breaks
    /// in any way, the BasicConsole class is likely to still work thus making it
    /// the most useful debugging tool.

    /// All of this code has been thoroughly used and abused, which means it is 
    /// well tested i.e. reliable and robust. Do not alter the code in any way!
    /// Really, this code does not need modifying and if you do so, you're more 
    /// likely to break it than fix or improve it.

    /// This code is specifically designed for 80x25 VGA text-mode. In theory you 
    /// could change the "rows" and "cols" values, but this would actually break 
    /// the code because some of it has values of 80, 25, 160 and 50 hard-coded
    /// which you would need to change. I am reluctant to go changing these hard
    /// coded values for the reasons given in prior notes.
    /// </remarks>
    public static unsafe class BasicConsole
    {
        /// <summary>
        /// Static constructor for the Basic Console.
        /// </summary>
        /// <remarks>
        /// This constructor should, assuming the compiler hasn't changed much, be one of the first, if not
        /// the first, static constructor to be called. Almost all other static constructors rely on the
        /// Basic Console being enable, one way or another.
        /// </remarks>
        static BasicConsole()
        {
        }

        /// <summary>
        /// Used to indicate whether the Basic Console has been initialised or not.
        /// </summary>
        /// <remarks>
        /// Useful for when fixing low-level errors in compiler which result in incorrect execution order
        /// and thus use of Basic Console before it is ready.
        /// </remarks>
        public static bool Initialised = false;

        /// <summary>
        /// The offset from the start of the memory (in characters) to write the next character to.
        /// </summary>
        /// <remarks>
        /// This would cause an issue if you changed the line length after already having printed text
        /// because you'd want to leave the next print location at the start of a new line.
        /// </remarks>
        static int offset = 0;
        /// <summary>
        /// The offset from the start of the memory (in characters) to write the next character to.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public static int Offset
        {
            get
            {
                return offset;
            }
        }

        /// <summary>
        /// A pointer to the start of the (character-based) video memory.
        /// </summary>
        public static char* vidMemBasePtr = (char*)0xB8000;

        /// <summary>
        /// Numbers of rows in the video memory.
        /// </summary>
        public static int rows = 25;
        /// <summary>
        /// Number of columns in the video memory.
        /// </summary>
        public static int cols = 80;
        /// <summary>
        /// The colour to print characters in. Do not set directly, use SetTextColour/SetBackgroundColour.
        /// </summary>
        public static char colour;
        /// <summary>
        /// The background colour. Do not set directly, use SetTextColour/SetBackgroundColour.
        /// </summary>
        public static char bg_colour;
        /// <summary>
        /// Default colour to print characters in.
        /// </summary>
        public static char default_colour;
        /// <summary>
        /// Colour to print warning characters in.
        /// </summary>
        public static char warning_colour;
        /// <summary>
        /// Colour to print error characters in.
        /// </summary>
        public static char error_colour;

        /// <summary>
        /// Initialises the BasicConsole class.
        /// </summary>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Compiler.NoGC]
        [Drivers.Compiler.Attributes.NoGC]
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
        /// Sets the console text colour.
        /// </summary>
        /// <param name="aText_colour">The text colour to use.</param>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Compiler.NoGC]
        [Drivers.Compiler.Attributes.NoGC]
        public static void SetTextColour(char aText_colour)
        {
            if (!Initialised) return;
            colour = (char)(bg_colour | (aText_colour & 0x0F00));
        }
        /// <summary>
        /// Sets the console background colour.
        /// </summary>
        /// <param name="aBg_colour">The background colour to use.</param>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Compiler.NoGC]
        [Drivers.Compiler.Attributes.NoGC]
        public static void SetBgColour(char aBg_colour)
        {
            if (!Initialised) return;
            bg_colour = (char)(aBg_colour & 0xF000);
            colour = (char)(bg_colour | (colour & 0x0F00));
        }

        /// <summary>
        /// Clears the output to all black.
        /// </summary>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Compiler.NoGC]
        [Drivers.Compiler.Attributes.NoGC]
        public static unsafe void Clear()
        {
            if (!Initialised) return;
            //Clear out every character on the screen
            int numToClear = rows * cols;
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
        /// Writes the specified string to the output at the current offset. 
        /// </summary>
        /// <param name="str">The string to output.</param>
        /// <remarks>
        /// If necessary, this method will move all existing text up the necessary number of lines to fit the new text on the bottom 
        /// of the screen.
        /// </remarks>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Compiler.NoGC]
        [Drivers.Compiler.Attributes.NoGC]
        public static unsafe void Write(FOS_System.String str)
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

            int strLength = str.length;
            int maxOffset = rows * cols;

            //This block shifts the video memory up the required number of lines.
            if(offset + strLength > maxOffset)
            {
                int amountToShift = (offset + strLength) - maxOffset;
                amountToShift = amountToShift + (80 - (amountToShift % 80));
                offset -= amountToShift;

                char* vidMemPtr_Old = vidMemBasePtr;
                char* vidMemPtr_New = vidMemBasePtr + amountToShift;
                char* maxVidMemPtr = vidMemBasePtr + (cols * rows);
                while(vidMemPtr_New < maxVidMemPtr)
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
        /// <summary>
        /// Writes the specified string to the output at the current offset then moves the offset to the end of the line.
        /// </summary>
        /// <param name="str">The string to output.</param>
        /// <remarks>
        /// This also blanks out the rest of the line to make sure no artifacts are left behind.
        /// </remarks>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Compiler.NoGC]
        [Drivers.Compiler.Attributes.NoGC]
        public static unsafe void WriteLine(FOS_System.String str)
        {
            if (!Initialised) return;
            if (str == null)
            {
                return;
            }

            //This block shifts the video memory up the required number of lines.
            if (offset == cols * rows)
            {
                char* vidMemPtr_Old = vidMemBasePtr;
                char* vidMemPtr_New = vidMemBasePtr + cols;
                char* maxVidMemPtr = vidMemBasePtr + (cols * rows);
                while (vidMemPtr_New < maxVidMemPtr)
                {
                    vidMemPtr_Old[0] = vidMemPtr_New[0];
                    vidMemPtr_Old++;
                    vidMemPtr_New++;
                }
                offset -= cols;
            }
            
            //This outputs the string
            Write(str);
            
            //This block "writes" the new line by filling in the remainder (if any) of the
            //  line with blank characters and correct background colour. 
            int diff = offset;
            while(diff > cols)
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

        /// <summary>
        /// Writes a blank line (line with a space).
        /// </summary>
        [Compiler.NoDebug]
        [Drivers.Compiler.Attributes.NoDebug]
        [Compiler.NoGC]
        [Drivers.Compiler.Attributes.NoGC]
        public static void WriteLine()
        {
            if (!Initialised) return;
            //We must write at least 1 character, so we just write a space since that
            //  is any empty character.
            WriteLine(" ");
        }

        /// <summary>
        /// Prints the test string (all the keyboard characters) to the start of the output - overwrites any existing text.
        /// </summary>
        [Compiler.NoGC]
        [Drivers.Compiler.Attributes.NoGC]
        public static unsafe void PrintTestString()
        {
            if (!Initialised) return;
            //This does not use the Write functions as it is a test function to 
            //  test that strings and the video memory output work.

            FOS_System.String str = "1234567890!\"£$%^&*()qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM[];'#,./{}:@~<>?\\|`¬¦";
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
        /// Whether to disable the delay output method. Used when debugging is enabled.
        /// </summary>
        public static bool DisableDelayOutput = false;
        /// <summary>
        /// Synchronous processing delay.
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

            //This method prints "." ".." "..." and so on until 
            //  ".........." (or some other length) is printed and
            //  then it resets the line to blank and repeats. Thus, 
            //  it creates a waiting bar.

            WriteLine();
            int a = 0;
            amount *= 5000000;
            for (int i = 0; i < amount; i++)
            {
                if (i % 500000 == 0)
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
        }

        public static void DumpMemory(void* ptr, int size)
        {
            if (!Initialised) return;
            uint* uPtr = (uint*)ptr;
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
