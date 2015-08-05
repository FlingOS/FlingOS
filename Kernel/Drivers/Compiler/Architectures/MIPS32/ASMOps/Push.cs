using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.MIPS32.ASMOps
{
    class Push : ASM.ASMOp
    {
        public OperandSize Size;
        public string Src;

        public override string Convert(ASM.ASMBlock theBlock)
        {
            int numBytes = -(int)Size;
            string storeOp = "";
            switch (Size)
            {
                case OperandSize.Byte:
                    storeOp = "sbu";
                    break;
                case OperandSize.Halfword:
                    storeOp = "shu";
                    break;
                case OperandSize.Word:
                    storeOp = "sw";
                    break;
            }
            return @"addi $sp, $sp, " + numBytes + "\r\n" + 
                storeOp + " " + Src + @", 0($sp)";
        }
    }
}
