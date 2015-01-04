using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.ATA
{
    public class PATAPI : ATA
    {
        /// <summary>
        /// IO ports for this device.
        /// </summary>
        protected ATAIOPorts IO;

        public PATAPI(ATAIOPorts anIO, ATA.ControllerID aControllerId, ATA.BusPosition aBusPosition)
        {
            IO = anIO;
            controllerId = aControllerId;
            busPosition = aBusPosition;
            // Disable IRQs, we use polling currently
            IO.Control.Write_Byte((byte)0x02);
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
