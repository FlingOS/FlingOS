using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.x86.ASMOps
{
    public class Label : ASM.ASMOp
    {
        public int ILPosition;
        public string Extension;

        public override string Convert(ASM.ASMBlock theBlock)
        {
            return theBlock.GenerateILOpLabel(ILPosition, Extension) + ":";
        }
    }
}
