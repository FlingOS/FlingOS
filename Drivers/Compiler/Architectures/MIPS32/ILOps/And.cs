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
using Drivers.Compiler.Architectures.MIPS32.ASMOps;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.MIPS32
{
    /// <summary>
    ///     See base class documentation.
    /// </summary>
    public class And : IL.ILOps.And
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            StackItem itemB = conversionState.CurrentStackFrame.GetStack(theOp).Pop();
            StackItem itemA = conversionState.CurrentStackFrame.GetStack(theOp).Pop();

            if (itemA.sizeOnStackInBytes == 4 &&
                itemB.sizeOnStackInBytes == 4)
            {
                conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                {
                    isFloat = false,
                    sizeOnStackInBytes = 4,
                    isGCManaged = false
                });
            }
            else if (itemA.sizeOnStackInBytes == 8 &&
                     itemB.sizeOnStackInBytes == 8)
            {
                conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                {
                    isFloat = false,
                    sizeOnStackInBytes = 8,
                    isGCManaged = false
                });
            }
        }

        /// <summary>
        ///     See base class documentation.
        /// </summary>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        ///     Thrown if either or both values to 'or' are floating point values.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        ///     Thrown if either or both values to multiply are not 4 or 8 bytes
        ///     in size or if the values are of different size.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Pop in reverse order to push
            StackItem itemB = conversionState.CurrentStackFrame.GetStack(theOp).Pop();
            StackItem itemA = conversionState.CurrentStackFrame.GetStack(theOp).Pop();


            if (itemB.sizeOnStackInBytes < 4 ||
                itemA.sizeOnStackInBytes < 4)
            {
                throw new InvalidOperationException("Invalid stack operand sizes!");
            }
            if (itemB.isFloat || itemA.isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Add floats is unsupported!");
            }
            if (itemA.sizeOnStackInBytes == 4 &&
                itemB.sizeOnStackInBytes == 4)
            {
                //Pop item B
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t1"});
                //Pop item A
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t0"});
                //And the two
                conversionState.Append(new ASMOps.And {Src1 = "$t1", Src2 = "$t0", Dest = "$t0"});
                //Push the result onto the stack
                conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t0"});

                conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                {
                    isFloat = false,
                    sizeOnStackInBytes = 4,
                    isGCManaged = false
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
                //Pop low bits
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t1"});
                //Pop high bits
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t2"});
                //Pop item A to edx:eax
                //Pop low bits
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t0"});
                //Pop high bits
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t3"});
                //And ecx:ebx with edx:eax
                conversionState.Append(new ASMOps.And {Src1 = "$t1", Src2 = "$t0", Dest = "$t0"});
                conversionState.Append(new ASMOps.And {Src1 = "$t2", Src2 = "$t3", Dest = "$t3"});
                //Push the result onto the stack
                conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t3"});
                conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t0"});

                conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                {
                    isFloat = false,
                    sizeOnStackInBytes = 8,
                    isGCManaged = false
                });
            }
        }
    }
}