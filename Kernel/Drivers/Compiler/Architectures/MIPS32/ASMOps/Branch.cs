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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drivers.Compiler.Architectures.MIPS32.ASMOps
{
    public class Branch : ASM.ASMOp
    {
        public string Src1;
        public string Src2;

        public BranchOp BranchType;
        public bool UnsignedTest;
        public int DestILPosition;
        public string Extension;
        
        public override string Convert(ASM.ASMBlock theBlock)
        {
            string jmpOp = "";
            bool includeSourceOperands = true;
            switch (BranchType)
            {
                case BranchOp.Branch:
                    jmpOp = "b";
                    includeSourceOperands = false;
                    break;
                case BranchOp.BranchZero:
                    jmpOp = "beqz";
                    break;
                case BranchOp.BranchNotZero:
                    jmpOp = "bne";
                    Src2 = "$0";
                    break;
                case BranchOp.BranchEqual:
                    jmpOp = "beq";
                    break;
                case BranchOp.BranchNotEqual:
                    jmpOp = "bne";
                    break;
                case BranchOp.BranchLessThan:
                    if (UnsignedTest)
                    {
                        jmpOp = "bltu";
                    }
                    else
                    {
                        jmpOp = "blt";
                    }
                    break;
                case BranchOp.BranchGreaterThan:
                    if (UnsignedTest)
                    {
                        jmpOp = "bgtu";
                    }
                    else
                    {
                        jmpOp = "bgt";
                    }
                    break;
                case BranchOp.BranchLessThanEqual:
                    if (UnsignedTest)
                    {
                        jmpOp = "bleu";
                    }
                    else
                    {
                        jmpOp = "ble";
                    }
                    break;
                case BranchOp.BranchGreaterThanEqual:
                    if (UnsignedTest)
                    {
                        jmpOp = "bgeu";
                    }
                    else
                    {
                        jmpOp = "bge";
                    }
                    break;
            }

            if (includeSourceOperands)
            {
                return jmpOp + " " + Src1 + ", " + Src2 + ", " + theBlock.GenerateILOpLabel(DestILPosition, Extension);
            }
            else
            {
                return jmpOp + theBlock.GenerateILOpLabel(DestILPosition, Extension);
            }
        }
    }
    public enum BranchOp
    {
        None,
        Branch,
        BranchZero,
        BranchNotZero,
        BranchEqual,
        BranchNotEqual,
        BranchLessThan,
        BranchGreaterThan,
        BranchLessThanEqual,
        BranchGreaterThanEqual
    }
}
