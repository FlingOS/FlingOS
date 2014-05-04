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
    public class And : ILOps.And
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if either or both values to 'or' are floating point values.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if either or both values to multiply are not 4 or 8 bytes
        /// in size or if the values are of different size.
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
                throw new NotSupportedException("Add floats is unsupported!");
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
                    result.AppendLine("and eax, ebx");
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
                    throw new InvalidOperationException("Invalid stack operand sizes! They should be the same size.");
                }
                else if (itemA.sizeOnStackInBytes == 8 &&
                    itemB.sizeOnStackInBytes == 8)
                {
                    //Pop item B to ecx:ebx
                    result.AppendLine("pop dword ebx");
                    result.AppendLine("pop dword ecx");
                    //Pop item A to edx:eax
                    result.AppendLine("pop dword eax");
                    result.AppendLine("pop dword edx");
                    //And ecx:ebx with edx:eax
                    result.AppendLine("and eax, ebx");
                    result.AppendLine("and edx, ecx");
                    result.AppendLine("push dword edx");
                    result.AppendLine("push dword eax");

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
