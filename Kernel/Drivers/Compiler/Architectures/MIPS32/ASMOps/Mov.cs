using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.MIPS32.ASMOps
{
    public class Mov : ASM.ASMOp
    {
        public enum MoveTypes
        {
            SrcMemoryToDestReg,
            SrcRegToDestMemory,
            ImmediateToReg,
            RegToReg
        }

        public OperandSize Size;
        public string Src;
        public string Dest;
        public MoveTypes MoveType = MoveTypes.RegToReg;

        public override string Convert(ASM.ASMBlock theBlock)
        {
            if (MoveType == MoveTypes.SrcRegToDestMemory)
            {
                switch (Size)
                {
                    case OperandSize.Byte:
                        return "sb " + Src + ", " + Dest;
                    case OperandSize.Halfword:
                        return "sh " + Src + ", " + Dest;
                    case OperandSize.Word:
                        return "sw " + Src + ", " + Dest;
                    default:
                        throw new NotSupportedException("MIPS: Unrecognised move operand sizes. (DestIsMemory)");
                }
            }
            else if (MoveType == MoveTypes.SrcMemoryToDestReg)
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
            else if (MoveType == MoveTypes.ImmediateToReg)
            {
                return "add " + Dest + ", $zero, " + Src;
            }
            else
            {
                return "move " + Dest + ", " + Src;
            }
        }
    }
}
