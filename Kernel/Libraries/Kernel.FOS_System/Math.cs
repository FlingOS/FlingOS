using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.FOS_System
{
    [Compiler.PluggedClass]
    public static class Math
    {
        [Compiler.PluggedMethod(ASMFilePath=@"ASM\Math\Divide")]
        public static ulong Divide(ulong dividend, uint divisor)
        {
            return 0;
        }
    }
}
