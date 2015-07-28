using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.x86.ASMOps
{
    public class Header : ASM.ASMHeader
    {
        public override string Convert(ASM.ASMBlock theBlock)
        {
            return @"BITS 32
SECTION .text";
        }
    }
}
