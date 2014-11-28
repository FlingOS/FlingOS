using Kernel.FOS_System;
using Kernel.FOS_System.Collections;
using Kernel.FOS_System.IO;
using Kernel.FOS_System.IO.Streams;

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

        protected byte[] data;

        protected ELFSection(ELFSectionHeader aHeader)
        {
            header = aHeader;
        }

        public virtual int Read(FileStream stream)
        {
            data = new byte[header.SectionSize];
            stream.Position = header.SectionFileOffset;
            int bytesRead = stream.Read(data, 0, data.Length);
            if (bytesRead != data.Length)
            {
                ExceptionMethods.Throw(new FOS_System.Exception("Failed to read section data from file!"));
            }
            return bytesRead;
        }

        public static ELFSection GetSection(ELFSectionHeader header)
        {
            if (header.SectionType == ElfSectionTypes.StrTab)
            {
                return new ELFStringTableSection(header);
            }

            return new ELFSection(header);
        }
    }
    public class ELFStringTableSection : ELFSection
    {
        private List strings = new List();
        private UInt32Dictionary indexTranslations = new UInt32Dictionary();
        public List Strings
        {
            get
            {
                return strings;
            }
        }
        public UInt32Dictionary IndexTranslations
        {
            get
            {
                return indexTranslations;
            }
        }

        public ELFStringTableSection(ELFSectionHeader header)
            : base(header)
        {
        }

        public override int Read(FileStream stream)
        {
            strings.Empty();

            int bytesRead = base.Read(stream);
            uint offset = 0;
            FOS_System.String currString;
            while (offset < bytesRead)
            {
                currString = ByteConverter.GetASCIIStringFromASCII(data, offset, (uint)(bytesRead - offset));
                indexTranslations.Add(offset, (uint)strings.Count);

                offset += (uint)currString.length + 1; //+1 for null terminator
                strings.Add(currString);
            }

            return strings.Count;
        }

        public FOS_System.String this[uint index]
        {
            get
            {
                return (FOS_System.String)strings[(int)indexTranslations[index]];
            }
        }
    }
}
