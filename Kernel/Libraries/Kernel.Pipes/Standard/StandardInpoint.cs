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
    ///     Represents an inpoint for a standard in or standard out pipe.
    /// </summary>
    public class StandardInpoint : BasicInpoint
    {
        /// <summary>
        ///     The buffer to use when reading strings from the pipe.
        /// </summary>
        protected byte[] ReadBuffer;

        /// <summary>
        ///     Creates and connects a new standard pipe to the target process as either a Standard In or Standard Out pipe.
        /// </summary>
        /// <param name="aOutProcessId">The target process Id.</param>
        /// <param name="OutputPipe">Whether the pipe is a Standard In or Standard Out pipe.</param>
        public StandardInpoint(uint aOutProcessId, bool OutputPipe)
            : base(
                aOutProcessId, PipeClasses.Standard,
                OutputPipe ? PipeSubclasses.Standard_Out : PipeSubclasses.Standard_In, 800)
        {
            ReadBuffer = new byte[BufferSize];
        }

        /// <summary>
        ///     Reads as much available data from the pipe as possible and interprets it as an ASCII string.
        /// </summary>
        /// <param name="blocking">Whether the read call should be blocking or not.</param>
        /// <returns>The string or empty string if the number of bytes read was zero.</returns>
        public String Read(bool blocking)
        {
            int bytesRead = base.Read(ReadBuffer, 0, ReadBuffer.Length, blocking);
            if (bytesRead > 0)
            {
                return ByteConverter.GetASCIIStringFromASCII(ReadBuffer, 0, (uint)bytesRead);
            }
            return "";
        }
    }
}