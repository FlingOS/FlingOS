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

#define GPT_TRACE
#undef GPT_TRACE

using Kernel.Devices;
using Kernel.Framework;
using Kernel.Framework.Collections;

namespace Kernel.FileSystems.Disk
{
    /// <summary>
    ///     Represents a GUID partition table partitioning scheme.
    /// </summary>
    public class GPT : Object
    {
        /// <summary>
        ///     Whether the GPT is valid or not.
        /// </summary>
        public readonly bool IsValid;

        /// <summary>
        ///     The bytes of the disk GUID.
        /// </summary>
        public byte[] DiskGUID;

        /// <summary>
        ///     First LBA which can be used for partition data.
        /// </summary>
        public ulong FirstUsableLBAForPartitions;

        /// <summary>
        ///     The LBA of the backup GPT header. Should be last sector on the disk.
        /// </summary>
        public ulong HeaderBackupLBA;

        /// <summary>
        ///     The CRC32 cheksum of the GPT header.
        /// </summary>
        public uint HeaderCRC32;

        /// <summary>
        ///     The LBA of the GPT header. Should always be 1.
        /// </summary>
        public ulong HeaderLBA;

        /// <summary>
        ///     The size of the GPT header.
        /// </summary>
        public uint HeaderSize;

        /// <summary>
        ///     Last LBA which can be used for partition data.
        /// </summary>
        public ulong LastUsableLBAForPartitions;

        /// <summary>
        ///     The number of entries in the partition array.
        /// </summary>
        public uint NumPartitionEntries;

        /// <summary>
        ///     CRC32 checksum of the partition array.
        /// </summary>
        public uint PartitionArrayCRC32;

        /// <summary>
        ///     The list of non-empty partitions found in partition table.
        /// </summary>
        public List Partitions = new List(4);

        /// <summary>
        ///     The revision number of the GPT.
        /// </summary>
        public uint Revision;

        /// <summary>
        ///     The size of a partition entry in the partition array.
        /// </summary>
        public uint SizeOfPartitionEntry;

        /// <summary>
        ///     The LBA at which the partition array starts.
        /// </summary>
        public ulong StartingLBAOfPartitionArray;

        /// <summary>
        ///     Initialises a new, empty GPT and marks it as valid.
        /// </summary>
        public GPT()
        {
            IsValid = true;
        }

        /// <summary>
        ///     Initialises a new GPT and attempts to read its information from the specified disk.
        /// </summary>
        /// <param name="disk">The disk to read the GPT from.</param>
        public GPT(DiskDevice disk)
        {
#if GPT_TRACE
            BasicConsole.WriteLine("Checking for GPT...");
            BasicConsole.DelayOutput(1);
#endif
            //Assumed block size of 512.
            uint blockSize = 512;

            //Note: The GPT format specifies a protective MBR entry (1 partition 
            //          covering the entire disk) immediately followed (byte-wise)
            //          by the GPT. Thus the GPT must come at 512th byte.
            //      However, some disks can have 4096 bytes per sector (/block) 
            //          so the code below might break as reading LBA 1 (2nd LBA) would
            //          return the wrong data. We probably ought to find some way to 
            //          check the block size and just load the required amount of data.

            //Check for single MBR partition with 0xEE system ID
            byte[] blockData = new byte[blockSize];
            //Read the first sector of data.
            disk.ReadBlock(0UL, 1U, blockData);

            //Attempt to read the MBR
            MBR TheMBR = new MBR(blockData);
            //If the MBR isn't valid, the protective MBR partition specified as part of GPT
            //  isn't present / valid so this isn't a valid GPT.
            if (!TheMBR.IsValid)
            {
#if GPT_TRACE
                BasicConsole.WriteLine("MBR invalid.");
                BasicConsole.DelayOutput(1);
#endif

                return;
            }
            //Or, if there is not one and only one partition in the MBR then the 
            //  protective MBR isn't valid so this isn't a valid GPT
            if (TheMBR.NumPartitions != 1)
            {
#if GPT_TRACE
                BasicConsole.WriteLine("No partitions in MBR.");
                BasicConsole.DelayOutput(1);
#endif
                return;
            }
            //Or, the first (/only) partition entry has the wrong ID. 0xEE is the partition
            //  ID for a GOT formatted MBR partition.
            if (TheMBR.Partitions[0].SystemID != 0xEE)
            {
#if GPT_TRACE
                BasicConsole.WriteLine(((Framework.String)"MBR partition 0 system ID not GPT. ") + TheMBR.Partitions[0].SystemID);
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

            //Read the GPT block
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

            //If any part of the ID was wrong, this is not a valid GPT.
            if (!OK)
            {
#if GPT_TRACE
                BasicConsole.WriteLine("GPT signature invalid.");
                BasicConsole.DelayOutput(1);
#endif
                return;
            }

            //Now we know, this is a valid GPT. Whether or not the actual entries are valid
            //  is yet to be determined. There is of course the small chance that some other
            //  data has conflicted with GPT data and that this isn't a GPT, it just looks 
            //  like it. If that is the case, what idiot formatted the disk we are reading
            //  because a conflict like that is impossible to detect without user input!
            IsValid = true;

#if GPT_TRACE
            BasicConsole.WriteLine("Valid GPT detected.");
            BasicConsole.DelayOutput(5);
#endif

            //Load-in GPT global data
            Revision = ByteConverter.ToUInt32(blockData, 8);
            HeaderSize = ByteConverter.ToUInt32(blockData, 12);
            HeaderCRC32 = ByteConverter.ToUInt32(blockData, 16);
            HeaderLBA = ByteConverter.ToUInt64(blockData, 24);
            HeaderBackupLBA = ByteConverter.ToUInt64(blockData, 32);
            FirstUsableLBAForPartitions = ByteConverter.ToUInt64(blockData, 40);
            LastUsableLBAForPartitions = ByteConverter.ToUInt64(blockData, 48);

#if GPT_TRACE
            BasicConsole.WriteLine(((Framework.String)"Revision : ") + Revision);
            BasicConsole.WriteLine(((Framework.String)"Header size : ") + HeaderSize);
            BasicConsole.WriteLine(((Framework.String)"Header CRC32 : ") + HeaderCRC32);
            BasicConsole.WriteLine(((Framework.String)"Header LBA : ") + HeaderLBA);
            BasicConsole.WriteLine(((Framework.String)"Header Backup LBA : ") + HeaderBackupLBA);
            BasicConsole.WriteLine(((Framework.String)"First usable LBA for partitions : ") + FirstUsableLBAForPartitions);
            BasicConsole.WriteLine(((Framework.String)"Last usable LBA for partitions : ") + LastUsableLBAForPartitions);
            BasicConsole.DelayOutput(5);
#endif

            //Load the disk ID
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

            //Load more global GPT data
            StartingLBAOfPartitionArray = ByteConverter.ToUInt64(blockData, 72);
            NumPartitionEntries = ByteConverter.ToUInt32(blockData, 80);
            SizeOfPartitionEntry = ByteConverter.ToUInt32(blockData, 84);
            PartitionArrayCRC32 = ByteConverter.ToUInt32(blockData, 88);

#if GPT_TRACE
            BasicConsole.WriteLine(((Framework.String)"Start LBA of part arrray : ") + StartingLBAOfPartitionArray);
            BasicConsole.WriteLine(((Framework.String)"Num part entries : ") + NumPartitionEntries);
            BasicConsole.WriteLine(((Framework.String)"Size of part entry : ") + SizeOfPartitionEntry);
            BasicConsole.WriteLine(((Framework.String)"Part array CRC32 : ") + PartitionArrayCRC32);
            BasicConsole.DelayOutput(5);
#endif

            ulong blockNum = StartingLBAOfPartitionArray;
            uint entriesPerBlock = blockSize/SizeOfPartitionEntry;

#if GPT_TRACE
            BasicConsole.WriteLine("Reading partition entries...");
            BasicConsole.WriteLine(((Framework.String)"blockNum=") + blockNum);
            BasicConsole.WriteLine(((Framework.String)"entriesPerBlock=") + entriesPerBlock);
            BasicConsole.DelayOutput(1);
#endif

            //TODO: Check the CRC32 values of the header and partition table
            //      are correct. 
            //Note: By not checking the CRCs, we have the option to manually edit
            //      the GPT without rejecting it if CRCs end up incorrect.
            //TODO: Add an override option to ignore the CRCs
            //TODO: Add a method to update / correct the CRCs

            //Read partition infos
            for (uint i = 0; i < NumPartitionEntries; i++)
            {
                //If we're on a block boundary, we need to load the next block
                //  of data to parse.
                if (i%entriesPerBlock == 0)
                {
#if GPT_TRACE
                    BasicConsole.WriteLine("Reading block data...");
                    BasicConsole.WriteLine(((Framework.String)"blockNum=") + blockNum);
                    BasicConsole.DelayOutput(1);
#endif
                    //Load the next block of data
                    disk.ReadBlock(blockNum++, 1u, blockData);
                }

                //Calculate the offset into the current data block
                uint offset = i%entriesPerBlock*SizeOfPartitionEntry;
#if GPT_TRACE
                BasicConsole.WriteLine("Reading entry...");
                BasicConsole.WriteLine(((Framework.String)"offset=") + offset);
#endif
                //Attempt to load the partition info
                PartitionInfo inf = new PartitionInfo(blockData, offset, SizeOfPartitionEntry);
                //Partitions are marked as empty by an all-zero type ID. If the partition is empty,
                //  there is no point adding it.
                if (!inf.Empty)
                {
#if GPT_TRACE
                    BasicConsole.WriteLine("Entry not empty.");
#endif
                    //Add the non-empty partition
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

        /// <summary>
        ///     Represents partition information read from the GPT.
        /// </summary>
        public class PartitionInfo : Object
        {
            /// <summary>
            ///     The partition attributes.
            /// </summary>
            public ulong Attributes;

            /// <summary>
            ///     The first LBA of the partition.
            /// </summary>
            public ulong FirstLBA;

            /// <summary>
            ///     The partition GUID.
            /// </summary>
            public byte[] ID = new byte[16];

            /// <summary>
            ///     The last LBA of the partition.
            /// </summary>
            public ulong LastLBA;

            /// <summary>
            ///     The name of the partition.
            /// </summary>
            public String Name;

            /// <summary>
            ///     The partition Type GUID.
            /// </summary>
            public byte[] TypeID = new byte[16];

            /// <summary>
            ///     Whether the partition entry is empty or not.
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
            ///     Initialises new partition information using the specified data.
            /// </summary>
            /// <param name="data">The data to read the partition info from.</param>
            /// <param name="offset">The offset in the data at which to start reading.</param>
            /// <param name="entrySize">The size of a partition entry.</param>
            public PartitionInfo(byte[] data, uint offset, uint entrySize)
            {
                //There is an underlying assumption here that the
                //  supplied data is of sufficient length (including the 
                //  extra length required for any specified offset).

                //TODO: Check the entry size is valid
                //TODO: Throw an exception if data.length + offset < entrySize

                //Copy in Type ID data
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

                //Copy in the partition ID data
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

                //Parse the other partition data
                FirstLBA = ByteConverter.ToUInt32(data, offset + 32);
                LastLBA = ByteConverter.ToUInt32(data, offset + 40);
                Attributes = ByteConverter.ToUInt32(data, offset + 48);
                Name = ByteConverter.GetASCIIStringFromUTF16(data, offset + 56, 36).Trim();

#if GPT_TRACE
                BasicConsole.WriteLine(((Framework.String)"First LBA : ") + FirstLBA);
                BasicConsole.WriteLine(((Framework.String)"Last LBA : ") + LastLBA);
                BasicConsole.WriteLine(((Framework.String)"Attributes : ") + Attributes);
                BasicConsole.WriteLine(((Framework.String)"Name : ") + Name);
#endif
            }
        }
    }
}