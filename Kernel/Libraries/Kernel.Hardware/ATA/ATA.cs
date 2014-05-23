using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.ATA
{
    public abstract class ATA : Devices.DiskDevice
    {
        public enum ControllerId
        { 
            Primary, 
            Secondary 
        }
        public enum BusPosition
        { 
            Master, 
            Slave 
        }

        public ControllerId controllerId;
        public BusPosition busPosition;

        internal ATA()
        {
            blockSize = 512;
        }
    }
}
