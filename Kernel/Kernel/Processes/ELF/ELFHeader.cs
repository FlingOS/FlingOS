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

namespace Kernel.Processes.ELF
{
    public enum ELFFileClass : byte
    {
        None = 0,
        Class32 = 1,
        Class64 = 2
    }

    public enum ELFDataEncoding : byte
    {
        Invalid = 0,
        LSB = 1,
        MSB = 2
    }

    public enum ELFFileType : ushort
    {
        None = 0,
        Relocatable = 1,
        Executable = 2,
        Shared = 3,
        Core = 4
    }

    public enum ELFMachines : ushort
    {
        None = 0,
        M32 = 1,
        SPARC = 2,
        Intel80386 = 3,
        Motorola68K = 4,
        Motorola88K = 5,
        Intel80860 = 7,
        MIPS = 8
    }

    public unsafe class ELFHeader : Object
    {
        public const int HEADER_SIZE = 52;
        public ushort ELFHeaderSize;
        public byte* EntryPoint;
        public ELFFileType FileType;
        public uint flags;

        public byte[] ident;
        public ELFMachines Machine;
        public ushort ProgHeaderEntrySize;
        public ushort ProgHeaderNumEntries;
        public uint ProgHeaderTableOffset;
        public ushort SecHeaderEntrySize;
        public ushort SecHeaderIdxForSecNameStrings;
        public ushort SecHeaderNumEntries;
        public uint SecHeaderTableOffset;
        public uint Version;

        public bool SignatureOK
        {
            get
            {
                bool OK = ident[0] == 0x7F;
                OK = OK && ident[1] == 'E';
                OK = OK && ident[2] == 'L';
                OK = OK && ident[3] == 'F';
                return OK;
            }
        }

        public ELFFileClass FileClass
        {
            get { return (ELFFileClass)ident[4]; }
        }

        public ELFDataEncoding DataEncoding
        {
            get { return (ELFDataEncoding)ident[5]; }
        }

        public byte HeaderVersion
        {
            get { return ident[6]; }
        }

        public ELFHeader(byte[] data)
        {
            ident = new byte[16];
            ident[0] = data[0];
            ident[1] = data[1];
            ident[2] = data[2];
            ident[3] = data[3];
            ident[4] = data[4];
            ident[5] = data[5];
            ident[6] = data[6];
            ident[7] = data[7];
            ident[8] = data[8];
            ident[9] = data[9];
            ident[10] = data[10];
            ident[11] = data[11];
            ident[12] = data[12];
            ident[13] = data[13];
            ident[14] = data[14];
            ident[15] = data[15];

            FileType = (ELFFileType)ByteConverter.ToUInt16(data, 16);
            Machine = (ELFMachines)ByteConverter.ToUInt16(data, 18);
            Version = ByteConverter.ToUInt32(data, 20);
            EntryPoint = (byte*)ByteConverter.ToUInt32(data, 24);
            ProgHeaderTableOffset = ByteConverter.ToUInt32(data, 28);
            SecHeaderTableOffset = ByteConverter.ToUInt32(data, 32);
            flags = ByteConverter.ToUInt32(data, 36);
            ELFHeaderSize = ByteConverter.ToUInt16(data, 40);
            ProgHeaderEntrySize = ByteConverter.ToUInt16(data, 42);
            ProgHeaderNumEntries = ByteConverter.ToUInt16(data, 44);
            SecHeaderEntrySize = ByteConverter.ToUInt16(data, 46);
            SecHeaderNumEntries = ByteConverter.ToUInt16(data, 48);
            SecHeaderIdxForSecNameStrings = ByteConverter.ToUInt16(data, 50);
        }
    }
}