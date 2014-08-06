using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.CPUs
{
    /// <summary>
    /// Represents an x86 32-bit CPU.
    /// </summary>
    [Compiler.PluggedClass]
    public class CPUx86_32 : Devices.CPU
    {
        /// <summary>
        /// Halts the CPU using the Hlt instruction.
        /// </summary>
        [Compiler.PluggedMethod(ASMFilePath=@"ASM\CPUs\CPUx86_32\Halt")]
        public override void Halt()
        {
        }

        /// <summary>
        /// The main x86 CPU instance.
        /// </summary>
        public static CPUx86_32 TheCPU;
        /// <summary>
        /// Initialises the main x86 CPU instance.
        /// </summary>
        public static void Init()
        {
            if (TheCPU == null)
            {
                TheCPU = new CPUx86_32();
            }
        }
    }
}
