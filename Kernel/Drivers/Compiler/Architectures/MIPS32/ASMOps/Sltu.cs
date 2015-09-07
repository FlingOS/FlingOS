using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.MIPS32.ASMOps
{
    public class Sltu : ASM.ASMOp
    {
        public string Src1;
        public string Src2;
        public string Dest;

        public override string Convert(ASM.ASMBlock theBlock)
        {
            return "sltu " + Dest + ", " + Src1 + ", " + Src2;
        }
    }
}
