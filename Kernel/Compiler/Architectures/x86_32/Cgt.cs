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

namespace Kernel.Compiler.Architectures.x86_32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Cgt : ILOps.Cgt
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if compare operands are floating point numbers or if either value is &lt; 4 bytes in length or
        /// operands are not of the same size.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            //Pop in reverse order to push
            StackItem itemB = aScannerState.CurrentStackFrame.Stack.Pop();
            StackItem itemA = aScannerState.CurrentStackFrame.Stack.Pop();

            bool unsignedComparison = (OpCodes)anILOpInfo.opCode.Value == OpCodes.Cgt_Un;

            string trueLabelName = string.Format("{0}.IL_{1}_True", aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method), anILOpInfo.Position);
            string elseLabelName = string.Format("{0}.IL_{1}_Else", aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method), anILOpInfo.Position);
            string endLabelName = string.Format("{0}.IL_{1}_End", aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method), anILOpInfo.Position);

            if (itemB.isFloat || itemA.isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Add floats is unsupported!");
            }
            else if(itemA.sizeOnStackInBytes == 4 && itemB.sizeOnStackInBytes == 4)
            {
                //Pop item B
                result.AppendLine("pop dword ebx");
                //Pop item A
                result.AppendLine("pop dword eax");
                //Compare A to B
                result.AppendLine("cmp eax, ebx");
                //If A <= B, jump to Else (not-true case)
                if (unsignedComparison)
                {
                    result.AppendLine(string.Format("jbe {0}", elseLabelName));
                }
                else
                {
                    result.AppendLine(string.Format("jle {0}", elseLabelName));
                }
                //Otherwise, A > B, so push true (true=1)
                result.AppendLine("push dword 1");
                //And then jump to the end of this IL op.
                result.AppendLine(string.Format("jmp {0}", endLabelName));
                //Insert the Else label.
                result.AppendLine(string.Format("{0}:", elseLabelName));
                //Else case - Push false (false=0)
                result.AppendLine("push dword 0");
                
                //Push the result onto our stack
                aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = false,
                    sizeOnStackInBytes = 4,
                    isGCManaged = false
                });
            }
            else if (itemA.sizeOnStackInBytes == 8 && itemB.sizeOnStackInBytes == 8)
            {
                //Pop item B
                result.AppendLine("pop dword ebx");
                result.AppendLine("pop dword ecx");
                //Pop item A
                result.AppendLine("pop dword eax");
                result.AppendLine("pop dword edx");
                //If A high bytes > B high bytes : True
                //If A high bytes = B high bytes : Check, if A low bytes > B low bytes : True
                //Else : False
                
                //Compare high bytes
                result.AppendLine("cmp edx, ecx");
                //A high bytes > B high bytes? Jump to true case.
                if (unsignedComparison)
                {
                    result.AppendLine(string.Format("ja {0}", trueLabelName));
                }
                else
                {
                    result.AppendLine(string.Format("jg {0}", trueLabelName));
                }
                //A high bytes < B high bytes? Jump to else case.
                if (unsignedComparison)
                {
                    result.AppendLine(string.Format("jb {0}", elseLabelName));
                }
                else
                {
                    result.AppendLine(string.Format("jl {0}", elseLabelName));
                }
                //Otherwise, A high bytes = B high bytes
                //So compare low bytes
                result.AppendLine("cmp eax, ebx");
                //A low bytes <= B low bytes? Jump to else case.
                if (unsignedComparison)
                {
                    result.AppendLine(string.Format("jbe {0}", elseLabelName));
                }
                else
                {
                    result.AppendLine(string.Format("jle {0}", elseLabelName));
                }
                //Otherwise A > B.

                //Insert True case label
                result.AppendLine(string.Format("{0}:", trueLabelName));
                //True case - Push true (true=1)
                result.AppendLine("push dword 1");
                //And then jump to the end of this IL op.
                result.AppendLine(string.Format("jmp {0}", endLabelName));
                //Insert Else case label
                result.AppendLine(string.Format("{0}:", elseLabelName));
                //Else case - Push false (false=0)
                result.AppendLine("push dword 0");

                //Push the result onto our stack
                aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = false,
                    // Yes, this is supposed to be 4 - the value that just got pushed was a 
                    // true / false integer
                    sizeOnStackInBytes = 4,
                    isGCManaged = false
                });
            }
            else
            {
                throw new NotSupportedException("Unsupported number of bytes for compare less than!");
            }

            //Always append the end label
            result.AppendLine(string.Format("{0}:", endLabelName));

            return result.ToString().Trim();
        }
    }
}
