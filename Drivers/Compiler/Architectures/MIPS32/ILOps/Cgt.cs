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
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.MIPS32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Cgt : IL.ILOps.Cgt
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            StackItem itemB = conversionState.CurrentStackFrame.Stack.Pop();
            StackItem itemA = conversionState.CurrentStackFrame.Stack.Pop();

            if (itemA.sizeOnStackInBytes == 4 && itemB.sizeOnStackInBytes == 4)
            {
                conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = false,
                    sizeOnStackInBytes = 4,
                    isGCManaged = false,
                    isValue = true
                });
            }
            else if (itemA.sizeOnStackInBytes == 8 && itemB.sizeOnStackInBytes == 8)
            {
                conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = false,
                    sizeOnStackInBytes = 4,
                    isGCManaged = false,
                    isValue = true
                });
            }
        }

        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if compare operands are floating point numbers or if either value is &lt; 4 bytes in length or
        /// operands are not of the same size.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Pop in reverse order to push
            StackItem itemB = conversionState.CurrentStackFrame.Stack.Pop();
            StackItem itemA = conversionState.CurrentStackFrame.Stack.Pop();

            bool unsignedComparison = (OpCodes)theOp.opCode.Value == OpCodes.Cgt_Un;
            int currOpPosition = conversionState.PositionOf(theOp);

            if (itemB.isFloat || itemA.isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Add floats is unsupported!");
            }
            else if(itemA.sizeOnStackInBytes == 4 && itemB.sizeOnStackInBytes == 4)
            {
                //Pop item B
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t1" });
                //Pop item A
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });
                //If A <= B, jump to Else (not-true case)
                conversionState.Append(new ASMOps.Branch() { Src1 = "$t0", Src2 = "$t1", BranchType = ASMOps.BranchOp.BranchLessThanEqual, DestILPosition = currOpPosition, Extension = "Else", UnsignedTest = unsignedComparison });
                //Otherwise, A > B, so push true (true=1)
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Dest = "$t4", Src = "1", MoveType = ASMOps.Mov.MoveTypes.ImmediateToReg });
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t4" });
                //And then jump to the end of this IL op.
                conversionState.Append(new ASMOps.Branch() { BranchType = ASMOps.BranchOp.Branch, DestILPosition = currOpPosition, Extension = "End" });
                //Insert the Else label.
                conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "Else" });
                //Else case - Push false (false=0)
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$zero" });
                
                //Push the result onto our stack
                conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = false,
                    sizeOnStackInBytes = 4,
                    isGCManaged = false,
                    isValue = true
                });
            }
            else if (itemA.sizeOnStackInBytes == 8 && itemB.sizeOnStackInBytes == 8)
            {
                //Pop item B
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t1" });
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t2" });
                //Pop item A
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t3" });
                //If A high bytes > B high bytes : True
                //If A high bytes = B high bytes : Check, if A low bytes > B low bytes : True
                //Else : False

                //A high bytes > B high bytes? Branch to true case.
                conversionState.Append(new ASMOps.Branch() { Src1 = "$t3", Src2 = "$t2", BranchType = ASMOps.BranchOp.BranchGreaterThan, DestILPosition = currOpPosition, Extension = "True", UnsignedTest = unsignedComparison });
                //A high bytes < B high bytes? Branch to else case.
                conversionState.Append(new ASMOps.Branch() { BranchType = ASMOps.BranchOp.BranchLessThan, DestILPosition = currOpPosition, Extension = "Else", UnsignedTest = unsignedComparison });
                //Otherwise, A high bytes = B high bytes
                //A low bytes <= B low bytes? Branch to else case.
                conversionState.Append(new ASMOps.Branch() { Src1 = "$t0", Src2 = "$t1", BranchType = ASMOps.BranchOp.BranchLessThanEqual, DestILPosition = currOpPosition, Extension = "Else", UnsignedTest = unsignedComparison });

                //Otherwise A > B.

                //Insert True case label
                conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "True" });
                //True case - Push true (true=1)
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Dest = "$t4", Src = "1", MoveType = ASMOps.Mov.MoveTypes.ImmediateToReg });
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t4" });
                //And then jump to the end of this IL op.
                conversionState.Append(new ASMOps.Branch() { BranchType = ASMOps.BranchOp.Branch, DestILPosition = currOpPosition, Extension = "End" });
                //Insert Else case label
                conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "Else" });
                //Else case - Push false (false=0)
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$zero" });
                
                //Push the result onto our stack
                conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = false,
                    // Yes, this is supposed to be 4 - the value that just got pushed was a 
                    // true / false integer
                    sizeOnStackInBytes = 4,
                    isGCManaged = false,
                    isValue = true
                });
            }
            else
            {
                throw new NotSupportedException("Unsupported number of bytes for compare greater than!");
            }

            //Always append the end label
            conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "End" });
        }
    }
}
