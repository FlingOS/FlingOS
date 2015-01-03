using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.ATA
{
    public class SATA : Devices.DiskDevice
    {
        public SATA()
        {
        }

        public override void ReadBlock(ulong aBlockNo, uint aBlockCount, byte[] aData)
        {
        }
        public override void WriteBlock(ulong aBlockNo, uint aBlockCount, byte[] aData)
        {
        }

        public override void CleanCaches()
        {
        }
    }
}
