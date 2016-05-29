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

namespace Drivers.Compiler.Architectures.MIPS32.ASMOps
{
    public class Branch : ASMOp
    {
        public BranchOp BranchType;
        public int DestILPosition;
        public string Extension;
        public string Src1;
        public string Src2;
        public bool UnsignedTest;

        public override string Convert(ASMBlock TheBlock)
        {
            string jmpOp = "";
            int numSourceOperands = 2;
            switch (BranchType)
            {
                case BranchOp.Branch:
                    jmpOp = "b";
                    numSourceOperands = 0;
                    break;
                case BranchOp.BranchZero:
                    jmpOp = "beqz";
                    numSourceOperands = 1;
                    break;
                case BranchOp.BranchNotZero:
                    jmpOp = "bne";
                    Src2 = "$zero";
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

            string label = TheBlock.GenerateMethodLabel() + TheBlock.GenerateILOpLabel(DestILPosition, Extension);

            if (numSourceOperands == 2)
            {
                return jmpOp + " " + Src1 + ", " + Src2 + ", " + label + "\nnop";
            }
            if (numSourceOperands == 1)
            {
                return jmpOp + " " + Src1 + ", " + label + "\nnop";
            }
            return jmpOp + " " + label + "\nnop";
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