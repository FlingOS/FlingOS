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
    public class Not : IL.ILOps.Not
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            //No effect
        }

        /// <summary>
        ///     See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        ///     Thrown if either or both values to not are floating point values or
        ///     if the values are 8 bytes in size.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            StackItem theItem = conversionState.CurrentStackFrame.GetStack(theOp).Peek();
            if (theItem.isFloat)
            {
                //SUPPORT - Not op for floats
                throw new NotSupportedException("Not op not supported for float operands!");
            }

            if (theItem.sizeOnStackInBytes == 4)
            {
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EAX"});
                conversionState.Append(new ASMOps.Not {Dest = "EAX"});
                conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EAX"});
            }
            else if (theItem.sizeOnStackInBytes == 8)
            {
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EAX"});
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EBX"});
                conversionState.Append(new ASMOps.Not {Dest = "EAX"});
                conversionState.Append(new ASMOps.Not {Dest = "EBX"});
                conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EBX"});
                conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EAX"});
            }
            else
            {
                throw new NotSupportedException("Not op not supported for operand size!");
            }
        }
    }
}