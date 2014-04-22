using System;

namespace Kernel
{
    /// <summary>
    /// A basic console implementation - uses the BIOS's fixed text-video memory to output ASCII text.
    /// </summary>
    public static class BasicConsole
    {
        /// <summary>
        /// The offset from the start of the memory (in characters) to write the next character to.
        /// </summary>
        static int offset = 0;
        
        /// <summary>
        /// Clears the output to all black.
        /// </summary>
        [Compiler.NoDebug]
        [Compiler.NoGC]
        public static unsafe void Clear()
        {
            //Rows = 25, Cols = 80
            int numToClear = 25 * 80;
            char* vidMemPtr = (char*)0xB8000;
            while (numToClear > 0)
            {
                vidMemPtr[0] = (char)(0x0000);
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
            int maxOffset = 80 * 25;

            if(offset + strLength > maxOffset)
            {
                int linesToShift = 1;
                offset -= 80;
                int strLengthCpy = strLength;

                while (offset + strLengthCpy > maxOffset)
                {
                    strLengthCpy -= 80;
                    offset -= 80;
                    linesToShift++;
                }

                char* vidMemPtr_Old = (char*)(0xB8000);
                char* vidMemPtr_New = (char*)(0xB8000 + (linesToShift * 80 * 2));
                char* maxVidMemPtr = (char*)(0xB8000 + (80 * 50));
                while(vidMemPtr_New < maxVidMemPtr)
                {
                    vidMemPtr_Old[0] = vidMemPtr_New[0];
                    vidMemPtr_Old++;
                    vidMemPtr_New++;
                }
                while (vidMemPtr_Old < maxVidMemPtr)
                {
                    vidMemPtr_Old[0] = (char)0x0000;
                    vidMemPtr_Old++;
                }
            }
            
            char* vidMemPtr = (char*)(0xB8000 + (offset * 2));
            byte* strPtr = FOS_System.String.GetBytePointer(str);
            while (strLength > 0)
            {
                vidMemPtr[0] = (char)(*strPtr | 0x0200);

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
            Write(str);

            int diff = offset;
            while(diff > 80)
            {
                diff -= 80;
            }
            diff = 80 - diff;

            char* vidMemPtr = (char*)(0xB8000 + (offset * 2));
            while (diff > 0)
            {
                vidMemPtr[0] = (char)(0x0000);

                diff--;
                vidMemPtr++;
                offset++;
            }
        }
        
        /// <summary>
        /// Prints the test string (all the keyboard characters) to the start of the output - overwrites any existing text.
        /// </summary>
        [Compiler.NoGC]
        public static unsafe void PrintTestString()
        {
            string str = "1234567890!\"£$%^&*()qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM[];'#,./{}:@~<>?\\|`¬¦";
            int strLength = FOS_System.String.GetLength(str);
            byte* strPtr = FOS_System.String.GetBytePointer(str);
            char* vidMemPtr = (char*)0xB8000;
            while (strLength > 0)
            {
                vidMemPtr[0] = (char)(*strPtr | 0x0200);

                strPtr++;
                vidMemPtr++;
                strLength--;
            }
        }
    }
}
