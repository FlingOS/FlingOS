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

#define MBR_TRACE
#undef MBR_TRACE

using Kernel.Devices;
using Kernel.Framework;
using Kernel.Framework.Collections;

namespace Kernel.FileSystems.Disk
{
    /// <summary>
    ///     Represents a master boot record partitioning scheme.
    /// </summary>
    public class MBR : Object
    {
        /// <summary>
        ///     Whether the MBR is valid or not.
        /// </summary>
        public readonly bool IsValid;

        /// <summary>
        ///     The number of partitions set in the Partitions array.
        /// </summary>
        protected uint numPartitions;

        /*
         * For details see these articles: 
         *   - http://wiki.osdev.org/Partition_Table
         *   - http://wiki.osdev.org/MBR_(x86)
         */

        /// <summary>
        ///     The partitions in this MBR.
        /// </summary>
        public PartitionInfo[] Partitions = new PartitionInfo[4];

        /// <summary>
        ///     The number of partitions set in the Partitions array.
        /// </summary>
        public uint NumPartitions
        {
            get { return numPartitions; }
        }

        /// <summary>
        ///     The actual capacity (length) of the partitions array.
        /// </summary>
        protected int PartitionsCapacity
        {
            get { return Partitions.Length; }
        }

        /// <summary>
        ///     Initializes a new, empty MBR and marks it as valid.
        /// </summary>
        public MBR()
        {
            IsValid = true;
        }

        /// <summary>
        ///     Initializes a new MBR from the specified MBR data.
        /// </summary>
        /// <param name="aMBR">The MBR data read from the disk.</param>
        public MBR(byte[] aMBR)
        {
#if MBR_TRACE
            BasicConsole.WriteLine("MBR: 1");
            BasicConsole.WriteLine((uint)Utilities.ObjectUtilities.GetHandle(aMBR));
            BasicConsole.DelayOutput(5);
#endif
            //See whether this is a valid MBR
            if (aMBR[0x1FE] != 0x55 || aMBR[0x1FF] != 0xAA)
            {
                IsValid = false;
                return;
            }

#if MBR_TRACE
            BasicConsole.WriteLine("MBR: 2");
#endif

            //ID is valid so we must assume this is a valid MBR.
            //  If there is data conflict that has caused this to look like MBR when it isn't, 
            //  then we have little or no way of telling.
            IsValid = true;

            //MBR has four partition entries, one or more of which may be empty. We have to check all
            //  of them as there is nothing to say you can't have an empty first partition and non-empty
            //  2nd for example.

            //Note: Positions of MBR entries are fixed

            //Attempt to parse the first entry
            PartitionInfo partInfo = ParsePartition(aMBR, 0x1BE);
            if (partInfo != null)
            {
                //Non-empty, valid partition so add it
                AddPartitionToList(partInfo);
            }
#if MBR_TRACE
            BasicConsole.WriteLine("MBR: 13");
#endif
            //Attempt to parse the second entry
            partInfo = ParsePartition(aMBR, 0x1CE);
            if (partInfo != null)
            {
                //Non-empty, valid partition so add it
                AddPartitionToList(partInfo);
            }
            //Attempt to parse the third entry
            partInfo = ParsePartition(aMBR, 0x1DE);
            if (partInfo != null)
            {
                //Non-empty, valid partition so add it
                AddPartitionToList(partInfo);
            }
            //Attempt to parse the fourth entry
            partInfo = ParsePartition(aMBR, 0x1EE);
            if (partInfo != null)
            {
                //Non-empty, valid partition so add it
                AddPartitionToList(partInfo);
            }
        }

        /// <summary>
        ///     Parses partition information from the MBR data at the specified offset.
        /// </summary>
        /// <param name="aMBR">The MBR data.</param>
        /// <param name="aLoc">The offset of the partition information in the MBR data.</param>
        /// <returns>The partition information or null if the information is not a valid partition.</returns>
        protected static PartitionInfo ParsePartition(byte[] aMBR, uint aLoc)
        {
#if MBR_TRACE
            BasicConsole.WriteLine("MBR: 3");
#endif
            //System ID gives you preliminary information
            //  about what type of data is in the partition and whether
            //  the partition is empty or not.
            byte systemID = aMBR[aLoc + 4];

#if MBR_TRACE
            BasicConsole.WriteLine("MBR: 4");
#endif

            if (systemID == 0)
            {
                // If SystemID == 0 means, this partition entry is un-used
                return null;
            }

#if MBR_TRACE
            BasicConsole.WriteLine("MBR: 5");
#endif
            //Various System IDs for EBR (Extended Boot Record)
            //  (I'd like to know why developers felt the need to each create their own
            //   ID for an EBR partition entry within MBR. Seems silly...)
            if (systemID == 0x5 || systemID == 0xF || systemID == 0x85)
            {
#if MBR_TRACE
                BasicConsole.WriteLine("MBR: 6");
#endif

                //Extended Boot Record formatted partition detected
                //DOS only knows about 05, Windows 95 introduced 0F, Linux introduced 85 
                //Search for logical volumes
                //http://thestarman.pcministry.com/asm/mbr/PartTables2.htm
                return new PartitionInfo(ByteConverter.ToUInt32(aMBR, aLoc + 8));
            }
#if MBR_TRACE
                BasicConsole.WriteLine("MBR: 7");
#endif

            uint startSector = ByteConverter.ToUInt32(aMBR, aLoc + 8);
            uint sectorCount = ByteConverter.ToUInt32(aMBR, aLoc + 12);

#if MBR_TRACE
                BasicConsole.WriteLine(((Framework.String)"startSector: ") + startSector);
                BasicConsole.WriteLine(((Framework.String)"sectorCount: ") + sectorCount);
                BasicConsole.DelayOutput(5);
#endif

#if MBR_TRACE
                BasicConsole.WriteLine("MBR: 8");
#endif
            bool bootable = aMBR[aLoc + 0] == 0x81;
            return new PartitionInfo(bootable, systemID, startSector, sectorCount);
        }

        /// <summary>
        ///     Adds partition info to the list of partitions.
        /// </summary>
        /// <param name="partInfo">The partition info to add.</param>
        protected void AddPartitionToList(PartitionInfo partInfo)
        {
#if MBR_TRACE
            BasicConsole.WriteLine("MBR: 9");
#endif

            //If we need to expand the capacity of the partitions array
            //Note: This stuff was programmed before the List class was programmed.
            if (numPartitions >= PartitionsCapacity)
            {
#if MBR_TRACE
                BasicConsole.WriteLine("MBR: 10");
#endif

                PartitionInfo[] newArray = new PartitionInfo[NumPartitions + 4];
                for (int i = 0; i < numPartitions; i++)
                {
                    newArray[i] = Partitions[i];
                }
                Partitions = newArray;

#if MBR_TRACE
                BasicConsole.WriteLine("MBR: 11");
#endif
            }

            //Add the partition entry.
            Partitions[numPartitions++] = partInfo;

#if MBR_TRACE
            BasicConsole.WriteLine("MBR: 12");
#endif
        }

        /// <summary>
        ///     Creates a new FAT32 partition that covers the entire drive.
        /// </summary>
        /// <param name="aDisk">The disk to create the partition for.</param>
        /// <param name="bootable">Whether to mark the partition as bootable or not.</param>
        /// <returns>The new partition information.</returns>
        public static PartitionInfo CreateFAT32PartitionInfo(DiskDevice aDisk, bool bootable)
        {
            //Can't remember why but we have to start at 3rd logical block
            //  - First sector (sector 0) is for the MBR
            //  - Why do we leave a second sector empty after it?
            return new PartitionInfo(bootable, 0xC, 2U, (uint)(aDisk.Blocks - 2));
        }

        /// <summary>
        ///     Formats the specified using the specified partition informations.
        /// </summary>
        /// <param name="aDisk">The disk to format.</param>
        /// <param name="partitionInfos">The partition informations to use for the format.</param>
        public static void FormatDisk(DiskDevice aDisk, List partitionInfos)
        {
            //Necessary to remove any trace of GPT:
            //  Overwrite first 256 sectors with 0s (should do the trick)
            aDisk.WriteBlock(0UL, 256U, null);

#if MBR_TRACE
            BasicConsole.WriteLine("Creating new MBR data...");
#endif
            byte[] newMBRData = new byte[512];

            newMBRData[0x1FE] = 0x55;
            newMBRData[0x1FF] = 0xAA;

#if MBR_TRACE
            BasicConsole.WriteLine(((Framework.String)"Set signature: ") + newMBRData[0x1FE] + " " + newMBRData[0x1FF]);
            BasicConsole.DelayOutput(1);

            BasicConsole.WriteLine(((Framework.String)"Num new partitions: ") + partitionInfos.Count);
            BasicConsole.DelayOutput(1);
#endif

            uint part1Offset = 0x1BE;
            for (uint i = 0; i < partitionInfos.Count; i++)
            {
                PartitionInfo partInfo = (PartitionInfo)partitionInfos[(int)i];
                uint partOffset = part1Offset + 0x10*i;
#if MBR_TRACE
                BasicConsole.WriteLine(((Framework.String)"Partition ") + i + " @ " + partOffset);
                BasicConsole.WriteLine(((Framework.String)"Bootable : ") + partInfo.Bootable);
                BasicConsole.WriteLine(((Framework.String)"SystemID : ") + partInfo.SystemID);
                BasicConsole.WriteLine(((Framework.String)"StartSector : ") + partInfo.StartSector);
                BasicConsole.WriteLine(((Framework.String)"SectorCount : ") + partInfo.SectorCount);
                BasicConsole.DelayOutput(2);
#endif

                //Bootable / active
                newMBRData[partOffset + 0] = (byte)(partInfo.Bootable ? 0x81 : 0x00);
                //System ID
                newMBRData[partOffset + 4] = partInfo.SystemID;
                //Start sector
                newMBRData[partOffset + 8] = (byte)partInfo.StartSector;
                newMBRData[partOffset + 9] = (byte)(partInfo.StartSector >> 8);
                newMBRData[partOffset + 10] = (byte)(partInfo.StartSector >> 16);
                newMBRData[partOffset + 11] = (byte)(partInfo.StartSector >> 24);
                //Sector count
                newMBRData[partOffset + 12] = (byte)partInfo.SectorCount;
                newMBRData[partOffset + 13] = (byte)(partInfo.SectorCount >> 8);
                newMBRData[partOffset + 14] = (byte)(partInfo.SectorCount >> 16);
                newMBRData[partOffset + 15] = (byte)(partInfo.SectorCount >> 24);

#if MBR_TRACE
                BasicConsole.WriteLine("Reading back data...");
                byte bootable = newMBRData[partOffset + 0];
                byte systemID = newMBRData[partOffset + 4];
                UInt32 startSector = ByteConverter.ToUInt32(newMBRData, partOffset + 8);
                UInt32 sectorCount = ByteConverter.ToUInt32(newMBRData, partOffset + 12);
                BasicConsole.WriteLine(((Framework.String)"Bootable : ") + bootable);
                BasicConsole.WriteLine(((Framework.String)"SystemID : ") + systemID);
                BasicConsole.WriteLine(((Framework.String)"StartSector : ") + startSector);
                BasicConsole.WriteLine(((Framework.String)"SectorCount : ") + sectorCount);
                BasicConsole.DelayOutput(2);
#endif
            }

#if MBR_TRACE
            BasicConsole.WriteLine("Writing data...");
#endif
            aDisk.WriteBlock(0UL, 1U, newMBRData);
#if MBR_TRACE
            BasicConsole.WriteLine("Data written.");
            BasicConsole.DelayOutput(1);
#endif
            aDisk.CleanCaches();
        }

        /// <summary>
        ///     Represents partition information read from the MBR.
        /// </summary>
        public class PartitionInfo : Object
        {
            /// <summary>
            ///     Whether the partition is bootable or not.
            /// </summary>
            public readonly bool Bootable;

            /// <summary>
            ///     The number of sectors in the partition.
            /// </summary>
            public readonly uint SectorCount;

            /*  Not used - we use LBA values not Head/Sector/Cylinder
                public readonly byte EndingHead;
                public readonly byte EndingSector;
                public readonly byte EndingCylinder;
            */

            /// <summary>
            ///     The first sector number of the partition.
            /// </summary>
            public readonly uint StartSector;

            /*  Not used - we use LBA values not Head/Sector/Cylinder
                public readonly byte StartingHead;
                public readonly byte StartingSector;
                public readonly byte StartingCylinder;
            */

            /// <summary>
            ///     The System ID of the partition.
            /// </summary>
            public readonly byte SystemID;

            /// <summary>
            ///     The location of the Extended Boot Record information.
            /// </summary>
            public uint EBRLocation;

            /// <summary>
            ///     Initializes new partition information with only EBR information.
            /// </summary>
            /// <param name="anEBRLocation">The location of the EBR information on disk.</param>
            public PartitionInfo(uint anEBRLocation)
            {
                EBRLocation = anEBRLocation;
            }

            /// <summary>
            ///     Initializes new partition information.
            /// </summary>
            /// <param name="isBootable">Whether the partition is bootable or not.</param>
            /// <param name="aSystemID">The partition's System ID.</param>
            /// <param name="aStartSector">The sector number of the first sector in the partition.</param>
            /// <param name="aSectorCount">The number of sectors in the partition.</param>
            public PartitionInfo(bool isBootable, byte aSystemID, uint aStartSector, uint aSectorCount)
            {
                Bootable = isBootable;
                SystemID = aSystemID;
                StartSector = aStartSector;
                SectorCount = aSectorCount;
            }
        }
    }
}