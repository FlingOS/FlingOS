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
    public class Not : ILOps.Not
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if either or both values to shift left are floating point values or
        /// if the values are 8 bytes in size.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if either or both values to multiply are not 4 or 8 bytes
        /// in size or if the values are of different size.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            StackItem theItem = aScannerState.CurrentStackFrame.Stack.Peek();
            if (theItem.isFloat)
            {
                //SUPPORT - Not op for floats
                throw new NotSupportedException("Not op not supported for float operands!");
            }

            if (theItem.sizeOnStackInBytes == 4)
            {
                result.AppendLine("pop dword eax");
                result.AppendLine("not eax");
                result.AppendLine("push dword eax");
            }
            else if (theItem.sizeOnStackInBytes == 8)
            {
                result.AppendLine("pop dword eax");
                result.AppendLine("pop dword ebx");
                result.AppendLine("not eax");
                result.AppendLine("not ebx");
                result.AppendLine("push dword ebx");
                result.AppendLine("push dword eax");
            }
            else
            {
                throw new NotSupportedException("Not op not supported for operand size!");
            }

            return result.ToString().Trim();
        }
    }
}
