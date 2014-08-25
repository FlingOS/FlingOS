#region Copyright Notice
// ------------------------------------------------------------------------------ //
//                                                                                //
//               All contents copyright � Edward Nutting 2014                     //
//                                                                                //
//        You may not share, reuse, redistribute or otherwise use the             //
//        contents this file outside of the Fling OS project without              //
//        the express permission of Edward Nutting or other copyright             //
//        holder. Any changes (including but not limited to additions,            //
//        edits or subtractions) made to or from this document are not            //
//        your copyright. They are the copyright of the main copyright            //
//        holder for all Fling OS files. At the time of writing, this             //
//        owner was Edward Nutting. To be clear, owner(s) do not include          //
//        developers, contributors or other project members.                      //
//                                                                                //
// ------------------------------------------------------------------------------ //
#endregion
    
#define GPT_TRACE
#undef GPT_TRACE

using System;
using Kernel.FOS_System.Collections;

namespace Kernel.FOS_System.IO.Disk
{
    /// <summary>
    /// Represents a GUID partition table partitioning scheme.
    /// </summary>
    public class GPT : FOS_System.Object
    {
        /// <summary>
        /// The list of non-empty partitions found in partition table.
        /// </summary>
        public List Partitions = new List(4);

        /// <summary>
        /// The revision number of the GPT.
        /// </summary>
        public uint Revision = 0;
        /// <summary>
        /// The size of the GPT header.
        /// </summary>
        public uint HeaderSize = 0;
        /// <summary>
        /// The CRC32 cheksum of the GPT header.
        /// </summary>
        public uint HeaderCRC32 = 0;
        /// <summary>
        /// The LBA of the GPT header. Should always be 1.
        /// </summary>
        public ulong HeaderLBA = 0;
        /// <summary>
        /// The LBA of the backup GPT header. Shoukd be last sector on the disk.
        /// </summary>
        public ulong HeaderBackupLBA = 0;
        /// <summary>
        /// First LBA which can be used for partition data.
        /// </summary>
        public ulong FirstUsableLBAForPartitions = 0;
        /// <summary>
        /// Last LBA which can be used for partition data.
        /// </summary>
        public ulong LastUsableLBAForPartitions = 0;
        /// <summary>
        /// The bytes of the disk GUID.
        /// </summary>
        public byte[] DiskGUID = null;
        /// <summary>
        /// The LBA at which the partition array starts. 
        /// </summary>
        public ulong StartingLBAOfPartitionArray = 0;
        /// <summary>
        /// The number of entries in the partition array.
        /// </summary>
        public uint NumPartitionEntries = 0;
        /// <summary>
        /// The size of a partition entry in the partition array.
        /// </summary>
        public uint SizeOfPartitionEntry = 0;
        /// <summary>
        /// CRC32 checksum of the partition array.
        /// </summary>
        public uint PartitionArrayCRC32 = 0;

        /// <summary>
        /// Whether the GPT is valid or not.
        /// </summary>
        public readonly bool IsValid = false;

        /// <summary>
        /// Represents partition information read from the GPT.
        /// </summary>
        public class PartitionInfo : FOS_System.Object
        {
            /// <summary>
            /// Whether the partition entry is empty or not.
            /// </summary>
            public bool Empty
            {
                get
                {
                    return ID[0] == 0 &&
                           ID[1] == 0 &&
                           ID[2] == 0 &&
                           ID[3] == 0 &&
                           ID[4] == 0 &&
                           ID[5] == 0 &&
                           ID[6] == 0 &&
                           ID[7] == 0 &&
                           ID[8] == 0 &&
                           ID[9] == 0 &&
                           ID[10] == 0 &&
                           ID[11] == 0 &&
                           ID[12] == 0 &&
                           ID[13] == 0 &&
                           ID[14] == 0 &&
                           ID[15] == 0;
                }
            }
            /// <summary>
            /// The partition Type GUID.
            /// </summary>
            public byte[] TypeID = new byte[16];
            /// <summary>
            /// The partition GUID.
            /// </summary>
            public byte[] ID = new byte[16];
            /// <summary>
            /// The first LBA of the partition.
            /// </summary>
            public ulong FirstLBA = 0;
            /// <summary>
            /// The last LBA of the partition.
            /// </summary>
            public ulong LastLBA = 0;
            /// <summary>
            /// The partition attributes.
            /// </summary>
            public ulong Attributes = 0;
            /// <summary>
            /// The name of the partition.
            /// </summary>
            public FOS_System.String Name;

            /// <summary>
            /// Initialises new partition information using the specified data.
            /// </summary>
            /// <param name="data">The data to read the partition info from.</param>
            /// <param name="offset">The offset in the data at which to start reading.</param>
            /// <param name="entrySize">The size of a partition entry.</param>
            public PartitionInfo(byte[] data, uint offset, uint entrySize)
            {
                TypeID[0] = data[offset + 0];
                TypeID[1] = data[offset + 1];
                TypeID[2] = data[offset + 2];
                TypeID[3] = data[offset + 3];
                TypeID[4] = data[offset + 4];
                TypeID[5] = data[offset + 5];
                TypeID[6] = data[offset + 6];
                TypeID[7] = data[offset + 7];
                TypeID[8] = data[offset + 8];
                TypeID[9] = data[offset + 9];
                TypeID[10] = data[offset + 10];
                TypeID[11] = data[offset + 11];
                TypeID[12] = data[offset + 12];
                TypeID[13] = data[offset + 13];
                TypeID[14] = data[offset + 14];
                TypeID[15] = data[offset + 15];

                ID[0] = data[offset + 16];
                ID[1] = data[offset + 17];
                ID[2] = data[offset + 18];
                ID[3] = data[offset + 19];
                ID[4] = data[offset + 20];
                ID[5] = data[offset + 21];
                ID[6] = data[offset + 22];
                ID[7] = data[offset + 23];
                ID[8] = data[offset + 24];
                ID[9] = data[offset + 25];
                ID[10] = data[offset + 26];
                ID[11] = data[offset + 27];
                ID[12] = data[offset + 28];
                ID[13] = data[offset + 29];
                ID[14] = data[offset + 30];
                ID[15] = data[offset + 31];

                FirstLBA = ByteConverter.ToUInt32(data, offset + 32);
                LastLBA = ByteConverter.ToUInt32(data, offset + 40);
                Attributes = ByteConverter.ToUInt32(data, offset + 48);
                Name = ByteConverter.GetASCIIStringFromUTF16(data, offset + 56, 36).Trim();

#if GPT_TRACE
                BasicConsole.WriteLine(((FOS_System.String)"First LBA : ") + FirstLBA);
                BasicConsole.WriteLine(((FOS_System.String)"Last LBA : ") + LastLBA);
                BasicConsole.WriteLine(((FOS_System.String)"Attributes : ") + Attributes);
                BasicConsole.WriteLine(((FOS_System.String)"Name : ") + Name);
#endif
            }
        }

        /// <summary>
        /// Initialises a new, empty GPT and marks it as valid.
        /// </summary>
        public GPT()
        {
            IsValid = true;
        }
        /// <summary>
        /// Initialises a new GPT and attempts to read its information from the specified disk.
        /// </summary>
        /// <param name="disk">The disk to read the GPT from.</param>
        public GPT(Hardware.Devices.DiskDevice disk)
        {
#if GPT_TRACE
            BasicConsole.WriteLine("Checking for GPT...");
            BasicConsole.DelayOutput(1);
#endif

            uint blockSize = 512;
            //Check for single MBR partition with 0xEE system ID
            byte[] blockData = new byte[blockSize];
            disk.ReadBlock(0UL, 1U, blockData);

            MBR TheMBR = new MBR(blockData);
            if (!TheMBR.IsValid)
            {
#if GPT_TRACE
                BasicConsole.WriteLine("MBR invalid.");
                BasicConsole.DelayOutput(1);
#endif

                return;
            }
            else if (TheMBR.NumPartitions == 0)
            {
#if GPT_TRACE
                BasicConsole.WriteLine("No partitions in MBR.");
                BasicConsole.DelayOutput(1);
#endif
                return;
            }
            else if (TheMBR.Partitions[0].SystemID != 0xEE)
            {
#if GPT_TRACE
                BasicConsole.WriteLine(((FOS_System.String)"MBR partition 0 system ID not GPT. ") + TheMBR.Partitions[0].SystemID);
                BasicConsole.DelayOutput(1);
#endif

                return;
            }

#if GPT_TRACE
            BasicConsole.WriteLine("GPT MBR partition detected.");
            BasicConsole.DelayOutput(1);
#endif

            //Now we know this is very-likely to be GPT formatted. 
            //  But we must check the GPT header for signature etc.

            disk.ReadBlock(1UL, 1U, blockData);

            //Check for GPT signature: 0x45 0x46 0x49 0x20 0x50 0x41 0x52 0x54
            bool OK = blockData[0] == 0x45;
            OK = OK && blockData[1] == 0x46;
            OK = OK && blockData[2] == 0x49;
            OK = OK && blockData[3] == 0x20;
            OK = OK && blockData[4] == 0x50;
            OK = OK && blockData[5] == 0x41;
            OK = OK && blockData[6] == 0x52;
            OK = OK && blockData[7] == 0x54;

            if (!OK)
            {
#if GPT_TRACE
                BasicConsole.WriteLine("GPT signature invalid.");
                BasicConsole.DelayOutput(1);
#endif
                return;
            }

            IsValid = true;
            
#if GPT_TRACE
            BasicConsole.WriteLine("Valid GPT detected.");
            BasicConsole.DelayOutput(5);
#endif

            Revision = ByteConverter.ToUInt32(blockData, 8);
            HeaderSize = ByteConverter.ToUInt32(blockData, 12);
            HeaderCRC32 = ByteConverter.ToUInt32(blockData, 16);
            HeaderLBA = ByteConverter.ToUInt64(blockData, 24);
            HeaderBackupLBA = ByteConverter.ToUInt64(blockData, 32);
            FirstUsableLBAForPartitions = ByteConverter.ToUInt64(blockData, 40);
            LastUsableLBAForPartitions = ByteConverter.ToUInt64(blockData, 48);

#if GPT_TRACE
            BasicConsole.WriteLine(((FOS_System.String)"Revision : ") + Revision);
            BasicConsole.WriteLine(((FOS_System.String)"Header size : ") + HeaderSize);
            BasicConsole.WriteLine(((FOS_System.String)"Header CRC32 : ") + HeaderCRC32);
            BasicConsole.WriteLine(((FOS_System.String)"Header LBA : ") + HeaderLBA);
            BasicConsole.WriteLine(((FOS_System.String)"Header Backup LBA : ") + HeaderBackupLBA);
            BasicConsole.WriteLine(((FOS_System.String)"First usable LBA for partitions : ") + FirstUsableLBAForPartitions);
            BasicConsole.WriteLine(((FOS_System.String)"Last usable LBA for partitions : ") + LastUsableLBAForPartitions);
            BasicConsole.DelayOutput(5);
#endif

            DiskGUID = new byte[16];
            DiskGUID[0] = blockData[56];
            DiskGUID[1] = blockData[57];
            DiskGUID[2] = blockData[58];
            DiskGUID[3] = blockData[59];
            DiskGUID[4] = blockData[60];
            DiskGUID[5] = blockData[61];
            DiskGUID[6] = blockData[62];
            DiskGUID[7] = blockData[63];
            DiskGUID[8] = blockData[64];
            DiskGUID[9] = blockData[65];
            DiskGUID[10] = blockData[66];
            DiskGUID[11] = blockData[67];
            DiskGUID[12] = blockData[68];
            DiskGUID[13] = blockData[69];
            DiskGUID[14] = blockData[70];
            DiskGUID[15] = blockData[71];

            StartingLBAOfPartitionArray = ByteConverter.ToUInt64(blockData, 72);
            NumPartitionEntries = ByteConverter.ToUInt32(blockData, 80);
            SizeOfPartitionEntry = ByteConverter.ToUInt32(blockData, 84);
            PartitionArrayCRC32 = ByteConverter.ToUInt32(blockData, 88);

#if GPT_TRACE
            BasicConsole.WriteLine(((FOS_System.String)"Start LBA of part arrray : ") + StartingLBAOfPartitionArray);
            BasicConsole.WriteLine(((FOS_System.String)"Num part entries : ") + NumPartitionEntries);
            BasicConsole.WriteLine(((FOS_System.String)"Size of part entry : ") + SizeOfPartitionEntry);
            BasicConsole.WriteLine(((FOS_System.String)"Part array CRC32 : ") + PartitionArrayCRC32);
            BasicConsole.DelayOutput(5);
#endif

            ulong blockNum = StartingLBAOfPartitionArray;
            uint entriesPerBlock = blockSize / SizeOfPartitionEntry;

#if GPT_TRACE
            BasicConsole.WriteLine("Reading partition entries...");
            BasicConsole.WriteLine(((FOS_System.String)"blockNum=") + blockNum);
            BasicConsole.WriteLine(((FOS_System.String)"entriesPerBlock=") + entriesPerBlock);
            BasicConsole.DelayOutput(1);
#endif

            //Read partition infos
            for(uint i = 0; i < NumPartitionEntries; i++)
            {
                if (i % entriesPerBlock == 0)
                {
#if GPT_TRACE
                    BasicConsole.WriteLine("Reading block data...");
                    BasicConsole.WriteLine(((FOS_System.String)"blockNum=") + blockNum);
                    BasicConsole.DelayOutput(1);
#endif
                    disk.ReadBlock(blockNum++, 1u, blockData);
                }

                uint offset = (i % entriesPerBlock) * SizeOfPartitionEntry;
#if GPT_TRACE
                BasicConsole.WriteLine("Reading entry...");
                BasicConsole.WriteLine(((FOS_System.String)"offset=") + offset);
#endif
                PartitionInfo inf = new PartitionInfo(blockData, offset, SizeOfPartitionEntry);
                if (!inf.Empty)
                {
#if GPT_TRACE
                    BasicConsole.WriteLine("Entry not empty.");
#endif
                    Partitions.Add(inf);
                }
#if GPT_TRACE
                else
                {
                    BasicConsole.WriteLine("Entry empty.");
                }
#endif   
            }
        }
    }
}
