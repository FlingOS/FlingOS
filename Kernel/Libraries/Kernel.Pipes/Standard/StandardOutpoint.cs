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
using Kernel.Framework.Processes.Requests.Pipes;

namespace Kernel.Pipes.Standard
{
    /// <summary>
    ///     Represents an outpoint for a standard in or standard out pipe.
    /// </summary>
    public class StandardOutpoint : BasicOutpoint
    {
        /// <summary>
        ///     Creates and registers an outpoint as either a Standard In or Standard Out pipe outpoint.
        /// </summary>
        /// <param name="OutputPipe">Whether the outpoint is for a Standard Out pipe or (if not) a Standard In pipe.</param>
        public StandardOutpoint(bool OutputPipe)
            : base(
                PipeClasses.Standard, OutputPipe ? PipeSubclasses.Standard_Out : PipeSubclasses.Standard_In,
                PipeConstants.UnlimitedConnections)
        {
        }

        /// <summary>
        ///     Writes a character to the pipe.
        /// </summary>
        /// <param name="PipeId">The Id of the pipe to write to.</param>
        /// <param name="Character">The character to write.</param>
        /// <param name="Blocking">Whether the write call should be blocking or not.</param>
        /// <remarks>
        ///     <para>
        ///         Id required since an outpoint can be connected to multiple pipes.
        ///     </para>
        ///     <para>
        ///         Treats the character as a single ASCII byte. In future, may want to make this UTF16 (two bytes, Unicode).
        ///     </para>
        /// </remarks>
        public void Write(int PipeId, char Character, bool Blocking)
        {
            byte[] data = new byte[1] {(byte)Character};
            base.Write(PipeId, data, 0, data.Length, Blocking);
        }

        /// <summary>
        ///     Writes a message to the pipe.
        /// </summary>
        /// <param name="PipeId">The Id of the pipe to write to.</param>
        /// <param name="Message">The message to write.</param>
        /// <param name="Blocking">Whether the write call should be blocking or not.</param>
        /// <remarks>
        ///     <para>
        ///         Id required since an outpoint can be connected to multiple pipes.
        ///     </para>
        ///     <para>
        ///         Treats the message as ASCII. In future, may want to make this UTF16 (two bytes, Unicode).
        ///     </para>
        /// </remarks>
        public void Write(int PipeId, String Message, bool Blocking)
        {
            if (Message == "")
            {
                Message = "\0";
            }
            byte[] data = ByteConverter.GetASCIIBytes(Message);
            base.Write(PipeId, data, 0, data.Length, Blocking);
        }

        /// <summary>
        ///     Writes a message to the pipe followed by a new line character.
        /// </summary>
        /// <param name="PipeId">The Id of the pipe to write to.</param>
        /// <param name="Message">The message to write.</param>
        /// <param name="Blocking">Whether the write call should be blocking or not.</param>
        /// <remarks>
        ///     <para>
        ///         Id required since an outpoint can be connected to multiple pipes.
        ///     </para>
        ///     <para>
        ///         Treats the message as ASCII. In future, may want to make this UTF16 (two bytes, Unicode).
        ///     </para>
        /// </remarks>
        public void WriteLine(int PipeId, String Message, bool Blocking)
        {
            if (Message == "")
            {
                Message = "\0";
            }
            byte[] data = ByteConverter.GetASCIIBytes(Message);
            base.Write(PipeId, data, 0, data.Length, Blocking);
            data[0] = (byte)'\n';
            base.Write(PipeId, data, 0, 1, Blocking);
        }
    }
}