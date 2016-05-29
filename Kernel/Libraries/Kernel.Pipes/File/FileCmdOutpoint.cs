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
    ///     Represents an outpoint for a standard in or standard out pipe.
    /// </summary>
    public class FileCmdOutpoint : BasicOutpoint
    {
        /// <summary>
        ///     Creates and registers a storage command outpoint.
        /// </summary>
        public FileCmdOutpoint(int MaxConnections)
            : base(PipeClasses.File, PipeSubclasses.File_Command, MaxConnections)
        {
        }

        private unsafe void WriteCommand(int PipeId, FileCommands Command, ulong DiskId, ulong FileHandle, ulong BlockNo,
            uint BlockCount)
        {
            byte[] buffer = new byte[sizeof(FilePipeCommand)];
            FilePipeCommand* CmdPtr =
                (FilePipeCommand*)((byte*)ObjectUtilities.GetHandle(buffer) + Array.FieldsBytesSize);
            CmdPtr->Command = (int)Command;
            CmdPtr->DiskId = DiskId;
            CmdPtr->BlockNo = BlockNo;
            CmdPtr->BlockCount = BlockCount;
            Write(PipeId, buffer, 0, buffer.Length, true);
        }

        public void Send_StatFS(int PipeId)
        {
            WriteCommand(PipeId, FileCommands.StatFS, 0, 0, 0, 0);
        }

        public void Write_ListDir(int PipeId)
        {
            WriteCommand(PipeId, FileCommands.ListDir, 0, 0, 0, 0);
        }

        //public unsafe void Send_Read(int PipeId, ulong FileHandle, ulong BlockNo, uint BlockCount)
        //{
        //    WriteCommand(PipeId, FileCommands.Read, 0, FileHandle, BlockNo, BlockCount);

        //}
        //public unsafe void Send_Write(int PipeId, ulong FileHandle, ulong BlockNo, uint BlockCount)
        //{
        //    WriteCommand(PipeId, FileCommands.Write, 0, FileHandle, BlockNo, BlockCount);
        //}
    }
}