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

namespace Kernel.Pipes.Storage
{
    /// <summary>
    ///     Represents an outpoint for a standard in or standard out pipe.
    /// </summary>
    public class StorageDataOutpoint : BasicOutpoint
    {
        /// <summary>
        ///     Creates and registers an outpoint as either a Standard In or Standard Out pipe outpoint.
        /// </summary>
        public StorageDataOutpoint(int MaxConnections, bool OutputPipe)
            : base(
                PipeClasses.Storage, OutputPipe ? PipeSubclasses.Storage_Data_Out : PipeSubclasses.Storage_Data_In,
                MaxConnections)
        {
        }

        //TODO: Appropriate functions

        public unsafe void WriteDiskInfos(int PipeId, ulong[] DiskIds)
        {
            byte[] buffer = new byte[sizeof(StoragePipeDataHeader) + sizeof(StoragePipeDataDiskInfo)*DiskIds.Length];
            StoragePipeDataHeader* HdrPtr =
                (StoragePipeDataHeader*)((byte*)ObjectUtilities.GetHandle(buffer) + Array.FieldsBytesSize);
            HdrPtr->Count = DiskIds.Length;
            StoragePipeDataDiskInfo* DataPtr =
                (StoragePipeDataDiskInfo*)
                    ((byte*)ObjectUtilities.GetHandle(buffer) + Array.FieldsBytesSize + sizeof(StoragePipeDataHeader));
            for (int i = 0; i < DiskIds.Length; i++)
            {
                DataPtr[i].Id = DiskIds[i];
            }
            Write(PipeId, buffer, 0, buffer.Length, true);
        }
    }
}