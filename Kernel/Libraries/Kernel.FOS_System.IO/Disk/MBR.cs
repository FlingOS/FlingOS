using System;
using Kernel.FOS_System;
using Kernel.FOS_System.Collections;

namespace Kernel.FOS_System.IO.Disk
{
    public class MBR : FOS_System.Object
    {
        /*
         * For details see these articles: 
         *   - http://wiki.osdev.org/Partition_Table
         *   - http://wiki.osdev.org/MBR_(x86)
         */
        public PartitionInfo[] Partitions = new PartitionInfo[4];
        protected UInt32 numPartitions = 0;
        public UInt32 NumPartitions
        {
            get
            {
                return numPartitions;
            }
        }
        protected int PartitionsCapacity
        {
            get
            {
                return Partitions.Length;
            }
        }

        public readonly bool IsValid = false;

        public class PartitionInfo : FOS_System.Object
        {
            public UInt32 EBRLocation = 0;

            public readonly bool Bootable;
            /*  Not used - we use LBA values not Head/Sector/Cylinder
                public readonly byte StartingHead;
                public readonly byte StartingSector;
                public readonly byte StartingCylinder;
            */
            public readonly byte SystemID;
            /*  Not used - we use LBA values not Head/Sector/Cylinder
                public readonly byte EndingHead;
                public readonly byte EndingSector;
                public readonly byte EndingCylinder;
            */
            public readonly UInt32 StartSector;
            public readonly UInt32 SectorCount;

            public PartitionInfo(UInt32 anEBRLocation)
            {
                EBRLocation = anEBRLocation;
            }
            public PartitionInfo(bool isBootable, byte aSystemID, UInt32 aStartSector, UInt32 aSectorCount)
            {
                Bootable = isBootable;
                SystemID = aSystemID;
                StartSector = aStartSector;
                SectorCount = aSectorCount;
            }
        }

        public MBR()
        {
            IsValid = true;
        }
        public MBR(byte[] aMBR)
        {
            //See whether this is a valid MBR
            if (aMBR[0x1FE] != 0x55 || aMBR[0x1FF] != 0xAA)
            {
                IsValid = false;
                return;
            }

            IsValid = true;

            PartitionInfo partInfo = ParsePartition(aMBR, 0x1BE);
            if (partInfo != null)
            {
                AddPartitionToList(partInfo);
            }
            partInfo = ParsePartition(aMBR, 0x1CE);
            if (partInfo != null)
            {
                AddPartitionToList(partInfo);
            }
            partInfo = ParsePartition(aMBR, 0x1DE);
            if (partInfo != null)
            {
                AddPartitionToList(partInfo);
            }
            partInfo = ParsePartition(aMBR, 0x1EE);
            if (partInfo != null)
            {
                AddPartitionToList(partInfo);
            }
        }

        protected static PartitionInfo ParsePartition(byte[] aMBR, UInt32 aLoc)
        {
            byte systemID = aMBR[aLoc + 4];

            if (systemID == 0)
            {
                // If SystemID == 0 means, this partition entry is un-used
                return null;
            }

            if (systemID == 0x5 || systemID == 0xF || systemID == 0x85)
            {
                //Extended Partition Detected
                //DOS only knows about 05, Windows 95 introduced 0F, Linux introduced 85 
                //Search for logical volumes
                //http://thestarman.pcministry.com/asm/mbr/PartTables2.htm
                return new PartitionInfo(FOS_System.ByteConverter.ToUInt32(aMBR, aLoc + 8));
            }
            else
            {
                UInt32 startSector = FOS_System.ByteConverter.ToUInt32(aMBR, aLoc + 8);
                UInt32 sectorCount = FOS_System.ByteConverter.ToUInt32(aMBR, aLoc + 12);

                //BasicConsole.WriteLine(((FOS_System.String)"startSector: ") + startSector);
                //BasicConsole.WriteLine(((FOS_System.String)"sectorCount: ") + sectorCount);
                //BasicConsole.DelayOutput(5);

                bool bootable = aMBR[aLoc + 0] == 0x81;
                return new PartitionInfo(bootable, systemID, startSector, sectorCount);
            }
        }
        protected void AddPartitionToList(PartitionInfo partInfo)
        {
            if (numPartitions >= PartitionsCapacity)
            {
                PartitionInfo[] newArray = new PartitionInfo[NumPartitions + 4];
                for (int i = 0; i < numPartitions; i++)
                {
                    newArray[i] = Partitions[i];
                }
                Partitions = newArray;
            }

            Partitions[numPartitions++] = partInfo;
        }

        public static PartitionInfo CreateFAT32PartitionInfo(Hardware.Devices.DiskDevice aDisk, bool bootable)
        {
            return new PartitionInfo(bootable, 0xC, 2U, (uint)(aDisk.BlockCount - 1));
        }
        public static void FormatDisk(Hardware.Devices.DiskDevice aDisk, List partitions)
        {
            //BasicConsole.WriteLine("Creating new MBR data...");
            byte[] newMBRData = new byte[512];

            newMBRData[0x1FE] = 0x55;
            newMBRData[0x1FF] = 0xAA;

            //BasicConsole.WriteLine(((FOS_System.String)"Set signature: ") + newMBRData[0x1FE] + " " + newMBRData[0x1FF]);
            //BasicConsole.DelayOutput(1);

            //BasicConsole.WriteLine(((FOS_System.String)"Num new partitions: ") + partitions.Count);
            //BasicConsole.DelayOutput(1);

            uint part1Offset = 0x1BE;
            for (uint i = 0; i < partitions.Count; i++)
            {
                PartitionInfo partInfo = (PartitionInfo)partitions[(int)i];
                uint partOffset = part1Offset + (0x10 * i);
                //BasicConsole.WriteLine(((FOS_System.String)"Partition ") + i + " @ " + partOffset);
                //BasicConsole.WriteLine(((FOS_System.String)"Bootable : ") + partInfo.Bootable);
                //BasicConsole.WriteLine(((FOS_System.String)"SystemID : ") + partInfo.SystemID);
                //BasicConsole.WriteLine(((FOS_System.String)"StartSector : ") + partInfo.StartSector);
                //BasicConsole.WriteLine(((FOS_System.String)"SectorCount : ") + partInfo.SectorCount);
                //BasicConsole.DelayOutput(10);

                //Bootable / active
                newMBRData[partOffset + 0] = (byte)(partInfo.Bootable ? 0x81 : 0x00);
                //[File] System ID
                newMBRData[partOffset + 4] = partInfo.SystemID;
                //Start sector
                newMBRData[partOffset + 8] = (byte)(partInfo.StartSector);
                newMBRData[partOffset + 9] = (byte)(partInfo.StartSector >> 8);
                newMBRData[partOffset + 10] = (byte)(partInfo.StartSector >> 16);
                newMBRData[partOffset + 11] = (byte)(partInfo.StartSector >> 24);
                //Sector count
                newMBRData[partOffset + 12] = (byte)(partInfo.SectorCount);
                newMBRData[partOffset + 13] = (byte)(partInfo.SectorCount >> 8);
                newMBRData[partOffset + 14] = (byte)(partInfo.SectorCount >> 16);
                newMBRData[partOffset + 15] = (byte)(partInfo.SectorCount >> 24);

                //BasicConsole.WriteLine("Reading back data...");
                //byte bootable = newMBRData[partOffset + 0];
                //byte systemID = newMBRData[partOffset + 4];
                //UInt32 startSector = ByteConverter.ToUInt32(newMBRData, partOffset + 8);
                //UInt32 sectorCount = ByteConverter.ToUInt32(newMBRData, partOffset + 12);
                //BasicConsole.WriteLine(((FOS_System.String)"Bootable : ") + bootable);
                //BasicConsole.WriteLine(((FOS_System.String)"SystemID : ") + systemID);
                //BasicConsole.WriteLine(((FOS_System.String)"StartSector : ") + startSector);
                //BasicConsole.WriteLine(((FOS_System.String)"SectorCount : ") + sectorCount);
                //BasicConsole.DelayOutput(10);
            }

            //BasicConsole.WriteLine("Writing data...");
            aDisk.WriteBlock(0UL, 1U, newMBRData);
            //BasicConsole.WriteLine("Data written.");
            //BasicConsole.DelayOutput(5);
        }
    }
}
