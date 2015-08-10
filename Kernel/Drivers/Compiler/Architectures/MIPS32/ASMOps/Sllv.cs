using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.MIPS32.ASMOps
{
    public class Sllv : ASM.ASMOp
    {
        public string Dest;
        public string Src;
        public string BitsReg;

        public override string Convert(ASM.ASMBlock theBlock)
        {
            return "sllv " + Dest + ", " + Src + ", " + BitsReg;
        }
    }
}
