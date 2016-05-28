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
    public class Switch : IL.ILOps.Switch
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            conversionState.CurrentStackFrame.GetStack(theOp).Pop();
        }

        public override void Preprocess(ILPreprocessState preprocessState, ILOp theOp)
        {
            for (int i = 0; i < theOp.ValueBytes.Length/4; i++)
            {
                int branchOffset = theOp.NextOffset + Utilities.ReadInt32(theOp.ValueBytes, i*4);
                ILOp opToGoTo = preprocessState.Input.At(branchOffset);
                opToGoTo.LabelRequired = true;
            }
        }

        /// <summary>
        ///     See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        ///     Thrown if divide operands are floating point numbers or if attempting to divide 64-bit numbers.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        ///     Thrown if either operand is &lt; 4 bytes long.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            StackItem testItem = conversionState.CurrentStackFrame.GetStack(theOp).Pop();

            if (testItem.isFloat)
            {
                //TODO: Support floats
                throw new NotSupportedException("Switch for floats no supported!");
            }
            if (testItem.sizeOnStackInBytes != 4)
            {
                //TODO: Support other sizes
                throw new NotSupportedException("Switch for non-int32s not supported!");
            }

            conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EAX"});
            for (int i = 0; i < theOp.ValueBytes.Length/4; i++)
            {
                int branchOffset = theOp.NextOffset + Utilities.ReadInt32(theOp.ValueBytes, i*4);
                ILOp opToGoTo = conversionState.Input.At(branchOffset);
                int branchPos = conversionState.PositionOf(opToGoTo);

                conversionState.Append(new Cmp {Arg1 = "EAX", Arg2 = i.ToString()});
                conversionState.Append(new Jmp {JumpType = JmpOp.JumpEqual, DestILPosition = branchPos});
            }
        }
    }
}