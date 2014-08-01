using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Hardware.CPUs
{
    [Compiler.PluggedClass]
    public class CPUx86_32 : Devices.CPU
    {
        [Compiler.PluggedMethod(ASMFilePath=@"ASM\CPUs\CPUx86_32\Halt")]
        public override void Halt()
        {
        }

        public static CPUx86_32 TheCPU;
        public static void Init()
        {
            if (TheCPU == null)
            {
                TheCPU = new CPUx86_32();
            }
        }
    }
}
