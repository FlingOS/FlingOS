using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.MIPS32.ASMOps
{
    public class Mov : ASM.ASMOp
    {
        public OperandSize Size;
        public string Src;
        public string Dest;
        bool DestIsMemory = false;
        bool SrcIsMemory = false;

        public override string Convert(ASM.ASMBlock theBlock)
        {
            if (DestIsMemory && SrcIsMemory)
            {
                throw new NotSupportedException("MIPS: Cannot move between memory locations directly! Must go via a register.");
            }

            if (DestIsMemory)
            {
                switch (Size)
                {
                    case OperandSize.Byte:
                        return "sb " + Dest + ", " + Src;
                    case OperandSize.Halfword:
                        return "sh " + Dest + ", " + Src;
                    case OperandSize.Word:
                        return "sw " + Dest + ", " + Src;
                    default:
                        throw new NotSupportedException("MIPS: Unrecognised move operand sizes. (DestIsMemory)");
                }
            }
            else if (SrcIsMemory)
            {
                switch (Size)
                {
                    case OperandSize.Byte:
                        return "lbu " + Dest + ", " + Src;
                    case OperandSize.Halfword:
                        return "lhu " + Dest + ", " + Src;
                    case OperandSize.Word:
                        return "lw " + Dest + ", " + Src;
                    default:
                        throw new NotSupportedException("MIPS: Unrecognised move operand sizes. (SrcIsMemory)");
                }
            }
            else
            {
                return "move " + Dest + ", " + Src;
            }
        }
    }
}
