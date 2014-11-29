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
