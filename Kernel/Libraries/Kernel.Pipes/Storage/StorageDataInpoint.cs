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
    ///     Represents an inpoint for a standard in or standard out pipe.
    /// </summary>
    public class StorageDataInpoint : BasicInpoint
    {
        /// <summary>
        ///     The buffer to use when reading strings from the pipe.
        /// </summary>
        protected byte[] ReadBuffer;

        /// <summary>
        ///     Creates and connects a new standard pipe to the target process as either a Standard In or Standard Out pipe.
        /// </summary>
        /// <param name="aOutProcessId">The target process Id.</param>
        public StorageDataInpoint(uint aOutProcessId, bool OutputPipe)
            : base(
                aOutProcessId, PipeClasses.Storage,
                OutputPipe ? PipeSubclasses.Storage_Data_Out : PipeSubclasses.Storage_Data_In, 8192)
        {
            ReadBuffer = new byte[BufferSize];
        }

        //TODO: Appropriate functions

        public unsafe ulong[] ReadDiskInfos(bool blocking)
        {
            int bytesRead = Read(ReadBuffer, 0, sizeof(StoragePipeDataHeader), blocking);
            if (bytesRead > 0)
            {
                int Count;
                {
                    StoragePipeDataHeader* HdrPtr =
                        (StoragePipeDataHeader*)((byte*)ObjectUtilities.GetHandle(ReadBuffer) + Array.FieldsBytesSize);
                    Count = HdrPtr->Count;
                }

                StoragePipeDataDiskInfo* DataPtr =
                    (StoragePipeDataDiskInfo*)((byte*)ObjectUtilities.GetHandle(ReadBuffer) + Array.FieldsBytesSize);

                ulong[] result = new ulong[Count];
                for (int i = 0; i < Count; i++)
                {
                    bytesRead = Read(ReadBuffer, 0, sizeof(StoragePipeDataDiskInfo), blocking);
                    if (bytesRead <= 0)
                    {
                        BasicConsole.WriteLine(
                            "StorageDataInpoint : Error reading disk infos! Reading disk info data returned zero (or negative) byte count.");
                    }
                    else
                    {
                        result[i] = DataPtr->Id;
                    }
                }
                return result;
            }
            return new ulong[0];
        }
    }
}