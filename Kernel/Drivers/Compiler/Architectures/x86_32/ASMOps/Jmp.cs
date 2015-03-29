using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.x86.ASMOps
{
    public class Jmp : ASM.ASMOp
    {
        public JmpOp JumpType;
        public bool UnsignedTest;
        public int DestILPosition;
        public string Extension;
        
        public override string Convert(ASM.ASMBlock theBlock)
        {
            string jmpOp = "";
            switch (JumpType)
            {
                case JmpOp.Jump:
                    jmpOp = "jmp";
                    break;
                case JmpOp.JumpZero:
                    jmpOp = "jz";
                    break;
                case JmpOp.JumpNotZero:
                    jmpOp = "jnz";
                    break;
                case JmpOp.JumpEqual:
                    jmpOp = "je";
                    break;
                case JmpOp.JumpNotEqual:
                    jmpOp = "jne";
                    break;
                case JmpOp.JumpLessThan:
                    if (UnsignedTest)
                    {
                        jmpOp = "jb";
                    }
                    else
                    {
                        jmpOp = "jl";
                    }
                    break;
                case JmpOp.JumpGreaterThan:
                    if (UnsignedTest)
                    {
                        jmpOp = "ja";
                    }
                    else
                    {
                        jmpOp = "jg";
                    }
                    break;
                case JmpOp.JumpLessThanEqual:
                    if (UnsignedTest)
                    {
                        jmpOp = "jbe";
                    }
                    else
                    {
                        jmpOp = "jle";
                    }
                    break;
                case JmpOp.JumpGreaterThanEqual:
                    if (UnsignedTest)
                    {
                        jmpOp = "jae";
                    }
                    else
                    {
                        jmpOp = "jge";
                    }
                    break;
            }

            return jmpOp + " " + theBlock.GenerateILOpLabel(DestILPosition, Extension);
        }
    }
    public enum JmpOp
    {
        None,
        Jump,
        JumpZero,
        JumpNotZero,
        JumpEqual,
        JumpNotEqual,
        JumpLessThan,
        JumpGreaterThan,
        JumpLessThanEqual,
        JumpGreaterThanEqual
    }
}
