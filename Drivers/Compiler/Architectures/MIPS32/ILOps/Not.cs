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
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.MIPS32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Not : IL.ILOps.Not
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            //No effect
        }

        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// If either value is &lt; 4 bytes in length or
        /// operands are not of the same size.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Pop item to negate
            StackItem itemA = conversionState.CurrentStackFrame.Stack.Pop();

            if (itemA.isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Negate float vals not suppported yet!");
            }

            if (itemA.sizeOnStackInBytes == 4)
            {
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });
                //To not, arg Xor -1
                conversionState.Append(new ASMOps.Mov() { Dest = "$t4", Src = "0xFFFFFFFF", MoveType = ASMOps.Mov.MoveTypes.ImmediateToReg });
                conversionState.Append(new ASMOps.Xor() { Dest = "$t0", Src1 = "$t0", Src2 = "$t4" });
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t0" });
            }
            else if (itemA.sizeOnStackInBytes == 8)
            {
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t3" });
                conversionState.Append(new ASMOps.Mov() { Dest = "$t4", Src = "0xFFFFFFFF", MoveType = ASMOps.Mov.MoveTypes.ImmediateToReg });
                conversionState.Append(new ASMOps.Xor() { Dest = "$t0", Src1 = "$t0", Src2 = "$t4" });
                conversionState.Append(new ASMOps.Xor() { Dest = "$t3", Src1 = "$t3", Src2 = "$t4" });
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t3" });
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t0" });
            }
            else
            {
                throw new NotSupportedException("Stack item size not supported by neg op!");
            }

            conversionState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                isFloat = itemA.isFloat,
                sizeOnStackInBytes = itemA.sizeOnStackInBytes,
                isGCManaged = itemA.isGCManaged
            });
        }
    }
}
