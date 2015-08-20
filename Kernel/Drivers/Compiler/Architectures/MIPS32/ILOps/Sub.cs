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
    public class Sub : IL.ILOps.Sub
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            StackItem itemB = conversionState.CurrentStackFrame.Stack.Pop();
            StackItem itemA = conversionState.CurrentStackFrame.Stack.Pop();

            if (itemA.sizeOnStackInBytes == 4 &&
                itemB.sizeOnStackInBytes == 4)
            {
                conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = false,
                    sizeOnStackInBytes = 4,
                    isGCManaged = false
                });
            }
            else if (itemA.sizeOnStackInBytes == 8 &&
                itemB.sizeOnStackInBytes == 8)
            {
                conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = false,
                    sizeOnStackInBytes = 8,
                    isGCManaged = false
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
        /// Thrown if the either of the values to add are floating point.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the either of the values to add are less than 4 bytes in size
        /// or if they are of different sizes.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Pop the operands from our stack in reverse order
            //i.e. second operand was pushed last so comes off the 
            //top of the stack first

            //Pop item B - one of the items to subtract
            StackItem itemB = conversionState.CurrentStackFrame.Stack.Pop();
            //Pop item A - the other item to subtract
            StackItem itemA = conversionState.CurrentStackFrame.Stack.Pop();

            //If either item item is < 4 bytes then we have a stack error.
            if (itemB.sizeOnStackInBytes < 4 ||
                itemA.sizeOnStackInBytes < 4)
            {
                throw new InvalidOperationException("Invalid stack operand sizes!");
            }
            //If either item is floating point, we must use floating point conversions
            //and floating point arithmetic
            else if (itemB.isFloat || itemA.isFloat)
            {
                //SUPPORT - floats
                //  - We need to convert items to float if necessary
                //  - Then use floating point arithmetic
                //  - Then push the result onto the stack and mark it as float
                throw new NotSupportedException("Sub floats is unsupported!");
            }
            else
            {
                //If both items are Int32s (or UInt32s - it is irrelevant)
                //Note: IL handles type conversions using other ops
                if (itemA.sizeOnStackInBytes == 4 &&
                    itemB.sizeOnStackInBytes == 4)
                {
                    //Pop item B
                    conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t1" });
                    //Pop item A
                    conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });
                    //Subtract the two
                    conversionState.Append(new ASMOps.Sub() { Src1 = "$t0", Src2 = "$t1", Dest = "$t0"});
                    //Push the result onto the stack
                    conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t0" });

                    //Push the result onto our stack
                    conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                    {
                        isFloat = false,
                        sizeOnStackInBytes = 4,
                        isGCManaged = false
                    });
                }
                //Invalid if the operands are of different sizes.
                //Note: This usually occurs when a previous IL op failed to process properly.
                else if ((itemA.sizeOnStackInBytes == 8 &&
                    itemB.sizeOnStackInBytes == 4) ||
                    (itemA.sizeOnStackInBytes == 4 &&
                    itemB.sizeOnStackInBytes == 8))
                {
                    throw new InvalidOperationException("Invalid stack operand sizes! They should be the same size.");
                }
                else if (itemA.sizeOnStackInBytes == 8 &&
                    itemB.sizeOnStackInBytes == 8)
                {
                    //Pop item B to $t2:$t1
                    //Pop low bits
                    conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t1" });
                    //Pop high bits
                    conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t2" });
                    //Pop item A to $t3:$t0
                    //Pop low bits
                    conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });
                    //Pop high bits
                    conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t3" });
                    //Sub $t2:$t1 from $t3:$t0
                    //Check for borrow
                    conversionState.Append(new ASMOps.Sltu() { Dest = "$t5", Src1 = "$t0", Src2 = "$t1" });
                    //Sub low bits
                    conversionState.Append(new ASMOps.Sub() {  Dest = "$t0", Src1 = "$t0", Src2 = "$t1", Unsigned = true });
                    //Sub high bits including any borrow from
                    //when low bits were subtracted
                    conversionState.Append(new ASMOps.Add() { Dest = "$t3", Src1 = "$t5", Src2 = "$t3", Unsigned = true });
                    conversionState.Append(new ASMOps.Sub() { Dest = "$t3", Src1 = "$t3",  Src2 = "$t2", Unsigned = true });
                    //Push the result
                    //Push high bits
                    conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t3" });
                    //Push low bits
                    conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t0" });

                    //Push the result onto our stack
                    conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                    {
                        isFloat = false,
                        sizeOnStackInBytes = 8,
                        isGCManaged = false
                    });
                }
            }
        }
    }
}
