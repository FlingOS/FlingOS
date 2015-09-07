using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.MIPS32.ASMOps
{
    public class Srlv : ASM.ASMOp
    {
        public string Dest;
        public string Src;
        public string BitsReg;

        public override string Convert(ASM.ASMBlock theBlock)
        {
            return "srlv " + Dest + ", " + Src + ", " + BitsReg;
        }
    }
}
