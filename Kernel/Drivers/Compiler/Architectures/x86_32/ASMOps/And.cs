using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.x86.ASMOps
{
    public class And : ASM.ASMOp
    {
        public Registers Src;
        public Registers Dest;

        public override string Convert()
        {
            return "and " + System.Enum.GetName(typeof(Registers), Dest) + ", " + System.Enum.GetName(typeof(Registers), Src);
        }
    }
}
