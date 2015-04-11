using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.x86.ASMOps
{
    public class Call : ASM.ASMOp
    {
        public string Target;

        public override string Convert(ASM.ASMBlock theBlock)
        {
            return "call " + Target;
        }
    }
}
