using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.MIPS32.ASMOps
{
    public class Add : ASM.ASMOp
    {
        public string Src1;
        public string Src2;
        public string Dest;
        public bool Unsigned = true;

        public override string Convert(ASM.ASMBlock theBlock)
        {
            if (Unsigned)
            {
                return "addu " + Dest + ", " + Src1 + ", " + Src2;
            }
            else
            {
                return "add " + Dest + ", " + Src1 + ", " + Src2;
            }
        }
    }
}
