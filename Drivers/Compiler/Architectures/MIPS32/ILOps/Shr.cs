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
using Drivers.Compiler.Architectures.MIPS32.ASMOps;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.MIPS32
{
    /// <summary>
    ///     See base class documentation.
    /// </summary>
    public class Shr : IL.ILOps.Shr
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            //Pop in reverse order to push
            StackItem itemB = conversionState.CurrentStackFrame.GetStack(theOp).Pop();
            StackItem itemA = conversionState.CurrentStackFrame.GetStack(theOp).Pop();

            if (itemA.sizeOnStackInBytes == 4 &&
                itemB.sizeOnStackInBytes == 4)
            {
                conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                {
                    isFloat = false,
                    sizeOnStackInBytes = 4,
                    isGCManaged = false,
                    isValue = itemA.isValue && itemB.isValue
                });
            }
            else if (itemA.sizeOnStackInBytes == 8 &&
                     itemB.sizeOnStackInBytes == 4)
            {
                if ((OpCodes)theOp.opCode.Value == OpCodes.Shr_Un)
                {
                    conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                    {
                        isFloat = false,
                        sizeOnStackInBytes = 8,
                        isGCManaged = false,
                        isValue = itemA.isValue && itemB.isValue
                    });
                }
                else
                {
                    conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                    {
                        isFloat = false,
                        sizeOnStackInBytes = 8,
                        isGCManaged = false,
                        isValue = itemA.isValue && itemB.isValue
                    });
                }
            }
            else if (itemA.sizeOnStackInBytes == 8 &&
                     itemB.sizeOnStackInBytes == 8)
            {
                if ((OpCodes)theOp.opCode.Value == OpCodes.Shr_Un)
                {
                    conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                    {
                        isFloat = false,
                        sizeOnStackInBytes = 8,
                        isGCManaged = false,
                        isValue = itemA.isValue && itemB.isValue
                    });
                }
                else
                {
                    conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                    {
                        isFloat = false,
                        sizeOnStackInBytes = 8,
                        isGCManaged = false,
                        isValue = itemA.isValue && itemB.isValue
                    });
                }
            }
        }

        /// <summary>
        ///     See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        ///     Thrown if either or both values to shift left are floating point values or
        ///     if the values are 8 bytes in size.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        ///     Thrown if either or both values to multiply are not 4 or 8 bytes
        ///     in size or if the values are of different size.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Pop in reverse order to push
            StackItem itemB = conversionState.CurrentStackFrame.GetStack(theOp).Pop();
            StackItem itemA = conversionState.CurrentStackFrame.GetStack(theOp).Pop();

            int currOpPosition = conversionState.PositionOf(theOp);

            if (itemB.sizeOnStackInBytes < 4 ||
                itemA.sizeOnStackInBytes < 4)
            {
                throw new InvalidOperationException("Invalid stack operand sizes!");
            }
            if (itemB.isFloat || itemA.isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Shift right on floats is unsupported!");
            }
            bool SignedShift = (OpCodes)theOp.opCode.Value != OpCodes.Shr_Un;

            if (itemA.sizeOnStackInBytes == 4 &&
                itemB.sizeOnStackInBytes == 4)
            {
                if (!SignedShift)
                {
                    //Pop item B
                    conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t2"});
                    //Pop item A
                    conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t0"});

                    conversionState.Append(new Srlv {Dest = "$t0", Src = "$t0", BitsReg = "$t2"});
                    conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t0"});

                    conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                    {
                        isFloat = false,
                        sizeOnStackInBytes = 4,
                        isGCManaged = false,
                        isValue = itemA.isValue && itemB.isValue
                    });
                }
                else
                {
                    //Pop item B
                    conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t2"});
                    //Pop item A
                    conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t0"});

                    conversionState.Append(new Srav {Dest = "$t0", Src = "$t0", BitsReg = "$t2"});
                    conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t0"});

                    conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                    {
                        isFloat = false,
                        sizeOnStackInBytes = 4,
                        isGCManaged = false,
                        isValue = itemA.isValue && itemB.isValue
                    });
                }
            }
            else if (itemA.sizeOnStackInBytes == 4 &&
                     itemB.sizeOnStackInBytes == 8)
            {
                throw new InvalidOperationException("Invalid stack operand sizes! 4,8 not supported.");
            }
            else if (itemA.sizeOnStackInBytes == 8 &&
                     itemB.sizeOnStackInBytes == 4)
            {
                if (!SignedShift)
                {
                    //Pop item B
                    conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t2"});
                    //Pop item A (8 bytes)
                    conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t0"});
                    conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t3"});

                    //Check shift size
                    conversionState.Append(new Branch
                    {
                        Src1 = "$t2",
                        Src2 = "32",
                        BranchType = BranchOp.BranchGreaterThanEqual,
                        DestILPosition = currOpPosition,
                        Extension = "ShiftMoreThan32",
                        UnsignedTest = true
                    });

                    //Shr (< 32)
                    //Works for shifting 64 bit values
                    //Right shift (logical) low bits ($t0) by $t2
                    conversionState.Append(new Srlv {Src = "$t0", BitsReg = "$t2", Dest = "$t0"});
                    //Left shift high bits ($t3) by (32-$t2) into temp ($t1)
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Word,
                        Src = "32",
                        Dest = "$t1",
                        MoveType = Mov.MoveTypes.ImmediateToReg
                    });
                    conversionState.Append(new ASMOps.Sub {Src1 = "$t1", Src2 = "$t2", Dest = "$t1"});
                    conversionState.Append(new Sllv {Src = "$t3", BitsReg = "$t1", Dest = "$t1"}); //$t1 = temp
                    //Copy temp to low bits
                    conversionState.Append(new ASMOps.Or {Src1 = "$t1", Src2 = "$t0", Dest = "$t0"});
                    //Right shift (logical) high bits by $t2
                    conversionState.Append(new Srlv {Src = "$t3", BitsReg = "$t2", Dest = "$t3"});
                    conversionState.Append(new Branch
                    {
                        BranchType = BranchOp.Branch,
                        DestILPosition = currOpPosition,
                        Extension = "End"
                    });

                    //Shr (>= 32)
                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "ShiftMoreThan32"});
                    //Move high bits ($t3) to low bits ($t0)
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Word,
                        Src = "$t3",
                        Dest = "$t0",
                        MoveType = Mov.MoveTypes.RegToReg
                    });
                    //Zero out high bits
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Word,
                        Src = "0",
                        Dest = "$t3",
                        MoveType = Mov.MoveTypes.ImmediateToReg
                    });
                    //Right shift (logical) low bits by (t2-32)
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Word,
                        Src = "32",
                        Dest = "$t1",
                        MoveType = Mov.MoveTypes.ImmediateToReg
                    });
                    conversionState.Append(new ASMOps.Sub {Src1 = "$t2", Src2 = "$t1", Dest = "$t2"});
                    conversionState.Append(new Srlv {Src = "$t0", BitsReg = "$t2", Dest = "$t0"});

                    //Push result
                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "End"});
                    conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t3"});
                    conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t0"});

                    conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                    {
                        isFloat = false,
                        sizeOnStackInBytes = 8,
                        isGCManaged = false,
                        isValue = itemA.isValue && itemB.isValue
                    });
                }
                else
                {
                    //Pop item B
                    conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t2"});
                    //Pop item A (8 bytes)
                    conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t0"});
                    conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t3"});

                    //Check shift size
                    conversionState.Append(new Branch
                    {
                        Src1 = "$t2",
                        Src2 = "32",
                        BranchType = BranchOp.BranchGreaterThanEqual,
                        DestILPosition = currOpPosition,
                        Extension = "ShiftMoreThan32",
                        UnsignedTest = true
                    });

                    //Shr (< 32)
                    //Works for shifting 64 bit values
                    //Right shift (logical) low bits ($t0) by $t2
                    conversionState.Append(new Srlv {Src = "$t0", BitsReg = "$t2", Dest = "$t0"});
                    //Left shift high bits ($t3) by (32-$t2) into temp ($t1)
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Word,
                        Src = "32",
                        Dest = "$t1",
                        MoveType = Mov.MoveTypes.ImmediateToReg
                    });
                    conversionState.Append(new ASMOps.Sub {Src1 = "$t1", Src2 = "$t2", Dest = "$t1"});
                    conversionState.Append(new Sllv {Src = "$t3", BitsReg = "$t1", Dest = "$t1"}); //$t1 = temp
                    //Copy temp to low bits
                    conversionState.Append(new ASMOps.Or {Src1 = "$t1", Src2 = "$t0", Dest = "$t0"});
                    //Right shift (arithmetic) high bits by $t2
                    conversionState.Append(new Srav {Src = "$t3", BitsReg = "$t2", Dest = "$t3"});
                    conversionState.Append(new Branch
                    {
                        BranchType = BranchOp.Branch,
                        DestILPosition = currOpPosition,
                        Extension = "End"
                    });

                    //Shr (>= 32)
                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "ShiftMoreThan32"});
                    //Move high bits ($t3) to low bits ($t0)
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Word,
                        Src = "$t3",
                        Dest = "$t0",
                        MoveType = Mov.MoveTypes.RegToReg
                    });
                    //Conserve sign
                    //Right shift (arithmetic) high bits by 31 to conserve sign
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Word,
                        Src = "31",
                        Dest = "$t1",
                        MoveType = Mov.MoveTypes.ImmediateToReg
                    });
                    conversionState.Append(new Srav {Src = "$t3", BitsReg = "$t1", Dest = "$t3"});

                    //Right shift (arithmetic) low bits by (t2-32)
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Word,
                        Src = "32",
                        Dest = "$t1",
                        MoveType = Mov.MoveTypes.ImmediateToReg
                    });
                    conversionState.Append(new ASMOps.Sub {Src1 = "$t2", Src2 = "$t1", Dest = "$t2"});
                    conversionState.Append(new Srav {Src = "$t0", BitsReg = "$t2", Dest = "$t0"});

                    //Push result
                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "End"});
                    conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t3"});
                    conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t0"});

                    conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                    {
                        isFloat = false,
                        sizeOnStackInBytes = 8,
                        isGCManaged = false,
                        isValue = itemA.isValue && itemB.isValue
                    });
                }
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

                if (!SignedShift)
                {
                    //Pop item B (8 bytes)
                    conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t2"});
                    conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t1"});
                    //Pop item A (8 bytes)
                    conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t0"});
                    conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t3"});
                    //Check high 4 bytes of second param     
                    conversionState.Append(new Branch
                    {
                        Src1 = "$t1",
                        BranchType = BranchOp.BranchZero,
                        DestILPosition = currOpPosition,
                        Extension = "Zero"
                    });
                    conversionState.Append(new Push {Size = OperandSize.Word, Src = "$zero"});
                    conversionState.Append(new Push {Size = OperandSize.Word, Src = "$zero"});
                    conversionState.Append(new Branch
                    {
                        BranchType = BranchOp.Branch,
                        DestILPosition = currOpPosition,
                        Extension = "End2"
                    });
                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "Zero"});
                    conversionState.Append(new Branch
                    {
                        Src1 = "$t2",
                        Src2 = "32",
                        BranchType = BranchOp.BranchGreaterThanEqual,
                        DestILPosition = currOpPosition,
                        Extension = "ShiftMoreThan32",
                        UnsignedTest = true
                    });

                    //Shrd (< 32)
                    //Works for shifting 64 bit values
                    //Right shift (logical) low bits ($t0) by $t2
                    conversionState.Append(new Srlv {Src = "$t0", BitsReg = "$t2", Dest = "$t0"});
                    //Left shift high bits ($t3) by (32-$t2) into temp
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Word,
                        Src = "32",
                        Dest = "$t1",
                        MoveType = Mov.MoveTypes.ImmediateToReg
                    });
                    conversionState.Append(new ASMOps.Sub {Src1 = "$t1", Src2 = "$t2", Dest = "$t1"});
                    conversionState.Append(new Sllv {Src = "$t3", BitsReg = "$t1", Dest = "$t1"}); //$t1 = temp
                    //Copy temp to low bits
                    conversionState.Append(new ASMOps.Or {Src1 = "$t1", Src2 = "$t0", Dest = "$t0"});
                    //Right shift (logical) high bits by $t2
                    conversionState.Append(new Srlv {Src = "$t3", BitsReg = "$t2", Dest = "$t3"});
                    conversionState.Append(new Branch
                    {
                        BranchType = BranchOp.Branch,
                        DestILPosition = currOpPosition,
                        Extension = "End1"
                    });

                    //Shr (>= 32)
                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "ShiftMoreThan32"});
                    //Move high bits ($t3) to low bits ($t0)
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Word,
                        Src = "$t3",
                        Dest = "$t0",
                        MoveType = Mov.MoveTypes.RegToReg
                    });
                    //Zero out high bits
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Word,
                        Src = "0",
                        Dest = "$t3",
                        MoveType = Mov.MoveTypes.ImmediateToReg
                    });
                    //Right shift (logical) low bits by (t2-32)
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Word,
                        Src = "32",
                        Dest = "$t1",
                        MoveType = Mov.MoveTypes.ImmediateToReg
                    });
                    conversionState.Append(new ASMOps.Sub {Src1 = "$t2", Src2 = "$t1", Dest = "$t2"});
                    conversionState.Append(new Srlv {Src = "$t0", BitsReg = "$t2", Dest = "$t0"});

                    //Push result
                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "End1"});
                    conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t3"});
                    conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t0"});
                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "End2"});

                    conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                    {
                        isFloat = false,
                        sizeOnStackInBytes = 8,
                        isGCManaged = false,
                        isValue = itemA.isValue && itemB.isValue
                    });
                }
                else
                {
                    //Pop item B (8 bytes)
                    conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t2"});
                    conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t1"});
                    //Pop item A (8 bytes)
                    conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t0"});
                    conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t3"});
                    //Check high 4 bytes of second param     
                    conversionState.Append(new Branch
                    {
                        Src1 = "$t1",
                        BranchType = BranchOp.BranchZero,
                        DestILPosition = currOpPosition,
                        Extension = "Zero"
                    });
                    conversionState.Append(new Push {Size = OperandSize.Word, Src = "$zero"});
                    conversionState.Append(new Push {Size = OperandSize.Word, Src = "$zero"});
                    conversionState.Append(new Branch
                    {
                        BranchType = BranchOp.Branch,
                        DestILPosition = currOpPosition,
                        Extension = "End2"
                    });
                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "zero"});
                    conversionState.Append(new Branch
                    {
                        Src1 = "$t2",
                        Src2 = "32",
                        BranchType = BranchOp.BranchGreaterThanEqual,
                        DestILPosition = currOpPosition,
                        Extension = "ShiftMoreThan32",
                        UnsignedTest = true
                    });

                    //Shrd (< 32)
                    //Works for shifting 64 bit values
                    //Right shift (logical) low bits ($t0) by $t2
                    conversionState.Append(new Srlv {Src = "$t0", BitsReg = "$t2", Dest = "$t0"});
                    //Left shift high bits ($t3) by (32-$t2) into temp
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Word,
                        Src = "32",
                        Dest = "$t1",
                        MoveType = Mov.MoveTypes.ImmediateToReg
                    });
                    conversionState.Append(new ASMOps.Sub {Src1 = "$t1", Src2 = "$t2", Dest = "$t1"});
                    conversionState.Append(new Sllv {Src = "$t3", BitsReg = "$t1", Dest = "$t1"}); //$t1 = temp
                    //Copy temp to low bits
                    conversionState.Append(new ASMOps.Or {Src1 = "$t1", Src2 = "$t0", Dest = "$t0"});
                    //Right shift (arithmetic) high bits by $t2
                    conversionState.Append(new Srav {Src = "$t3", BitsReg = "$t2", Dest = "$t3"});
                    conversionState.Append(new Branch
                    {
                        BranchType = BranchOp.Branch,
                        DestILPosition = currOpPosition,
                        Extension = "End1"
                    });

                    //Shr (>= 32)
                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "ShiftMoreThan32"});
                    //Move high bits ($t3) to low bits ($t0)
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Word,
                        Src = "$t3",
                        Dest = "$t0",
                        MoveType = Mov.MoveTypes.RegToReg
                    });
                    //Conserve sign

                    //Right shift (arithmetic) high bits by 31 to conserve sign
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Word,
                        Src = "31",
                        Dest = "$t1",
                        MoveType = Mov.MoveTypes.ImmediateToReg
                    });
                    conversionState.Append(new Srav {Src = "$t3", BitsReg = "$t1", Dest = "$t3"});

                    //Right shift (arithmetic) low bits by (t2-32)
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Word,
                        Src = "32",
                        Dest = "$t1",
                        MoveType = Mov.MoveTypes.ImmediateToReg
                    });
                    conversionState.Append(new ASMOps.Sub {Src1 = "$t2", Src2 = "$t1", Dest = "$t2"});
                    conversionState.Append(new Srav {Src = "$t0", BitsReg = "$t2", Dest = "$t0"});

                    //Push result
                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "End1"});
                    conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t3"});
                    conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t0"});
                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "End2"});

                    conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
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