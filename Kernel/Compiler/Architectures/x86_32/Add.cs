using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.Architectures.x86_32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Add : ILOps.Add
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if attempt to add a floating point number since floats are not supported yet.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if either stack argument is &lt; 4 bytes in size.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            //Pop the operands from our stack in reverse order
            //i.e. second operand was pushed last so comes off the 
            //top of the stack first

            //Pop item B - one of the items to add
            StackItem itemB = aScannerState.CurrentStackFrame.Stack.Pop();
            //Pop item A - the other item to add
            StackItem itemA = aScannerState.CurrentStackFrame.Stack.Pop();

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
                // Note: Check but I think floating point arithmetic is done using 
                //       XMM registers and their specific ops.
                throw new NotSupportedException("Add floats is unsupported!");
            }
            else
            {
                //If both items are Int32s (or UInt32s - it is irrelevant)
                //Note: IL handles type conversions using other ops
                if (itemA.sizeOnStackInBytes == 4 &&
                    itemB.sizeOnStackInBytes == 4)
                {
                    //Pop item B
                    result.AppendLine("pop dword ebx");
                    //Pop item A
                    result.AppendLine("pop dword eax");
                    //Add the two
                    result.AppendLine("add eax, ebx");
                    //Push the result onto the stack
                    result.AppendLine("push dword eax");

                    //Push the result onto our stack
                    aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
                    {
                        isFloat = false,
                        sizeOnStackInBytes = 4
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
                    //Pop item B to ecx:ebx
                    //Pop low bits
                    result.AppendLine("pop dword ebx");
                    //Pop high bits
                    result.AppendLine("pop dword ecx");
                    //Pop item A to edx:eax
                    //Pop low bits
                    result.AppendLine("pop dword eax");
                    //Pop high bits
                    result.AppendLine("pop dword edx");
                    //Add ecx:ebx to edx:eax
                    //Add low bits
                    result.AppendLine("add eax, ebx");
                    //Add high bits including any carry from 
                    //when low bits were added
                    result.AppendLine("adc edx, ecx");
                    //Push the result
                    //Push high bits
                    result.AppendLine("push dword edx");
                    //Push low bits
                    result.AppendLine("push dword eax");

                    //Push the result onto our stack
                    aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
                    {
                        isFloat = false,
                        sizeOnStackInBytes = 8
                    });
                }
            }

            return result.ToString().Trim();
        }
    }
}
