using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.Devices
{
    public abstract class CPU : Device
    {
        public abstract void Halt();

        public static CPU Default;
        public static void InitDefault()
        {
            CPUs.CPUx86_32.Init();
            Default = CPUs.CPUx86_32.TheCPU;
        }
    }
}
