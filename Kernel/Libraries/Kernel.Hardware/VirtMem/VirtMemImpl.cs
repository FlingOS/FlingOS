using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.VirtMem
{
    public abstract class VirtMemImpl : FOS_System.Object
    {
        public abstract void Test();

        public abstract void Map(uint pAddr, uint vAddr);
        public abstract uint GetPhysicalAddress(uint vAddr);
    }
}
