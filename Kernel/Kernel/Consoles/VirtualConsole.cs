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

using Kernel.Framework;
using Kernel.Pipes.Standard;

namespace Kernel.Consoles
{
    /// <summary>
    ///     Implements the more advanced Console class. This is used by processes to handle outputting to the Window Manager
    ///     Task.
    /// </summary>
    public class VirtualConsole : Console
    {
        /// <summary>
        ///     The output pipe to the Window Manager Task.
        /// </summary>
        protected StandardOutpoint StdOut;

        /// <summary>
        ///     The Id of the main output pipe.
        /// </summary>
        protected int StdOutPipeId;

        protected uint StdOutRemoteProcessId;

        public VirtualConsole()
            : base(0, 0)
        {
        }

        /// <summary>
        ///     Creates the output pipe and waits for the Window Manager to connect.
        /// </summary>
        public void Connect()
        {
            StdOut = new StandardOutpoint(true);
            StdOutPipeId = StdOut.WaitForConnect(out StdOutRemoteProcessId);
        }

        /// <summary>
        ///     Clears the screen (currently by outputting 25 new lines, meaning scrolling back is still possible).
        /// </summary>
        public override void Clear()
        {
            //25 new lines clear out the screen
            StdOut.Write(StdOutPipeId, "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n", true);
        }

        /// <summary>
        ///     Required override. Does nothing at the moment.
        /// </summary>
        public override void Update()
        {
            //Don't think we need to do anything here
        }

        /// <summary>
        ///     Writes the specified text to the output.
        /// </summary>
        /// <param name="str">The string to write.</param>
        public override void Write(String str)
        {
            StdOut.Write(StdOutPipeId, str, true);
        }

        /// <summary>
        ///     Writes a new line to the output.
        /// </summary>
        public override void WriteLine()
        {
            StdOut.Write(StdOutPipeId, "\n", true);
        }

        /// <summary>
        ///     Meaningless for a virtual console. Always returns 0.
        /// </summary>
        /// <returns>The offset to be subtracted.</returns>
        protected override int GetDisplayOffset_Char()
        {
            return 0;
        }

        /// <summary>
        ///     Meaningless for a virtual console. Always returns 0.
        /// </summary>
        /// <returns>The offset to be subtracted.</returns>
        protected override int GetDisplayOffset_Line()
        {
            return 0;
        }

        /// <summary>
        ///     Currently does not.
        /// </summary>
        /// <remarks>
        ///     This requires implementation. Need to use messages to command the Window Manager.
        /// </remarks>
        /// <param name="character">The offset from the start of the line to the cursor.</param>
        /// <param name="line">The line number (from the display perspective, not the buffer perspective) of the cursor.</param>
        public override void SetCursorPosition(ushort character, ushort line)
        {
            //TODO: VirtualConsole cannot SetCursorPosition. Requires Task to Window Manager command system. Links to the Scrolling issue.
        }
    }
}