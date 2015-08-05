using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.MIPS32.ASMOps
{
    public class Pop : ASM.ASMOp
    {
        public OperandSize Size;
        public string Dest;

        public override string Convert(ASM.ASMBlock theBlock)
        {
            int numBytes = (int)Size;
            string loadOp = "";
            switch(Size)
            {
                case OperandSize.Byte:
                    loadOp = "lbu";
                    break;
                case OperandSize.Halfword:
                    loadOp = "lhu";
                    break;
                case OperandSize.Word:
                    loadOp = "lw";
                    break;
            }
            return loadOp + " " + Dest + ", 0($sp)\r\n" +
                    "addi $sp, $sp, " + numBytes;
        }
    }
}
