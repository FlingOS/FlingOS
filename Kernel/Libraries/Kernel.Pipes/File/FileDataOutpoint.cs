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
    public class FileDataOutpoint : BasicOutpoint
    {
        /// <summary>
        ///     Creates and registers an outpoint as either a Standard In or Standard Out pipe outpoint.
        /// </summary>
        public FileDataOutpoint(int MaxConnections, bool OutputPipe)
            : base(
                PipeClasses.File, OutputPipe ? PipeSubclasses.File_Data_Out : PipeSubclasses.File_Data_In,
                MaxConnections)
        {
        }

        //TODO: Appropriate functions

        public unsafe void WriteFSInfos(int PipeId, String[] FSPrefixes)
        {
            byte[] buffer = new byte[sizeof(FilePipeDataHeader) + sizeof(FilePipeDataFSInfo)*FSPrefixes.Length];
            FilePipeDataHeader* HdrPtr =
                (FilePipeDataHeader*)((byte*)ObjectUtilities.GetHandle(buffer) + Array.FieldsBytesSize);
            HdrPtr->Count = FSPrefixes.Length;
            FilePipeDataFSInfo* DataPtr =
                (FilePipeDataFSInfo*)
                    ((byte*)ObjectUtilities.GetHandle(buffer) + Array.FieldsBytesSize + sizeof(FilePipeDataHeader));
            for (int i = 0; i < FSPrefixes.Length; i++)
            {
                if (FSPrefixes[i].Length > 10)
                {
                    BasicConsole.WriteLine(
                        "FileDataOutpoint.WriteFSInfo > Error! FS prefix longer than maximum transmittable length (10).");
                }
                for (int j = 0; j < 10; j++)
                {
                    DataPtr[i].Prefix[j] = j < FSPrefixes[i].Length ? FSPrefixes[i][j] : '\0';
                }
            }
            Write(PipeId, buffer, 0, buffer.Length, true);
        }

        public void WriteString(int PipeId, String str)
        {
            if (str == "")
            {
                str = "\0";
            }
            byte[] strBytes = ByteConverter.GetASCIIBytes(str);
            Write(PipeId, strBytes, 0, strBytes.Length, true);
        }
    }
}