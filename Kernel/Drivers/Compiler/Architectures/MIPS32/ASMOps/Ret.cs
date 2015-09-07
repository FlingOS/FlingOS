using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.MIPS32.ASMOps
{
    public class Ret : ASM.ASMOp
    {
        public override string Convert(ASM.ASMBlock theBlock)
        {
            return "lw $ra, 0($sp)\r\n" +
                   "addi $sp, $sp, 4\r\n" +
                   "j $ra";
        }
    }
}
