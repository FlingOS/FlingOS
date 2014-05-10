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
        public static void SetTextColour(char aText_colour)
        {
            colour = (char)(bg_colour | (aText_colour & 0x0F00));
        }
        /// <summary>
        /// Sets the console background colour.
        /// </summary>
        /// <param name="aBg_colour">The background colour to use.</param>
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
        public static unsafe void Write(string str)
        {
            int strLength = FOS_System.String.GetLength(str);
            int maxOffset = rows * cols;

            if(offset + strLength > maxOffset)
            {
                int amountToShift = (offset + strLength) - maxOffset;
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
            char* strPtr = FOS_System.String.GetCharPointer(str);
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
        public static unsafe void WriteLine(string str)
        {
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
            string str = "1234567890!\"£$%^&*()qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM[];'#,./{}:@~<>?\\|`¬¦";
            int strLength = FOS_System.String.GetLength(str);
            char* strPtr = FOS_System.String.GetCharPointer(str);
            char* vidMemPtr = vidMemBasePtr;
            while (strLength > 0)
            {
                vidMemPtr[0] = (char)((*strPtr & 0x00FF) | colour);

                strPtr++;
                vidMemPtr++;
                strLength--;
            }
        }
    }
}
