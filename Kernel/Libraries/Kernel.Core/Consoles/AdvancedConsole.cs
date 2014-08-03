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
    public unsafe class AdvancedConsole : Console
    {
        /// <summary>
        /// A pointer to the start of the (character-based) video memory.
        /// </summary>
        public static char* vidMemBasePtr = (char*)0xB8000;

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
                    vidMemPtr[j] = (char)0;
                }
                vidMemPtr -= LineLength;
            }
        }

        protected override int GetDisplayOffset_Char()
        {
            return 0;
        }
        protected override int GetDisplayOffset_Line()
        {
            return 0;
        }

        public override void SetCursorPosition(ushort character, ushort line)
        {
            ushort offset = (ushort)((line * LineLength) + character);
            CursorCmdPort.Write((byte)14);
            CursorDataPort.Write((byte)(offset >> 8));
            CursorCmdPort.Write((byte)15);
            CursorDataPort.Write((byte)(offset));
        }
    }
}
