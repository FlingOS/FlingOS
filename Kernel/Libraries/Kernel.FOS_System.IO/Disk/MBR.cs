using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            //Verify this is a "valid bootsector"
            //if (aMBR[0x1FE] != 0x55 || aMBR[0x1FF] != 0xAA)
            //{
            //    return;
            //}

            IsValid = true;

            PartitionInfo partInfo = ParsePartition(aMBR, 446);
            if (partInfo != null)
            {
                AddPartition(partInfo);
            }
            partInfo = ParsePartition(aMBR, 462);
            if (partInfo != null)
            {
                AddPartition(partInfo);
            }
            partInfo = ParsePartition(aMBR, 478);
            if (partInfo != null)
            {
                AddPartition(partInfo);
            }
            partInfo = ParsePartition(aMBR, 494);
            if (partInfo != null)
            {
                AddPartition(partInfo);
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

                bool bootable = aMBR[aLoc + 0] == 0x81;
                return new PartitionInfo(bootable, systemID, startSector, sectorCount);
            }
        }
        protected void AddPartition(PartitionInfo partInfo)
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
    }
}
