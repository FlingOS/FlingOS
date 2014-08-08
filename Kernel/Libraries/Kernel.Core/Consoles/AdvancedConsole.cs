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

namespace Kernel.Core.Consoles
{
    /// <summary>
    /// Implements the more advanced Console class. This is a more advanced alternative to the BasicConsole
    /// VGA text-mode implementation. This implementation of the Console class outputs text in VGA text-mode 
    /// directly to the VGA memory.
    /// </summary>
    public unsafe class AdvancedConsole : Console
    {
        /// <summary>
        /// The command port for manipulating the VGA text-mode cursor.
        /// </summary>
        protected Hardware.IO.IOPort CursorCmdPort = new Hardware.IO.IOPort(0x3D4);
        /// <summary>
        /// The data port for manipulating the VGA text-mode cursor.
        /// </summary>
        protected Hardware.IO.IOPort CursorDataPort = new Hardware.IO.IOPort(0x3D5);

        /// <summary>
        /// A pointer to the start of the (character-based) video memory.
        /// </summary>
        protected static char* vidMemBasePtr = (char*)0xB8000;

        /// <summary>
        /// Update the display.
        /// </summary>
        protected override void Update()
        {
            char* vidMemPtr = vidMemBasePtr + (24 * LineLength);
            for(int i = CurrentLine; i > -1 && i > CurrentLine - 25; i--)
            {
                char* cLinePtr = ((FOS_System.String)Buffer[i]).GetCharPointer();
                for (int j = 0; j < LineLength; j++)
                {
                    vidMemPtr[j] = cLinePtr[j];
                }
                vidMemPtr -= LineLength;
            }

            while(vidMemPtr >= vidMemBasePtr)
            {
                for (int j = 0; j < LineLength; j++)
                {
                    vidMemPtr[j] = (char)(' ' | CurrentAttr);
                }
                vidMemPtr -= LineLength;
            }
        }

        /// <summary>
        /// Gets the offset from the current character to where the cursor should be displayed.
        /// </summary>
        /// <returns>The offset to be subtracted.</returns>
        protected override int GetDisplayOffset_Char()
        {
            return 0;
        }
        /// <summary>
        /// Gets the offset from the current line to where the cursor should be displayed.
        /// </summary>
        /// <returns>The offset to be subtracted.</returns>
        protected override int GetDisplayOffset_Line()
        {
            //Creates a fixed-position cursor on line 24 (the bottom line of the screen in 25-line
            //  VGA text-mode)
            return CurrentLine - 24;
        }

        /// <summary>
        /// Sets the displayed position of the cursor.
        /// </summary>
        /// <param name="character">
        /// The 0-based offset from the start of a line to the character to display the cursor on.
        /// </param>
        /// <param name="line">The 0-based index of the line to display the cursor on.</param>
        public override void SetCursorPosition(ushort character, ushort line)
        {
            ushort offset = (ushort)((line * LineLength) + character);
            CursorCmdPort.Write_Byte((byte)14);
            CursorDataPort.Write_Byte((byte)(offset >> 8));
            CursorCmdPort.Write_Byte((byte)15);
            CursorDataPort.Write_Byte((byte)(offset));
        }
    }
}
