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

using System;
using Drivers.Compiler.ASM;

namespace Drivers.Compiler.Architectures.MIPS32.ASMOps
{
    public class Mov : ASMOp
    {
        public enum MoveTypes
        {
            SrcMemoryToDestReg,
            SrcRegToDestMemory,
            ImmediateToReg,
            RegToReg
        }

        public string Dest;
        public MoveTypes MoveType = MoveTypes.RegToReg;
        public bool SignExtend = false;

        public OperandSize Size;
        public string Src;

        public override string Convert(ASMBlock TheBlock)
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
            if (MoveType == MoveTypes.SrcMemoryToDestReg)
            {
                if (SignExtend)
                {
                    switch (Size)
                    {
                        case OperandSize.Byte:
                            return "lb " + Dest + ", " + Src;
                        case OperandSize.Halfword:
                            return "lh " + Dest + ", " + Src;
                        case OperandSize.Word:
                            return "lw " + Dest + ", " + Src;
                        default:
                            throw new NotSupportedException("MIPS: Unrecognised move operand sizes. (SrcIsMemory)");
                    }
                }
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
            if (MoveType == MoveTypes.ImmediateToReg)
            {
                return "add " + Dest + ", $zero, " + Src;
            }
            return "move " + Dest + ", " + Src;
        }
    }
}