#region LICENSE

// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Project owner: 
//		Email: edwardnutting@outlook.com
//		For paper mail address, please contact via email for details.
//
// ------------------------------------------------------------------------------ //

#endregion

using Drivers.Compiler.ASM;

namespace Drivers.Compiler.Architectures.x86.ASMOps
{
    public class Jmp : ASMOp
    {
        public int DestILPosition;
        public string Extension;
        public JmpOp JumpType;
        public bool UnsignedTest;

        public override string Convert(ASMBlock TheBlock)
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

            return jmpOp + " " + TheBlock.GenerateILOpLabel(DestILPosition, Extension);
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