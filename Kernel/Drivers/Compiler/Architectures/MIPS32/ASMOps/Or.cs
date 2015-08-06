using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.MIPS32.ASMOps
{
    public class Or : ASM.ASMOp
    {
        public string Src1;
        public string Src2;
        public string Dest;

        public override string Convert(ASM.ASMBlock theBlock)
        {
            return "or " + Dest + ", " + Src1 + ", " + Src2;
        }
    }
}
