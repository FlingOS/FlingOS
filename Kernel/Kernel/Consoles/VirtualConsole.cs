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
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Consoles
{
    public class VirtualConsole : Console
    {
        protected Pipes.Standard.StandardOutpoint StdOut;
        protected int StdOutPipeId;

        public void Connect()
        {
            StdOut = new Pipes.Standard.StandardOutpoint(true);
            StdOutPipeId = StdOut.WaitForConnect();
        }

        public override void Clear()
        {
            StdOut.Write(StdOutPipeId, 
                //2000 spaces to clear out the screen
                "                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                ", 
                true);
        }

        public override void Update()
        {
            //Don't think we need to do anything here
        }

        public override void Write(FOS_System.String str)
        {
            StdOut.Write(StdOutPipeId, str, true);
        }
        public override void WriteLine()
        {
            StdOut.Write(StdOutPipeId, "\n", true);
        }

        /// <summary>
        /// Gets the offset from the current character to where the cursor should be displayed.
        /// </summary>
        /// <returns>The offset to be subtracted.</returns>
        protected override int GetDisplayOffset_Char()
        {
            return -ScreenStartLineOffset;
        }
        /// <summary>
        /// Gets the offset from the current line to where the cursor should be displayed.
        /// </summary>
        /// <returns>The offset to be subtracted.</returns>
        protected override int GetDisplayOffset_Line()
        {
            //Creates a fixed-position cursor on line 24 (the bottom line of the screen in 25-line
            //  VGA text-mode)
            return CurrentLine - (ScreenStartLine + ScreenHeightInLines - 1);
        }

        public override void SetCursorPosition(ushort character, ushort line)
        {
            //TODO
        }
    }
}
