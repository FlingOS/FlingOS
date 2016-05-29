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
    public class FileDataInpoint : BasicInpoint
    {
        /// <summary>
        ///     The buffer to use when reading strings from the pipe.
        /// </summary>
        protected byte[] ReadBuffer;

        /// <summary>
        ///     Creates and connects a new standard pipe to the target process as either a Standard In or Standard Out pipe.
        /// </summary>
        /// <param name="aOutProcessId">The target process Id.</param>
        public FileDataInpoint(uint aOutProcessId, bool OutputPipe)
            : base(
                aOutProcessId, PipeClasses.File, OutputPipe ? PipeSubclasses.File_Data_Out : PipeSubclasses.File_Data_In,
                8192)
        {
            ReadBuffer = new byte[BufferSize];
        }

        //TODO: Appropriate functions

        public unsafe String[] ReadFSInfos(bool blocking)
        {
            int bytesRead = Read(ReadBuffer, 0, sizeof(FilePipeDataHeader), blocking);
            if (bytesRead > 0)
            {
                int Count;
                {
                    FilePipeDataHeader* HdrPtr =
                        (FilePipeDataHeader*)((byte*)ObjectUtilities.GetHandle(ReadBuffer) + Array.FieldsBytesSize);
                    Count = HdrPtr->Count;
                }

                FilePipeDataFSInfo* DataPtr =
                    (FilePipeDataFSInfo*)((byte*)ObjectUtilities.GetHandle(ReadBuffer) + Array.FieldsBytesSize);

                String[] result = new String[Count];
                for (int i = 0; i < Count; i++)
                {
                    bytesRead = Read(ReadBuffer, 0, sizeof(FilePipeDataFSInfo), blocking);

                    if (bytesRead <= 0)
                    {
                        BasicConsole.WriteLine(
                            "FileDataInpoint : Error reading file system infos! Reading file system info data returned zero (or negative) byte count.");
                    }
                    else
                    {
                        int charCount = 0;
                        for (; charCount < 10; charCount++)
                        {
                            if (DataPtr->Prefix[charCount] == '\0')
                            {
                                break;
                            }
                        }

                        result[i] = String.New(charCount);
                        for (int j = 0; j < charCount; j++)
                        {
                            result[i][j] = DataPtr->Prefix[j];
                        }
                    }
                }
                return result;
            }
            return new String[0];
        }

        public String ReadString(bool blocking)
        {
            int bytesRead = Read(ReadBuffer, 0, BufferSize, blocking);
            if (bytesRead > 0)
            {
                return ByteConverter.GetASCIIStringFromASCII(ReadBuffer, 0, (uint)bytesRead);
            }
            return null;
        }
    }
}