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
        protected ELFSectionHeader header;
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
            else if (header.SectionType == ElfSectionTypes.SymTab)
            {
                return new ELFSymbolTableSection(header);
            }
            else if (header.SectionType == ElfSectionTypes.Rel)
            {
                return new ELFRelocationTableSection(header);
            }
            else if (header.SectionType == ElfSectionTypes.RelA)
            {
                return new ELFRelocationAddendTableSection(header);
            }

            return new ELFSection(header);
        }
    }
    public class ELFStringTableSection : ELFSection
    {
        protected List strings = new List();
        public List Strings
        {
            get
            {
                return strings;
            }
        }
        protected UInt32Dictionary indexTranslations = new UInt32Dictionary();
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
    public unsafe class ELFSymbolTableSection : ELFSection
    {
        public enum SymbolBinding : byte
        {
            Local = 0,
            Global = 1,
            Weak = 2
        }
        public enum SymbolType : byte
        {
            NoType = 0,
            Object = 1,
            Func = 2,
            Section = 3,
            File = 4
        }
        public unsafe class Symbol : FOS_System.Object
        {
            //Interpreted from Info field
            public SymbolBinding Binding
            {
                get
                {
                    return (SymbolBinding)(Info >> 4);
                }
            }
            public SymbolType Type
            {
                get
                {
                    return (SymbolType)(Info & 0xF);
                }
            }

            public uint NameIdx;
            public byte* Value;
            public uint Size;
            public byte Info;
            public byte Other;
            public ushort SectionIndex;
        }

        private List symbols = new List();
        public List Symbols
        {
            get
            {
                return symbols;
            }
        }

        public int StringsSectionIndex
        {
            get
            {
                return (int)header.Link;
            }
        }

        public ELFSymbolTableSection(ELFSectionHeader header)
            : base(header)
        {
        }

        public override int Read(FileStream stream)
        {
            symbols.Empty();

            int bytesRead = base.Read(stream);
            uint offset = 0;
            Symbol currSymbol;
            while (offset < bytesRead)
            {
                currSymbol = new Symbol();

                currSymbol.NameIdx = ByteConverter.ToUInt32(data, offset + 0);
                currSymbol.Value = (byte*)ByteConverter.ToUInt32(data, offset + 4);
                currSymbol.Size = ByteConverter.ToUInt32(data, offset + 8);
                currSymbol.Info = data[offset + 12];
                currSymbol.Other = data[offset + 13];
                currSymbol.SectionIndex = ByteConverter.ToUInt16(data, offset + 14);

                symbols.Add(currSymbol);

                offset += header.EntrySize;
            }

            return symbols.Count;
        }

        public Symbol this[uint index]
        {
            get
            {
                return (Symbol)symbols[(int)index];
            }
        }
    }
    public unsafe class ELFRelocationTableSection : ELFSection
    {
        public unsafe class Relocation : FOS_System.Object
        {
            //Interpreted from Info field
            public uint Symbol
            {
                get
                {
                    return (Info >> 8);
                }
            }
            public byte Type
            {
                get
                {
                    return (byte)(Info & 0xFF);
                }
            }

            public uint Info;
            public byte* Offset;
        }

        protected List relocations = new List();
        public List Relocations
        {
            get
            {
                return relocations;
            }
        }

        public int SymbolTableSectionIndex
        {
            get
            {
                return (int)header.Link;
            }
        }
        public int SectionToRelocateIndex
        {
            get
            {
                return (int)header.Info;
            }
        }

        public ELFRelocationTableSection(ELFSectionHeader header)
            : base(header)
        {
        }

        public override int Read(FileStream stream)
        {
            relocations.Empty();

            int bytesRead = base.Read(stream);
            uint offset = 0;
            Relocation currRelocation;
            while (offset < bytesRead)
            {
                currRelocation = new Relocation();

                currRelocation.Offset = (byte*)ByteConverter.ToUInt32(data, offset + 0);
                currRelocation.Info = ByteConverter.ToUInt32(data, offset + 4);
                
                relocations.Add(currRelocation);

                offset += header.EntrySize;
            }

            return relocations.Count;
        }

        public Relocation this[uint index]
        {
            get
            {
                return (Relocation)Relocations[(int)index];
            }
        }
    }
    public unsafe class ELFRelocationAddendTableSection : ELFSection
    {
        public unsafe class RelocationAddend : FOS_System.Object
        {
            //Interpreted from Info field
            public uint Symbol
            {
                get
                {
                    return (Info >> 8);
                }
            }
            public byte Type
            {
                get
                {
                    return (byte)(Info & 0xFF);
                }
            }

            public uint Info;
            public byte* Offset;
            public short Addend;
        }

        protected List relocations = new List();
        public List Relocations
        {
            get
            {
                return relocations;
            }
        }

        public int SymbolTableSectionIndex
        {
            get
            {
                return (int)header.Link;
            }
        }
        public int SectionToRelocateIndex
        {
            get
            {
                return (int)header.Info;
            }
        }
        
        public ELFRelocationAddendTableSection(ELFSectionHeader header)
            : base(header)
        {
        }

        public override int Read(FileStream stream)
        {
            relocations.Empty();

            int bytesRead = base.Read(stream);
            uint offset = 0;
            RelocationAddend currRelocation;
            while (offset < bytesRead)
            {
                currRelocation = new RelocationAddend();

                currRelocation.Offset = (byte*)ByteConverter.ToUInt32(data, offset + 0);
                currRelocation.Info = ByteConverter.ToUInt32(data, offset + 4);
                currRelocation.Addend = (short)ByteConverter.ToUInt16(data, offset + 8);

                relocations.Add(currRelocation);

                offset += header.EntrySize;
            }

            return relocations.Count;
        }

        public RelocationAddend this[uint index]
        {
            get
            {
                return (RelocationAddend)Relocations[(int)index];
            }
        }

    }
}
