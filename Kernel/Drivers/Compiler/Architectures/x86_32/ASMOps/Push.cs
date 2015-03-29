using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.x86.ASMOps
{
    public class Push : ASM.ASMOp
    {
        public OperandSize Size;
        public string Src;

        public override string Convert(ASM.ASMBlock theBlock)
        {
            return "push " + ASMUtilities.GetOpSizeStr(Size) + " " + Src;
        }
    }
}
