using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.MIPS32.ASMOps
{
    public class Sll : ASM.ASMOp
    {
        public string Dest;
        public string Src;
        public int Bits;

        public override string Convert(ASM.ASMBlock theBlock)
        {
            return "sll " + Dest + ", " + Src + ", " + Bits;
        }
    }
}
