using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.x86.ASMOps
{
    public class Ret : ASM.ASMOp
    {

        public override string Convert(ASM.ASMBlock theBlock)
        {
            return "ret";
        }
    }
}
