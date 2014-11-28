using Kernel.FOS_System;

namespace Kernel.Core.Processes.ELF
{
    public unsafe class ELFHeader
    {
        public const int HEADER_SIZE = 52;

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

            type = ByteConverter.ToUInt16(data, 16);
            machine = ByteConverter.ToUInt16(data, 18);
            version = ByteConverter.ToUInt32(data, 20);
            entry = (byte*)ByteConverter.ToUInt32(data, 24);
            phoff = ByteConverter.ToUInt32(data, 28);
            shoff = ByteConverter.ToUInt32(data, 32);
            flags = ByteConverter.ToUInt32(data, 36);
            ehsize = ByteConverter.ToUInt16(data, 40);
            phentsize = ByteConverter.ToUInt16(data, 42);
            phnum = ByteConverter.ToUInt16(data, 44);
            shentsize = ByteConverter.ToUInt16(data, 46);
            shnum = ByteConverter.ToUInt16(data, 48);
            shstrndx = ByteConverter.ToUInt16(data, 50);
        }

        public byte[] ident;
        public ushort type;
        public ushort machine;
        public uint version;
        public byte* entry;
        public uint phoff;
        public uint shoff;
        public uint flags;
        public ushort ehsize;
        public ushort phentsize;
        public ushort phnum;
        public ushort shentsize;
        public ushort shnum;
        public ushort shstrndx;
    }
}