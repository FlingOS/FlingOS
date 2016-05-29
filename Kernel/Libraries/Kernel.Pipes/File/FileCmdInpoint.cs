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
using Kernel.Utilities;

namespace Kernel.Pipes.File
{
    /// <summary>
    ///     Represents an inpoint for a standard in or standard out pipe.
    /// </summary>
    public unsafe class FileCmdInpoint : BasicInpoint
    {
        /// <summary>
        ///     The buffer to use when reading commands from the pipe.
        /// </summary>
        protected byte[] ReadBuffer;

        /// <summary>
        ///     Creates and connects a new storage command pipe to the target process.
        /// </summary>
        /// <param name="aOutProcessId">The target process Id.</param>
        public FileCmdInpoint(uint aOutProcessId)
            : base(aOutProcessId, PipeClasses.File, PipeSubclasses.File_Command, sizeof(FilePipeCommand)*20)
        {
            ReadBuffer = new byte[BufferSize];
        }

        public FilePipeCommand* Read()
        {
            int bytesRead = base.Read(ReadBuffer, 0, ReadBuffer.Length, true);
            if (bytesRead > 0)
            {
                return (FilePipeCommand*)((byte*)ObjectUtilities.GetHandle(ReadBuffer) + Array.FieldsBytesSize);
            }
            return null;
        }
    }
}