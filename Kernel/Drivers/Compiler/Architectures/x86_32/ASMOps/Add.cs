using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.x86.ASMOps
{
    public class Add : ASM.ASMOp
    {
        public string Src;
        public string Dest;
        public bool WithCarry = false;

        public override string Convert(ASM.ASMBlock theBlock)
        {
            if (WithCarry)
            {
                return "adc " + Dest + ", " + Src;
            }
            else
            {
                return "add " + Dest + ", " + Src;
            }
        }
    }
}
