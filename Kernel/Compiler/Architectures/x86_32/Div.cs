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
    public class Div : ILOps.Div
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if divide operands are floating point numbers or if attempting to divide 64-bit numbers.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if either operand is &lt; 4 bytes long.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            //Pop in reverse order to push
            StackItem itemB = aScannerState.CurrentStackFrame.Stack.Pop();
            StackItem itemA = aScannerState.CurrentStackFrame.Stack.Pop();


            if (itemB.sizeOnStackInBytes < 4 ||
                itemA.sizeOnStackInBytes < 4)
            {
                throw new InvalidOperationException("Invalid stack operand sizes!");
            }
            else if (itemB.isFloat || itemA.isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Divide floats is unsupported!");
            }
            else
            {
                if (itemA.sizeOnStackInBytes == 4 &&
                    itemB.sizeOnStackInBytes == 4)
                {
                    //Pop item B
                    result.AppendLine("pop dword ebx");
                    //Pop item A
                    result.AppendLine("pop dword eax");
                    //Sign extend A to EAX:EDX
                    result.AppendLine("cdq");
                    if ((OpCodes)anILOpInfo.opCode.Value == OpCodes.Div_Un)
                    {
                        //Do the division
                        result.AppendLine("div ebx");
                    }
                    else
                    {
                        //Do the division
                        result.AppendLine("idiv ebx");
                    }
                    //Result stored in eax
                    result.AppendLine("push dword eax");

                    aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
                    {
                        isFloat = false,
                        sizeOnStackInBytes = 4
                    });
                }
                else if ((itemA.sizeOnStackInBytes == 8 &&
                          itemB.sizeOnStackInBytes == 4) || 
                         (itemA.sizeOnStackInBytes == 4 &&
                          itemB.sizeOnStackInBytes == 8))
                {
                    throw new InvalidOperationException("Invalid stack operand sizes! They should be the 32-32 or 64-64.");
                }
                else if (itemA.sizeOnStackInBytes == 8 &&
                    itemB.sizeOnStackInBytes == 8)
                {
                    //SUPPORT - 64-bit division
                    throw new NotSupportedException("64-bit by 64-bit division not supported yet!");
                }
            }

            return result.ToString().Trim();
        }
    }
}
