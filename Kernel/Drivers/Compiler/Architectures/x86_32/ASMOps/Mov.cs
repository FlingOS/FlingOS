using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.x86.ASMOps
{
    public class Mov : ASM.ASMOp
    {
        public OperandSize Size;
        public string Src;
        public string Dest;

        public override string Convert(ASM.ASMBlock theBlock)
        {
            return "mov " + ASMUtilities.GetOpSizeStr(Size) + " " + Dest + ", " + Src;
        }
    }
}
