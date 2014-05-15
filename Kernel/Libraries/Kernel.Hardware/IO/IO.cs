using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.IO
{
    public static class IO
    {
        private static bool[] First1024PortsUsed = new bool[1024];

        public static IOPort TryCreatePort(UInt16 aPort)
        {
            if (aPort < 1024)
            {
                if (!First1024PortsUsed[aPort])
                {
                    First1024PortsUsed[aPort] = true;
                    return new IOPort(aPort);
                }
                return null;
            }
            else
            {
                return new IOPort(aPort);
            }
        }
        public static IOPort TryCreatePort(UInt16 aBase, UInt16 anOffset)
        {
            return TryCreatePort((UInt16)(aBase + anOffset));
        }

        public static void ReleasePort(IOPort aPort)
        {
            if (aPort.Port < 1024)
            {
                First1024PortsUsed[aPort.Port] = false;
            }
        }
    }
}
