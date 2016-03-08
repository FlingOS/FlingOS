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
    public class Shl : IL.ILOps.Shl
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            //Pop in reverse order to push
            StackItem itemB = conversionState.CurrentStackFrame.Stack.Pop();
            StackItem itemA = conversionState.CurrentStackFrame.Stack.Pop();

            if (itemA.sizeOnStackInBytes == 4 &&
                itemB.sizeOnStackInBytes == 4)
            {
                conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = false,
                    sizeOnStackInBytes = 4,
                    isGCManaged = false,
                    isValue = itemA.isValue && itemB.isValue
                });
            }
            else if ((itemA.sizeOnStackInBytes == 8 &&
                itemB.sizeOnStackInBytes == 4))
            {
                conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = false,
                    sizeOnStackInBytes = 8,
                    isGCManaged = false,
                    isValue = itemA.isValue && itemB.isValue
                });
            }
            else if (itemA.sizeOnStackInBytes == 8 &&
                itemB.sizeOnStackInBytes == 8)
            {
                conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = false,
                    sizeOnStackInBytes = 8,
                    isGCManaged = false,
                    isValue = itemA.isValue && itemB.isValue
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
        /// Thrown if either or both values to shift left are floating point values or
        /// if the values are 8 bytes in size.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if either or both values to multiply are not 4 or 8 bytes
        /// in size or if the values are of different size.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Pop in reverse order to push
            StackItem itemB = conversionState.CurrentStackFrame.Stack.Pop();
            StackItem itemA = conversionState.CurrentStackFrame.Stack.Pop();

            int currOpPosition = conversionState.PositionOf(theOp);

            if (itemB.sizeOnStackInBytes < 4 ||
                itemA.sizeOnStackInBytes < 4)
            {
                throw new InvalidOperationException("Invalid stack operand sizes!");
            }
            else if (itemB.isFloat || itemA.isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Shift left on floats is unsupported!");
            }
            else
            {
                if (itemA.sizeOnStackInBytes == 4 &&
                    itemB.sizeOnStackInBytes == 4)
                {
                    //Pop item B
                    conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t2" });
                    //Pop item A
                    conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });
                    conversionState.Append(new ASMOps.Sllv() { Src = "$t0", BitsReg = "$t2", Dest = "$t0",  });
                    conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t0" });

                    conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                    {
                        isFloat = false,
                        sizeOnStackInBytes = 4,
                        isGCManaged = false,
                        isValue = itemA.isValue && itemB.isValue
                    });
                }
                else if ((itemA.sizeOnStackInBytes == 4 &&
                          itemB.sizeOnStackInBytes == 8))
                {
                    throw new InvalidOperationException("Invalid stack operand sizes! 4,8 not supported.");
                }
                else if ((itemA.sizeOnStackInBytes == 8 &&
                          itemB.sizeOnStackInBytes == 4))
                {
                    //Pop item B
                    conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t2" });
                    //Pop item A (8 bytes)
                    conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });
                    conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t3" });

                    //Check shift size
                    conversionState.Append(new ASMOps.Branch() { Src1 = "$t2", Src2 = "32", BranchType = ASMOps.BranchOp.BranchGreaterThanEqual, DestILPosition = currOpPosition, Extension = "ShiftMoreThan32", UnsignedTest = true });

                    //Shld (< 32)
                    //Works for shifting 64 bit values
                    //Left shift high bits ($t3) by $t2
                    conversionState.Append(new ASMOps.Sllv() { Src = "$t3", BitsReg = "$t2", Dest = "$t3" });
                    //Right shift low bits ($t0) by (32-$t2) into temp ($t1)
                    conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "32", Dest = "$t1", MoveType = ASMOps.Mov.MoveTypes.ImmediateToReg });
                    conversionState.Append(new ASMOps.Sub() { Src1 = "$t1", Src2 = "$t2", Dest = "$t1" });       
                    conversionState.Append(new ASMOps.Srlv() { Src = "$t0", BitsReg = "$t1", Dest = "$t1" });   //$t1 = temp
                    //Copy temp to high bits
                    conversionState.Append(new ASMOps.Or() { Src1 = "$t1", Src2 = "$t3",  Dest = "$t3"});
                    //Left shift low bytes by $t2
                    conversionState.Append(new ASMOps.Sllv() { Src = "$t0", BitsReg = "$t2", Dest = "$t0" });
                    conversionState.Append(new ASMOps.Branch() { BranchType = ASMOps.BranchOp.Branch, DestILPosition = currOpPosition, Extension = "End" });

                    //Shld (>= 32)
                    conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "ShiftMoreThan32" });
                    //Move low bits ($t0) to high bits ($t3)
                    conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "$t0", Dest = "$t3", MoveType = ASMOps.Mov.MoveTypes.RegToReg });
                    //Zero out low bits
                    conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "0", Dest = "$t0", MoveType = ASMOps.Mov.MoveTypes.ImmediateToReg });
                    //Left shift high bits by (t2-32)
                    conversionState.Append(new ASMOps.Sub() { Src1 = "$t2", Src2 = "32", Dest = "$t2" });
                    conversionState.Append(new ASMOps.Sllv() { Src = "$t3", BitsReg = "$t2", Dest = "$t3" });

                    //Push result
                    conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "End" });
                    conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t3" });
                    conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t0" });

                    conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                    {
                        isFloat = false,
                        sizeOnStackInBytes = 8,
                        isGCManaged = false,
                        isValue = itemA.isValue && itemB.isValue
                    });
                }
                else if (itemA.sizeOnStackInBytes == 8 &&
                    itemB.sizeOnStackInBytes == 8)
                {
                    //Note: Shifting by more than 64 bits is pointless since the value will be annihilated entirely.
                    //          "64" fits well within the low 32-bits
                    //      So for this op, we do the same as the 8-4 byte version but discard the top four bytes
                    //          of the second operand
                    //      Except we must check the high bytes for non-zero value. If they are non-zero, we simply
                    //          push a result of zero.

                    //Pop item B (8 bytes)
                    conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t2" });
                    conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t1" });
                    //Pop item A (8 bytes)
                    conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });
                    conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t3" });
                    //Check high 4 bytes of second param     
                    conversionState.Append(new ASMOps.Branch() { Src1 = "$t1", BranchType = ASMOps.BranchOp.BranchZero, DestILPosition = currOpPosition, Extension = "Zero" });
                    conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$zero" });
                    conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$zero" });
                    conversionState.Append(new ASMOps.Branch() { BranchType = ASMOps.BranchOp.Branch, DestILPosition = currOpPosition, Extension = "End2" });
                    conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "zero" });
                    conversionState.Append(new ASMOps.Branch() { Src1 = "$t2", Src2 = "32", BranchType = ASMOps.BranchOp.BranchGreaterThanEqual, DestILPosition = currOpPosition, Extension = "ShiftMoreThan32", UnsignedTest = true });

                    //Shld (< 32)
                    //Works for shifting 64 bit values
                    //Left shift high bits ($t3) by $t2
                    conversionState.Append(new ASMOps.Sllv() { Src = "$t3", BitsReg = "$t2", Dest = "$t3" });
                    //Right shift low bits ($t0) by (32-$t2)
                    conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "32", Dest = "$t1", MoveType = ASMOps.Mov.MoveTypes.ImmediateToReg });
                    conversionState.Append(new ASMOps.Sub() { Src1 = "$t1", Src2 = "$t2", Dest = "$t1" });       
                    conversionState.Append(new ASMOps.Srlv() { Src = "$t0", BitsReg = "$t1", Dest = "$t1" });   //$t1 = temp
                    //Copy temp to high bits
                    conversionState.Append(new ASMOps.Or() { Src1 = "$t1", Src2 = "$t3",  Dest = "$t3"});
                    conversionState.Append(new ASMOps.Sllv() { Src = "$t0", BitsReg = "$t2", Dest = "$t0" });
                    conversionState.Append(new ASMOps.Branch() { BranchType = ASMOps.BranchOp.Branch, DestILPosition = currOpPosition, Extension = "End1" });

                    //Shl (>= 32)
                    conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "ShiftMoreThan32" });
                    //Move low bits ($t0) to high bits ($t3)
                    conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "$t0", Dest = "$t3", MoveType = ASMOps.Mov.MoveTypes.RegToReg });
                    //Zero out low bits
                    conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "0", Dest = "$t0", MoveType = ASMOps.Mov.MoveTypes.ImmediateToReg });
                    //Left shift high bits by (t2-32)
                    conversionState.Append(new ASMOps.Sub() { Src1 = "$t2", Src2 = "32", Dest = "$t2" });
                    conversionState.Append(new ASMOps.Sllv() { Src = "$t3", BitsReg = "$t2", Dest = "$t3" });

                    //Push result
                    conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "End1" });
                    conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t3" });
                    conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t0" });
                    conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "End2" });

                    conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                    {
                        isFloat = false,
                        sizeOnStackInBytes = 8,
                        isGCManaged = false,
                        isValue = itemA.isValue && itemB.isValue
                    });
                }
            }
        }
    }
}
