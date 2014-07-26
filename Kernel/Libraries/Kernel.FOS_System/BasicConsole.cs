#region Copyright Notice
/// ------------------------------------------------------------------------------ ///
///                                                                                ///
///               All contents copyright � Edward Nutting 2014                     ///
///                                                                                ///
///        You may not share, reuse, redistribute or otherwise use the             ///
///        contents this file outside of the Fling OS project without              ///
///        the express permission of Edward Nutting or other copyright             ///
///        holder. Any changes (including but not limited to additions,            ///
///        edits or subtractions) made to or from this document are not            ///
///        your copyright. They are the copyright of the main copyright            ///
///        holder for all Fling OS files. At the time of writing, this             ///
///        owner was Edward Nutting. To be clear, owner(s) do not include          ///
///        developers, contributors or other project members.                      ///
///                                                                                ///
/// ------------------------------------------------------------------------------ ///
#endregion
    
using System;

namespace Kernel
{
    /// <summary>
    /// A basic console implementation - uses the BIOS's fixed text-video memory to output ASCII text.
    /// </summary>
    public static unsafe class BasicConsole
    {
        /// <summary>
        /// The offset from the start of the memory (in characters) to write the next character to.
        /// </summary>
        static int offset = 0;
        /// <summary>
        /// The offset from the start of the memory (in characters) to write the next character to.
        /// </summary>
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
        [Compiler.NoGC]
        public static void Init()
        {
            bg_colour = (char)0x0000;
            default_colour = (char)0x0F00;
            warning_colour = (char)0x0E00;
            error_colour = (char)0x0400;

            colour = (char)(bg_colour | default_colour);
        }

        /// <summary>
        /// Sets the console text colour.
        /// </summary>
        /// <param name="aText_colour">The text colour to use.</param>
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static void SetTextColour(char aText_colour)
        {
            colour = (char)(bg_colour | (aText_colour & 0x0F00));
        }
        /// <summary>
        /// Sets the console background colour.
        /// </summary>
        /// <param name="aBg_colour">The background colour to use.</param>
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static void SetBgColour(char aBg_colour)
        {
            bg_colour = (char)(aBg_colour & 0xF000);
            colour = (char)(bg_colour | (colour & 0x0F00));
        }

        /// <summary>
        /// Clears the output to all black.
        /// </summary>
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static unsafe void Clear()
        {
            int numToClear = rows * cols;
            char* vidMemPtr = vidMemBasePtr;
            while (numToClear > 0)
            {
                vidMemPtr[0] = bg_colour;
                vidMemPtr++;
                numToClear--;
            }

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
        [Compiler.NoGC]
        public static unsafe void Write(FOS_System.String str)
        {
            if (str == null)
            {
                return;
            }

            int strLength = str.length;
            int maxOffset = rows * cols;

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
        [Compiler.NoGC]
        public static unsafe void WriteLine(FOS_System.String str)
        {
            if (str == null)
            {
                return;
            }

            if (offset == BasicConsole.cols * BasicConsole.rows)
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
            
            Write(str);
            
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
        [Compiler.NoGC]
        public static void WriteLine()
        {
            WriteLine(" ");
        }

        /// <summary>
        /// Prints the test string (all the keyboard characters) to the start of the output - overwrites any existing text.
        /// </summary>
        [Compiler.NoGC]
        public static unsafe void PrintTestString()
        {
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
            if (DisableDelayOutput)
            {
                return;
            }

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
    }
}
