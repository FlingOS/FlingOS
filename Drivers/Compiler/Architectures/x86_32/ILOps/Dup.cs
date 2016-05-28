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
using Drivers.Compiler.Architectures.x86.ASMOps;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.x86
{
    /// <summary>
    ///     See base class documentation.
    /// </summary>
    public class Dup : IL.ILOps.Dup
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            StackItem itemA = conversionState.CurrentStackFrame.GetStack(theOp).Pop();

            conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
            {
                isFloat = itemA.isFloat,
                sizeOnStackInBytes = itemA.sizeOnStackInBytes,
                isGCManaged = itemA.isGCManaged,
                isValue = itemA.isValue
            });
            conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
            {
                isFloat = itemA.isFloat,
                sizeOnStackInBytes = itemA.sizeOnStackInBytes,
                isGCManaged = itemA.isGCManaged,
                isValue = itemA.isValue
            });
        }

        /// <summary>
        ///     See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        ///     If either value is &lt; 4 bytes in length or
        ///     operands are not of the same size.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Pop item to duplicate
            StackItem itemA = conversionState.CurrentStackFrame.GetStack(theOp).Pop();

            if (itemA.isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Duplicate float vals not suppported yet!");
            }

            if (itemA.sizeOnStackInBytes == 4)
            {
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EAX"});
                conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EAX"});
                conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EAX"});
            }
            else if (itemA.sizeOnStackInBytes == 8)
            {
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EAX"});
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EDX"});
                conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EDX"});
                conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EAX"});
                conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EDX"});
                conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EAX"});
            }
            else
            {
                throw new NotSupportedException("Stack item size not supported by duplicate op!");
            }

            conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
            {
                isFloat = itemA.isFloat,
                sizeOnStackInBytes = itemA.sizeOnStackInBytes,
                isGCManaged = itemA.isGCManaged,
                isValue = itemA.isValue
            });
            conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
            {
                isFloat = itemA.isFloat,
                sizeOnStackInBytes = itemA.sizeOnStackInBytes,
                isGCManaged = itemA.isGCManaged,
                isValue = itemA.isValue
            });
        }
    }
}