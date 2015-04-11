using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.x86.ASMOps
{
    public class Cmp : ASM.ASMOp
    {
        public string Arg2;
        public string Arg1;

        public override string Convert(ASM.ASMBlock theBlock)
        {
            return "cmp " + Arg1 + ", " + Arg2;
        }
    }
}
