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
    public class Neg : IL.ILOps.Neg
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            //No effect
        }

        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// If either value is &lt; 4 bytes in length or
        /// operands are not of the same size.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Pop item to negate
            StackItem itemA = conversionState.CurrentStackFrame.Stack.Pop();

            if (itemA.isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Negate float vals not suppported yet!");
            }

            // Two's Complement negation
            //  - "Not" value then add 1

            if (itemA.sizeOnStackInBytes == 4)
            {
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });
                //To negate, (arg XOR -1) + 1
                conversionState.Append(new ASMOps.Mov() { Src = "0xFFFFFFFF", Dest = "$t4", MoveType = ASMOps.Mov.MoveTypes.ImmediateToReg });
                conversionState.Append(new ASMOps.Xor() { Src1 = "$t0", Src2 = "$t4", Dest = "$t0" });
                conversionState.Append(new ASMOps.Add() { Src1 = "$t0", Src2 = "1", Dest = "$t0" });
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t0" });
            }
            else if (itemA.sizeOnStackInBytes == 8)
            {
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t3" });
                // "Not" the value (using XOR with 0xFFFFFFFF)
                conversionState.Append(new ASMOps.Mov() { Src = "0xFFFFFFFF",  Dest = "$t4", MoveType = ASMOps.Mov.MoveTypes.ImmediateToReg });
                conversionState.Append(new ASMOps.Xor() { Src1 = "$t0", Src2 = "$t4", Dest = "$t0" });
                conversionState.Append(new ASMOps.Xor() { Src1 = "$t3", Src2 = "$t4", Dest = "$t3" });

                // Then add 1
                conversionState.Append(new ASMOps.Mov() { Src = "1", Dest = "$t1", MoveType = ASMOps.Mov.MoveTypes.ImmediateToReg });
                conversionState.Append(new ASMOps.Mov() { Src = "0", Dest = "$t2", MoveType = ASMOps.Mov.MoveTypes.ImmediateToReg });
                //Add $t2:$t1 to $t3:$t0
                //Add low bits
                conversionState.Append(new ASMOps.Add() { Src1 = "$t1", Src2 = "$t0", Dest = "$t4", Unsigned = true });
                //Add carry bit to $t5
                conversionState.Append(new ASMOps.Sltu() { Src1 = "$t4", Src2 = "$t1", Dest = "$t5" });
                //Add high bits including any carry from 
                //when low bits were added
                conversionState.Append(new ASMOps.Add() { Src1 = "$t5", Src2 = "$t3", Dest = "$t5", Unsigned = true });
                conversionState.Append(new ASMOps.Add() { Src1 = "$t5", Src2 = "$t2", Dest = "$t3", Unsigned = true });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "$t4", Dest = "$t0", MoveType = ASMOps.Mov.MoveTypes.RegToReg });
                //Push the result
                //Push high bits
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t3" });
                //Push low bits
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t0" });
            }
            else
            {
                throw new NotSupportedException("Stack item size not supported by neg op!");
            }

            conversionState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                isFloat = itemA.isFloat,
                sizeOnStackInBytes = itemA.sizeOnStackInBytes,
                isGCManaged = itemA.isGCManaged
            });
        }
    }
}
