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
using System.Linq;
using Drivers.Compiler.Architectures.MIPS32.ASMOps;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.MIPS32
{
    /// <summary>
    ///     See base class documentation.
    /// </summary>
    public class Stloc : IL.ILOps.Stloc
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            conversionState.CurrentStackFrame.GetStack(theOp).Pop();
        }

        /// <summary>
        ///     See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        ///     Thrown if the value to store is floating point.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            ushort localIndex = 0;
            switch ((OpCodes)theOp.opCode.Value)
            {
                case OpCodes.Stloc:
                    localIndex = (ushort)Utilities.ReadInt16(theOp.ValueBytes, 0);
                    break;
                case OpCodes.Stloc_0:
                    localIndex = 0;
                    break;
                case OpCodes.Stloc_1:
                    localIndex = 1;
                    break;
                case OpCodes.Stloc_2:
                    localIndex = 2;
                    break;
                case OpCodes.Stloc_3:
                    localIndex = 3;
                    break;
                case OpCodes.Stloc_S:
                    localIndex = theOp.ValueBytes[0];
                    break;
            }

            int bytesOffset = 0;
            for (int i = 0; i < conversionState.Input.TheMethodInfo.LocalInfos.Count && i <= localIndex; i++)
            {
                bytesOffset +=
                    conversionState.Input.TheMethodInfo.LocalInfos.ElementAt(i).TheTypeInfo.SizeOnStackInBytes;
            }
            StackItem theItem = conversionState.CurrentStackFrame.GetStack(theOp).Pop();
            if (theItem.isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Float locals not supported yet!");
            }

            int locSize =
                conversionState.Input.TheMethodInfo.LocalInfos.ElementAt(localIndex).TheTypeInfo.SizeOnStackInBytes;
            if (locSize == 0)
            {
                conversionState.Append(new Comment("0 pop size (?!)"));
            }
            else if (locSize == 8)
            {
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t0"});
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "$t0",
                    Dest = "-" + bytesOffset + "($fp)",
                    MoveType = Mov.MoveTypes.SrcRegToDestMemory
                });
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t0"});
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "$t0",
                    Dest = "-" + (bytesOffset - 4) + "($fp)",
                    MoveType = Mov.MoveTypes.SrcRegToDestMemory
                });
            }
            else if (locSize == 4)
            {
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t0"});
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "$t0",
                    Dest = "-" + bytesOffset + "($fp)",
                    MoveType = Mov.MoveTypes.SrcRegToDestMemory
                });
            }
            else if (locSize == 2)
            {
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Halfword, Dest = "$t0"});
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Halfword,
                    Src = "$t0",
                    Dest = "-" + bytesOffset + "($fp)",
                    MoveType = Mov.MoveTypes.SrcRegToDestMemory
                });
            }
            else if (locSize == 1)
            {
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t0"});
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Byte,
                    Src = "$t0",
                    Dest = "-" + bytesOffset + "($fp)",
                    MoveType = Mov.MoveTypes.SrcRegToDestMemory
                });
            }
            else
            {
                for (int i = 0; i < locSize; i += 4)
                {
                    conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t0"});
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Word,
                        Src = "$t0",
                        Dest = "-" + (bytesOffset - i) + "($fp)",
                        MoveType = Mov.MoveTypes.SrcRegToDestMemory
                    });
                }
            }
        }
    }
}