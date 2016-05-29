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

using Kernel.FileSystems.Streams;
using Kernel.Framework;
using Kernel.Framework.Collections;

namespace Kernel.Processes.ELF
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

    public unsafe class ELFSectionHeader : Object
    {
        public uint AddressAlignment;
        public uint EntrySize;
        public ELFSectionFlags Flags;
        public uint Info;
        public uint Link;
        public byte* LoadAddress;

        public uint NameIndex;
        public uint SectionFileOffset;
        public uint SectionSize;
        public ElfSectionTypes SectionType;

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
    }

    public class ELFSection : Object
    {
        protected byte[] data;
        protected ELFSectionHeader header;

        public ELFSectionHeader Header
        {
            get { return header; }
        }

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
                ExceptionMethods.Throw(new Exception("Failed to read section data from file!"));
            }
            return bytesRead;
        }

        public static ELFSection GetSection(ELFSectionHeader header)
        {
            if (header.SectionType == ElfSectionTypes.StrTab)
            {
                return new ELFStringTableSection(header);
            }
            if (header.SectionType == ElfSectionTypes.SymTab)
            {
                return new ELFSymbolTableSection(header);
            }
            if (header.SectionType == ElfSectionTypes.DynSym)
            {
                return new ELFDynamicSymbolTableSection(header);
            }
            if (header.SectionType == ElfSectionTypes.Rel)
            {
                return new ELFRelocationTableSection(header);
            }
            if (header.SectionType == ElfSectionTypes.RelA)
            {
                return new ELFRelocationAddendTableSection(header);
            }
            if (header.SectionType == ElfSectionTypes.Dynamic)
            {
                return new ELFDynamicSection(header);
            }

            return new ELFSection(header);
        }
    }

    public class ELFStringTableSection : ELFSection
    {
        public String this[uint offset]
        {
            get
            {
                String currString = "";
                if (offset < data.Length)
                {
                    currString = ByteConverter.GetASCIIStringFromASCII(data, offset, (uint)(data.Length - offset));
                }
                return currString;
            }
        }

        public ELFStringTableSection(ELFSectionHeader header)
            : base(header)
        {
        }

        public bool IsMatch(uint offset, String value)
        {
            if (value == null || (offset == data.Length && value.Length != 0))
            {
                return false;
            }

            bool result = true;

            uint i = 0;
            for (; i < value.Length && i + offset < data.Length && data[i + offset] != 0; i++)
            {
                result &= value[i] == data[i + offset];
            }

            result &= i == value.Length;

            return result;
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

        protected List symbols = new List();

        public List Symbols
        {
            get { return symbols; }
        }

        public int StringsSectionIndex
        {
            get { return (int)header.Link; }
        }

        public Symbol this[uint index]
        {
            get { return (Symbol)symbols[(int)index]; }
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

        public class Symbol : Object
        {
            public byte Info;

            public uint NameIdx;
            public byte Other;
            public ushort SectionIndex;
            public uint Size;
            public byte* Value;
            //Interpreted from Info field
            public SymbolBinding Binding
            {
                get { return (SymbolBinding)(Info >> 4); }
            }

            public SymbolType Type
            {
                get { return (SymbolType)(Info & 0xF); }
            }
        }
    }

    public class ELFDynamicSymbolTableSection : ELFSymbolTableSection
    {
        public ELFDynamicSymbolTableSection(ELFSectionHeader header)
            : base(header)
        {
        }
    }

    public unsafe class ELFRelocationTableSection : ELFSection
    {
        public enum RelocationType : byte
        {
            R_386_NONE = 0,
            R_386_32 = 1,
            R_386_PC32 = 2,
            R_386_GOT32 = 3,
            R_386_PLT32 = 4,
            R_386_COPY = 5,
            R_386_GLOB_DAT = 6,
            R_386_JMP_SLOT = 7,
            R_386_RELATIVE = 8,
            R_386_GOTOFF = 9,
            R_386_GOTPC = 10,
            R_386_32PLT = 11,
            R_386_16 = 20,
            R_386_PC16 = 21,
            R_386_8 = 22,
            R_386_PC8 = 23,
            R_386_SIZE32 = 38
        }

        protected List relocations = new List();

        public List Relocations
        {
            get { return relocations; }
        }

        public int SymbolTableSectionIndex
        {
            get { return (int)header.Link; }
        }

        public int SectionToRelocateIndex
        {
            get { return (int)header.Info; }
        }

        public Relocation this[uint index]
        {
            get { return (Relocation)Relocations[(int)index]; }
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

        public class Relocation : Object
        {
            /// <summary>
            ///     This member gives both the symbol table index with respect to which the
            ///     relocation must be made, and the type of relocation to apply. For example,
            ///     a call instruction's relocation entry would hold the symbol table index
            ///     of the function being called. If the index is STN_UNDEF, the undefined
            ///     symbol index, the relocation uses 0 as the "symbol value". Relocation
            ///     types are processor-specific; descriptions of their behavior appear in the
            ///     processor supplement.
            /// </summary>
            /// <remarks>
            ///     Quoted from http://www.scs.stanford.edu/14wi-cs140/pintos/specs/sysv-abi-update.html/ch4.reloc.html
            /// </remarks>
            public uint Info;

            /// <summary>
            ///     This member gives the location at which to apply the relocation action.
            ///     For a relocatable file, the value is the byte offset from the beginning
            ///     of the section to the storage unit affected by the relocation. For an
            ///     executable file or a shared object, the value is the virtual address of
            ///     the storage unit affected by the relocation.
            /// </summary>
            /// <remarks>
            ///     Quoted from http://www.scs.stanford.edu/14wi-cs140/pintos/specs/sysv-abi-update.html/ch4.reloc.html
            /// </remarks>
            public byte* Offset;

            //Interpreted from Info field
            public uint Symbol
            {
                get { return Info >> 8; }
            }

            public RelocationType Type
            {
                get { return (RelocationType)(Info & 0xFF); }
            }
        }
    }

    public unsafe class ELFRelocationAddendTableSection : ELFSection
    {
        protected List relocations = new List();

        public List Relocations
        {
            get { return relocations; }
        }

        public int SymbolTableSectionIndex
        {
            get { return (int)header.Link; }
        }

        public int SectionToRelocateIndex
        {
            get { return (int)header.Info; }
        }

        public RelocationAddend this[uint index]
        {
            get { return (RelocationAddend)Relocations[(int)index]; }
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

        public class RelocationAddend : Object
        {
            /// <summary>
            ///     This member specifies a constant addend used to compute the value to be
            ///     stored into the relocatable field.
            /// </summary>
            /// <remarks>
            ///     Quoted from http://www.scs.stanford.edu/14wi-cs140/pintos/specs/sysv-abi-update.html/ch4.reloc.html
            /// </remarks>
            public short Addend;

            /// <summary>
            ///     This member gives both the symbol table index with respect to which the
            ///     relocation must be made, and the type of relocation to apply. For example,
            ///     a call instruction's relocation entry would hold the symbol table index
            ///     of the function being called. If the index is STN_UNDEF, the undefined
            ///     symbol index, the relocation uses 0 as the "symbol value". Relocation
            ///     types are processor-specific; descriptions of their behavior appear in the
            ///     processor supplement.
            /// </summary>
            /// <remarks>
            ///     Quoted from http://www.scs.stanford.edu/14wi-cs140/pintos/specs/sysv-abi-update.html/ch4.reloc.html
            /// </remarks>
            public uint Info;

            /// <summary>
            ///     This member gives the location at which to apply the relocation action.
            ///     For a relocatable file, the value is the byte offset from the beginning
            ///     of the section to the storage unit affected by the relocation. For an
            ///     executable file or a shared object, the value is the virtual address of
            ///     the storage unit affected by the relocation.
            /// </summary>
            /// <remarks>
            ///     Quoted from http://www.scs.stanford.edu/14wi-cs140/pintos/specs/sysv-abi-update.html/ch4.reloc.html
            /// </remarks>
            public byte* Offset;

            //Interpreted from Info field
            public uint Symbol
            {
                get { return Info >> 8; }
            }

            public ELFRelocationTableSection.RelocationType Type
            {
                get { return (ELFRelocationTableSection.RelocationType)(Info & 0xFF); }
            }
        }
    }

    public class ELFDynamicSection : ELFSection
    {
        public enum DynamicTag
        {
            Null = 0,
            Needed = 1,
            PLTRELSZ = 2,
            PLTGOT = 3,
            Hash = 4,
            StrTab = 5,
            SymTab = 6,
            RelA = 7,
            RelASZ = 8,
            RelAEnt = 9,
            StrSZ = 10,
            SymEnt = 11,
            Init = 12,
            Fini = 13,
            SOName = 14,
            RPath = 15,
            Symbolic = 16,
            Rel = 17,
            RelSZ = 18,
            RelEnt = 19,
            PLTRel = 20,
            Debug = 21,
            TextRel = 22,
            JmpRel = 23,
            Bind_Now = 24,
            Init_Array = 25,
            Fini_Array = 26,
            Init_ArraySz = 27,
            Fini_ArraySz = 28,
            RunPath = 29,
            Flags = 30,
            Encoding = 32
        }

        protected List dynamics = new List();

        public List Dynamics
        {
            get { return dynamics; }
        }

        public Dynamic StrTabDynamic
        {
            get
            {
                for (int i = 0; i < dynamics.Count; i++)
                {
                    Dynamic theDyn = (Dynamic)dynamics[i];
                    if (theDyn.Tag == DynamicTag.StrTab)
                    {
                        return theDyn;
                    }
                }
                return null;
            }
        }

        public Dynamic StrTabSizeDynamic
        {
            get
            {
                for (int i = 0; i < dynamics.Count; i++)
                {
                    Dynamic theDyn = (Dynamic)dynamics[i];
                    if (theDyn.Tag == DynamicTag.StrSZ)
                    {
                        return theDyn;
                    }
                }
                return null;
            }
        }

        public Dynamic this[uint index]
        {
            get { return (Dynamic)dynamics[(int)index]; }
        }

        public ELFDynamicSection(ELFSectionHeader header)
            : base(header)
        {
        }

        public override int Read(FileStream stream)
        {
            dynamics.Empty();

            int bytesRead = base.Read(stream);
            uint offset = 0;
            Dynamic currDynamic;
            while (offset < bytesRead)
            {
                currDynamic = new Dynamic();

                currDynamic.Tag = (DynamicTag)ByteConverter.ToUInt32(data, offset + 0);
                currDynamic.Val_Ptr = ByteConverter.ToUInt32(data, offset + 4);

                dynamics.Add(currDynamic);

                offset += header.EntrySize;
            }

            return dynamics.Count;
        }

        public class Dynamic : Object
        {
            public DynamicTag Tag;
            public uint Val_Ptr;
        }
    }
}