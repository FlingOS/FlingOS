using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.MIPS32.ASMOps
{
    public class Div : ASM.ASMOp
    {
        public string Arg1;
        public string Arg2;
        public bool Signed = false;

        public override string Convert(ASM.ASMBlock theBlock)
        {
            if (Signed)
            {
                return "div " + Arg1 + ", " + Arg2;
            }
            else
            {
                return "divu " + Arg1 + ", " + Arg2;
            }
        }
    }
    
}
