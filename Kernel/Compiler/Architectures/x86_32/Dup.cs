#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
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
    public class Dup : ILOps.Dup
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// If either value is &lt; 4 bytes in length or
        /// operands are not of the same size.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            //Pop item to duplicate
            StackItem itemA = aScannerState.CurrentStackFrame.Stack.Pop();

            if(itemA.isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Duplicate float vals not suppported yet!");
            }
            
            if(itemA.sizeOnStackInBytes == 4)
            {
                result.AppendLine("pop dword eax");
                result.AppendLine("push dword eax");
                result.AppendLine("push dword eax");
            }
            else if (itemA.sizeOnStackInBytes == 8)
            {
                result.AppendLine("pop dword eax");
                result.AppendLine("pop dword edx");
                result.AppendLine("push dword edx");
                result.AppendLine("push dword eax");
                result.AppendLine("push dword edx");
                result.AppendLine("push dword eax");
            }
            else
            {
                throw new NotSupportedException("Stack item size not supported by duplicate op!");
            }

            aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                isFloat = itemA.isFloat,
                sizeOnStackInBytes = itemA.sizeOnStackInBytes,
                isGCManaged = itemA.isGCManaged
            });
            aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                isFloat = itemA.isFloat,
                sizeOnStackInBytes = itemA.sizeOnStackInBytes,
                isGCManaged = itemA.isGCManaged
            });

            return result.ToString().Trim();
        }
    }
}
