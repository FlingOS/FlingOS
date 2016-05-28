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
    public class Mul : IL.ILOps.Mul
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
                     itemB.sizeOnStackInBytes == 8)
            {
                conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                {
                    isFloat = false,
                    isNewGCObject = false,
                    sizeOnStackInBytes = 8,
                    isGCManaged = false,
                    isValue = itemA.isValue && itemB.isValue
                });
            }
        }

        /// <summary>
        ///     See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        ///     Thrown if either or both values to multiply are floating point values or
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


            if (itemB.sizeOnStackInBytes < 4 ||
                itemA.sizeOnStackInBytes < 4)
            {
                throw new InvalidOperationException("Invalid stack operand sizes!");
            }
            if (itemB.isFloat || itemA.isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Divide floats is unsupported!");
            }
            if (itemA.sizeOnStackInBytes == 4 &&
                itemB.sizeOnStackInBytes == 4)
            {
                //Pop item B
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t1"});
                //Pop item A
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t0"});
                //Do the multiplication
                conversionState.Append(new ASMOps.Mul {Src1 = "$t0", Src2 = "$t1", Signed = true});
                //Load the result
                conversionState.Append(new Mflo {Dest = "$t0"});
                //Result stored in $t0
                conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t0"});

                conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                {
                    isFloat = false,
                    sizeOnStackInBytes = 4,
                    isGCManaged = false,
                    isValue = itemA.isValue && itemB.isValue
                });
            }
            else if ((itemA.sizeOnStackInBytes == 8 &&
                      itemB.sizeOnStackInBytes == 4) ||
                     (itemA.sizeOnStackInBytes == 4 &&
                      itemB.sizeOnStackInBytes == 8))
            {
                throw new InvalidOperationException("Invalid stack operand sizes! They should be 32-32 or 64-64.");
            }
            else if (itemA.sizeOnStackInBytes == 8 &&
                     itemB.sizeOnStackInBytes == 8)
            {
                Logger.LogWarning(Errors.ILCompiler_ScanILOpCustomWarning_ErrorCode, "", 0,
                    string.Format(Errors.ErrorMessages[Errors.ILCompiler_ScanILOpCustomWarning_ErrorCode],
                        "All 64-bit multiplication is treated as unsigned. Ensure you didn't intend signed 64-bit multiplication. Signed 64-bit multiplication is not supported yet."));

                //A = item A, B = item B
                //L = low bits, H = high bits
                // => A = AL + AH, B = BL + BH

                // A * B = (AL + AH) * (BL + BH)
                //       = (AL * BL) + (AL * BH) + (AH * BL) (Ignore: + (AH * BH))

                // AH = 12($sp)
                // AL = 8($sp)
                // BH = 4($sp)
                // BL = 0($sp)

                // mov $t0, 0        - Zero out registers
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "0",
                    Dest = "$t0",
                    MoveType = Mov.MoveTypes.ImmediateToReg
                });
                // mov $t1, 0
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "0",
                    Dest = "$t1",
                    MoveType = Mov.MoveTypes.ImmediateToReg
                });
                // mov $t2, 0
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "0",
                    Dest = "$t2",
                    MoveType = Mov.MoveTypes.ImmediateToReg
                });
                // mov $t3, 0
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "0",
                    Dest = "$t3",
                    MoveType = Mov.MoveTypes.ImmediateToReg
                });

                // mov $t0, 0($sp) - Load BL
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "0($sp)",
                    Dest = "$t0",
                    MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                });
                // mov $t1, 8($sp) - Load AL
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "8($sp)",
                    Dest = "$t1",
                    MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                });
                // mul $t1           - BL * AL, result in $lo:$hi
                conversionState.Append(new ASMOps.Mul {Src1 = "$t0", Src2 = "$t1", Signed = false});
                conversionState.Append(new Mfhi {Dest = "$t3"});
                conversionState.Append(new Mflo {Dest = "$t0"});
                // push $t3          - Push result keeping high bits
                conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t3"});
                // push $t0
                conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t0"});

                //                   - Add 8 to offsets for result(s)

                // mov $t0, 0        - Zero out registers
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "0",
                    Dest = "$t0",
                    MoveType = Mov.MoveTypes.ImmediateToReg
                });
                // mov $t3, 0
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "0",
                    Dest = "$t3",
                    MoveType = Mov.MoveTypes.ImmediateToReg
                });
                // mov $t0 4+8($sp) - Load BH
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "12($sp)",
                    Dest = "$t0",
                    MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                });
                // mul $t1           - BH * AL, result in $lo:$hi
                conversionState.Append(new ASMOps.Mul {Src1 = "$t0", Src2 = "$t1"});
                conversionState.Append(new Mflo {Dest = "$t0"});
                // push $t0          - Push result truncating high bits
                conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t0"});

                //                   - Add 12 to offsets for result(s)

                // mov $t0, 0        - Zero out registers
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "0",
                    Dest = "$t0",
                    MoveType = Mov.MoveTypes.ImmediateToReg
                });
                // mov $t3, 0
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "0",
                    Dest = "$t3",
                    MoveType = Mov.MoveTypes.ImmediateToReg
                });
                // mov $t0, 0+12($sp) - Load BL
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "12($sp)",
                    Dest = "$t0",
                    MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                });
                // mov $t1, 12+12($sp) - Load AH
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "24($sp)",
                    Dest = "$t1",
                    MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                });
                // mul $t1             - BL * AH, result in $lo:$hi
                conversionState.Append(new ASMOps.Mul {Src1 = "$t0", Src2 = "$t1"});
                conversionState.Append(new Mflo {Dest = "$t0"});
                // push $t0            - Push result truncating high bits
                conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t0"});

                //                     - Add 16 to offsets for result(s)

                // AL * BL = 8($sp) , 64 bits
                // AL * BH = 4($sp) , 32 bits - high bits
                // AH * BL = 0($sp) , 32 bits - high bits

                // mov $t0, 8($sp)  - Load AL * BL
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "8($sp)",
                    Dest = "$t0",
                    MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                });
                // mov $t3, 12($sp)
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "12($sp)",
                    Dest = "$t3",
                    MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                });
                // mov $t1, 0
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "0",
                    Dest = "$t1",
                    MoveType = Mov.MoveTypes.ImmediateToReg
                });
                // mov $t2, 4($sp)   - Load AL * BH
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "4($sp)",
                    Dest = "$t2",
                    MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                });
                // add $t3, $t2       - Add (AL * BL) + (AL * BH), result in $lo:$hi
                conversionState.Append(new ASMOps.Add {Src1 = "$t2", Src2 = "$t3", Dest = "$t3"});
                // mov $t2, 0($sp)   - Load AH * BL
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "0($sp)",
                    Dest = "$t2",
                    MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                });
                // add $t3, $t2       - Add ((AL * BL) + (AL * BH)) + (AH * BL), result in $lo:$hi
                conversionState.Append(new ASMOps.Add {Src1 = "$t2", Src2 = "$t3", Dest = "$t3"});

                // add $sp, 16+16     - Remove temp results and input values from stack
                conversionState.Append(new ASMOps.Add {Src1 = "$sp", Src2 = "32", Dest = "$sp"});

                // push $t3           - Push final result
                conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t3"});
                // push $t0
                conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t0"});

                conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                {
                    isFloat = false,
                    isNewGCObject = false,
                    sizeOnStackInBytes = 8,
                    isGCManaged = false,
                    isValue = itemA.isValue && itemB.isValue
                });
            }
        }
    }
}