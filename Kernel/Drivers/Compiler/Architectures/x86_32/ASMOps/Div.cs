using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.x86.ASMOps
{
    public class Div : ASM.ASMOp
    {
        public string Arg;
        public bool Signed = false;

        public override string Convert(ASM.ASMBlock theBlock)
        {
            if (Signed)
            {
                return "idiv " + Arg;
            }
            else
            {
                return "div " + Arg;
            }
        }
    }
}
