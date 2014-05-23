using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kernel.Hardware.Devices;

namespace Kernel.FOS_System.IO
{
    public class Partition : DiskDevice
    {
        internal DiskDevice TheDiskDevice;
        internal UInt64 StartingSector;

        public FOS_System.String VolumeID = "[NO ID]";

        public Partition(DiskDevice aDiskDevice, UInt64 aStartingSector, UInt64 aSectorCount)
        {
            TheDiskDevice = aDiskDevice;
            StartingSector = aStartingSector;
            blockCount = aSectorCount;
            blockSize = aDiskDevice.BlockSize;
        }

        public override void ReadBlock(UInt64 aBlockNo, UInt32 aBlockCount, byte[] aData)
        {
            UInt64 xHostBlockNo = StartingSector + aBlockNo;
            TheDiskDevice.ReadBlock(xHostBlockNo, aBlockCount, aData);
        }

        public override void WriteBlock(UInt64 aBlockNo, UInt32 aBlockCount, byte[] aData)
        {
            UInt64 xHostBlockNo = StartingSector + aBlockNo;
            TheDiskDevice.WriteBlock(xHostBlockNo, aBlockCount, aData);
        }
    }
}
