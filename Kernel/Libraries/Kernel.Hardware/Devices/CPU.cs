using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.Devices
{
    /// <summary>
    /// Represents a CPU in the machine.
    /// </summary>
    public abstract class CPU : Device
    {
        /// <summary>
        /// Halts the CPU (e.g. using x86 hlt instruction)
        /// </summary>
        public abstract void Halt();

        /// <summary>
        /// The default CPU.
        /// </summary>
        public static CPU Default;
        /// <summary>
        /// Initialises the default CPU.
        /// </summary>
        /// <remarks>
        /// Currently just straight up initialises the x86 CPU class. Should actually detect,
        /// either at compile time or runtime, which CPU artchitecture the OS is being run on.
        /// </remarks>
        public static void InitDefault()
        {
            CPUs.CPUx86_32.Init();
            Default = CPUs.CPUx86_32.TheCPU;
        }
    }
}
