using Kernel.FOS_System;

namespace Kernel.Core.Processes.ELF
{
    public enum ElfSectionTypes : uint
    {
        Null = 0,
        ProgBits = 1,
        SymTab = 2,
        StrTab = 3,
        RelA = 4,
        Hash = 5,
        Dynamic = 6,
        Note = 7,
        NoBits = 8,
        Rel = 9,
        SHLib = 10,
        DynSym = 11
    }
    public enum ELFSectionFlags : uint
    {
        Write = 0x1,
        Alloc = 0x2,
        ExecInstr = 0x4
    }
    public unsafe class ELFSectionHeader : FOS_System.Object
    {
        public ELFSectionHeader(byte[] header, ref uint offset)
        {
            NameIndex = ByteConverter.ToUInt32(header, offset);
            offset += 4;
            SectionType = (ElfSectionTypes)ByteConverter.ToUInt32(header, offset);
            offset += 4;
            Flags = (ELFSectionFlags)ByteConverter.ToUInt32(header, offset);
            offset += 4;
            LoadAddress = (byte*)ByteConverter.ToUInt32(header, offset);
            offset += 4;
            SectionFileOffset = ByteConverter.ToUInt32(header, offset);
            offset += 4;
            SectionSize = ByteConverter.ToUInt32(header, offset);
            offset += 4;
            Link = ByteConverter.ToUInt32(header, offset);
            offset += 4;
            Info = ByteConverter.ToUInt32(header, offset);
            offset += 4;
            AddressAlignment = ByteConverter.ToUInt32(header, offset);
            offset += 4;
            EntrySize = ByteConverter.ToUInt32(header, offset);
            offset += 4;
        }

        public uint NameIndex;
        public ElfSectionTypes SectionType;
        public ELFSectionFlags Flags;
        public byte* LoadAddress;
        public uint SectionFileOffset;
        public uint SectionSize;
        public uint Link;
        public uint Info;
        public uint AddressAlignment;
        public uint EntrySize;
    }
    public class ELFSection : FOS_System.Object
    {
        private ELFSectionHeader header;
        public ELFSectionHeader Header
        {
            get
            {
                return header;
            }
        }

        public ELFSection(ELFSectionHeader aHeader)
        {
            header = aHeader;
        }
    }
}
