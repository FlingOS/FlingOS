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
    
using Kernel.FOS_System;
using Kernel.FOS_System.Collections;
using Kernel.FOS_System.IO;
using Kernel.FOS_System.IO.Streams;

namespace Kernel.Core.Processes.ELF
{
    public enum ELFSegmentType : uint
    {
        Null = 0,
        Load = 1,
        Dynamic = 2,
        Interp = 3,
        Note = 4,
        SHLib = 5,
        PHDR = 6
    }
    public enum ELFFlags : uint
    {
        Executable = 1,
        Writeable = 2,
        Readable = 4
    }
    public unsafe class ELFSegmentHeader : FOS_System.Object
    {
        public ELFSegmentHeader(byte[] header, ref uint offset)
        {
            Type = (ELFSegmentType)ByteConverter.ToUInt32(header, offset);
            offset += 4;
            FileOffset = ByteConverter.ToUInt32(header, offset);
            offset += 4;
            VAddr = (byte*)ByteConverter.ToUInt32(header, offset);
            offset += 4;
            PAddr = (byte*)ByteConverter.ToUInt32(header, offset);
            offset += 4;
            FileSize = ByteConverter.ToUInt32(header, offset);
            offset += 4;
            MemSize = ByteConverter.ToUInt32(header, offset);
            offset += 4;
            Flags = (ELFFlags)ByteConverter.ToUInt32(header, offset);
            offset += 4;
            Align = ByteConverter.ToUInt32(header, offset);
            offset += 4;
        }

        public ELFSegmentType Type;
        public uint FileOffset;
        public byte* VAddr;
        public byte* PAddr;
        public uint FileSize;
        public uint MemSize;
        public ELFFlags Flags;
        public uint Align;
    }
    public unsafe class ELFSegment : FOS_System.Object
    {
        protected ELFSegmentHeader header;
        public ELFSegmentHeader Header
        {
            get
            {
                return header;
            }
        }

        protected byte[] data;
        public byte[] Data
        {
            get
            {
                return data;
            }
        }

        protected ELFSegment(ELFSegmentHeader aHeader)
        {
            header = aHeader;
        }

        public virtual int Read(FileStream stream)
        {
            data = new byte[header.FileSize];
            stream.Position = header.FileOffset;
            int bytesRead = stream.Read(data, 0, data.Length);
            if (bytesRead != data.Length)
            {
                ExceptionMethods.Throw(new FOS_System.Exception("Failed to read segment data from file!"));
            }
            return bytesRead;
        }

        public static ELFSegment GetSegment(ELFSegmentHeader header)
        {
            //if (header.Type == ElfSegmentTypes.???)
            //{
            //    return new ELF???Segment(header);
            //}

            return new ELFSegment(header);
        }
    }
}
