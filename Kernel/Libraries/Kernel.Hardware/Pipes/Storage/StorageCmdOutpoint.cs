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
using Kernel.Hardware.Processes;
using Kernel.FOS_System;
using Kernel.FOS_System.Processes;
using Kernel.FOS_System.Processes.Requests.Pipes;

namespace Kernel.Pipes.Storage
{
    /// <summary>
    /// Represents an outpoint for a standard in or standard out pipe.
    /// </summary>
    public class StorageCmdOutpoint : BasicOutpoint
    {
        /// <summary>
        /// Creates and registers a storage command outpoint.
        /// </summary>
        public StorageCmdOutpoint(int MaxConnections)
            : base(PipeClasses.Storage, PipeSubclasses.Storage_Command, MaxConnections)
        {
        }

        private unsafe void WriteCommand(int PipeId, StorageCommands Command, ulong DiskId, ulong BlockNo, uint BlockCount)
        {
            byte[] buffer = new byte[sizeof(StoragePipeCommand)];
            StoragePipeCommand* CmdPtr = (StoragePipeCommand*)((byte*)Utilities.ObjectUtilities.GetHandle(buffer) + FOS_System.Array.FieldsBytesSize);
            CmdPtr->Command = (int)Command;
            CmdPtr->DiskId = DiskId;
            CmdPtr->BlockNo = BlockNo;
            CmdPtr->BlockCount = BlockCount;
            base.Write(PipeId, buffer, 0, buffer.Length, true);
        }
        public unsafe void Send_DiskList(int PipeId)
        {
            WriteCommand(PipeId, StorageCommands.DiskList, 0, 0, 0);
        }
        public unsafe void Send_Read(int PipeId, ulong DiskId, ulong BlockNo, uint BlockCount)
        {
            WriteCommand(PipeId, StorageCommands.Read, DiskId, BlockNo, BlockCount);
        }
        public unsafe void Send_BlockSize(int PipeId, ulong DiskId)
        {
            WriteCommand(PipeId, StorageCommands.BlockSize, DiskId, 0, 0);
        }
    }
}
