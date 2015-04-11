using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.x86.ASMOps
{
    public class Sub : ASM.ASMOp
    {
        public string Src;
        public string Dest;
        public bool WithBorrow = false;

        public override string Convert(ASM.ASMBlock theBlock)
        {
            if (WithBorrow)
            {
                return "sbb " + Dest + ", " + Src;
            }
            else
            {
                return "sub " + Dest + ", " + Src;
            }
        }
    }
}
