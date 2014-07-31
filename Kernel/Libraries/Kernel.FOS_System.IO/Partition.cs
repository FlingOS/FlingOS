#region Copyright Notice
/// ------------------------------------------------------------------------------ ///
///                                                                                ///
///               All contents copyright � Edward Nutting 2014                     ///
///                                                                                ///
///        You may not share, reuse, redistribute or otherwise use the             ///
///        contents this file outside of the Fling OS project without              ///
///        the express permission of Edward Nutting or other copyright             ///
///        holder. Any changes (including but not limited to additions,            ///
///        edits or subtractions) made to or from this document are not            ///
///        your copyright. They are the copyright of the main copyright            ///
///        holder for all Fling OS files. At the time of writing, this             ///
///        owner was Edward Nutting. To be clear, owner(s) do not include          ///
///        developers, contributors or other project members.                      ///
///                                                                                ///
/// ------------------------------------------------------------------------------ ///
#endregion
    
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kernel.Hardware.Devices;

namespace Kernel.FOS_System.IO
{
    /// <summary>
    /// Represents a partition on a disk drive.
    /// </summary>
    public class Partition : DiskDevice
    {
        /// <summary>
        /// The underlying disk device on which this partition resides.
        /// </summary>
        internal DiskDevice TheDiskDevice;
        /// <summary>
        /// The sector number at which the partition starts.
        /// </summary>
        internal UInt64 StartingSector;

        /// <summary>
        /// The ID of this partition (volume).
        /// </summary>
        public FOS_System.String VolumeID = "[NO ID]";

        /// <summary>
        /// Initializes a new partition.
        /// </summary>
        /// <param name="aDiskDevice">The disk device on which the partition resides.</param>
        /// <param name="aStartingSector">The sector number at which the partition starts.</param>
        /// <param name="aSectorCount">The number of sectors in the partition.</param>
        public Partition(DiskDevice aDiskDevice, UInt64 aStartingSector, UInt64 aSectorCount)
        {
            TheDiskDevice = aDiskDevice;
            StartingSector = aStartingSector;
            blockCount = aSectorCount;
            blockSize = aDiskDevice.BlockSize;
        }

        /// <summary>
        /// Reads contiguous blocks within the partition. Block 0 = 1st sector of the partition.
        /// </summary>
        /// <param name="aBlockNo">The first sector (block) number to read.</param>
        /// <param name="aBlockCount">The number of sectors (blocks) to read.</param>
        /// <param name="aData">The buffer to read into.</param>
        public override void ReadBlock(UInt64 aBlockNo, UInt32 aBlockCount, byte[] aData)
        {
            UInt64 xHostBlockNo = StartingSector + aBlockNo;
            TheDiskDevice.ReadBlock(xHostBlockNo, aBlockCount, aData);
        }

        /// <summary>
        /// Writes contiguous blocks to the partition. Block 0 = 1st sector of the partition.
        /// </summary>
        /// <param name="aBlockNo">The first sector (block) to write.</param>
        /// <param name="aBlockCount">The number of sectors (blocks) to write.</param>
        /// <param name="aData">The data to write.</param>
        public override void WriteBlock(UInt64 aBlockNo, UInt32 aBlockCount, byte[] aData)
        {
            UInt64 xHostBlockNo = StartingSector + aBlockNo;
            TheDiskDevice.WriteBlock(xHostBlockNo, aBlockCount, aData);
        }

        public static bool HasPartitions(DiskDevice disk)
        {
            for (int i = 0; i < FOS_System.IO.FileSystemManager.Partitions.Count; i++)
            {
                FOS_System.IO.Partition part = (FOS_System.IO.Partition)FOS_System.IO.FileSystemManager.Partitions[i];
                if (part.TheDiskDevice == disk)
                {
                    return true;
                }
            }
            return false;
        }
        public static Partition GetFirstPartition(DiskDevice disk)
        {
            for (int i = 0; i < FOS_System.IO.FileSystemManager.Partitions.Count; i++)
            {
                FOS_System.IO.Partition part = (FOS_System.IO.Partition)FOS_System.IO.FileSystemManager.Partitions[i];
                if (part.TheDiskDevice == disk)
                {
                    return part;
                }
            }
            return null;
        }
    }
}
