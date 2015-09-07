using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.MIPS32.ASMOps
{
    public class Sub : ASM.ASMOp
    {
        public string Src1;
        public string Src2;
        public string Dest;
        public bool Unsigned = false;

        public override string Convert(ASM.ASMBlock theBlock)
        {
            if (Unsigned)
            {
                return "subu " + Dest + ", " + Src1 + ", " + Src2;
            }
            else
            {
                return "sub " + Dest + ", " + Src1 + ", " + Src2;
            }
        }
    }
}
