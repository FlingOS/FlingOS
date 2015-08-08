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
    public class Ldind : IL.ILOps.Ldind
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            StackItem addressItem = conversionState.CurrentStackFrame.Stack.Pop();
            int bytesToLoad = 0;

            switch ((OpCodes)theOp.opCode.Value)
            {
                case OpCodes.Ldind_U1:
                case OpCodes.Ldind_I1:
                    bytesToLoad = 1;
                    break;
                case OpCodes.Ldind_U2:
                case OpCodes.Ldind_I2:
                    bytesToLoad = 2;
                    break;
                case OpCodes.Ldind_U4:
                case OpCodes.Ldind_I4:
                case OpCodes.Ldind_I:
                    bytesToLoad = 4;
                    break;
                case OpCodes.Ldind_I8:
                    bytesToLoad = 8;
                    break;
            }

            conversionState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                sizeOnStackInBytes = bytesToLoad == 8 ? 8 : 4,
                isFloat = false,
                isGCManaged = false
            });
        }

        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Load indirect
            //Pop address
            //Push [address]
            
            StackItem addressItem = conversionState.CurrentStackFrame.Stack.Pop();
            int bytesToLoad = 0;
            
            switch ((OpCodes)theOp.opCode.Value)
            {
                case OpCodes.Ldind_U1:
                case OpCodes.Ldind_I1:
                    bytesToLoad = 1;
                    break;
                case OpCodes.Ldind_U2:
                case OpCodes.Ldind_I2:
                    bytesToLoad = 2;
                    break;
                case OpCodes.Ldind_U4:
                case OpCodes.Ldind_I4:
                case OpCodes.Ldind_I:
                    bytesToLoad = 4;
                    break;
                case OpCodes.Ldind_I8:
                    bytesToLoad = 8;
                    break;
            }

            //Pop address
            conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t1" });

            if (bytesToLoad == 1)
            {
                conversionState.Append(new ASMOps.Xor() { Src1 = "$t0", Src2 = "$t0", Dest = "$t0" });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "0($t1)", Dest = "$t0", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t0" });
            }
            else if (bytesToLoad == 2)
            {
                // Assume half word misaligned
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "1($t1)", Dest = "$t0", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
                conversionState.Append(new ASMOps.Sll() { Src = "$t0", Dest = "$t0", Bits = 8 });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "0($t1)", Dest = "$t4", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
                conversionState.Append(new ASMOps.Or() { Src1 = "$t4", Src2 = "$t0", Dest = "$t0" });

                // Push the loaded value
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t0" });
            }
            else if (bytesToLoad == 4)
            {
                //Assume half word misaligned
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "3($t1)", Dest = "$t0", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
                conversionState.Append(new ASMOps.Sll() { Src = "$t0", Dest = "$t0", Bits = 24 });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "2($t1)", Dest = "$t4", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
                conversionState.Append(new ASMOps.Sll() { Src = "$t0", Dest = "$t0", Bits = 16 });
                conversionState.Append(new ASMOps.Or() { Src1 = "$t4", Src2 = "$t0", Dest = "$t0" });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "1($t1)", Dest = "$t4", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
                conversionState.Append(new ASMOps.Sll() { Src = "$t0", Dest = "$t0", Bits = 8 });
                conversionState.Append(new ASMOps.Or() { Src1 = "$t4", Src2 = "$t0", Dest = "$t0" });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "0($t1)", Dest = "$t4", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
                conversionState.Append(new ASMOps.Or() { Src1 = "$t4", Src2 = "$t0", Dest = "$t0" });

                // Push the loaded value
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t0" });
            }
            else if (bytesToLoad == 8)
            {
                //TODO: Runtime check for address word alignment

                //Assume half word misaligned
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "7($t1)", Dest = "$t0", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
                conversionState.Append(new ASMOps.Sll() { Src = "$t0", Dest = "$t0", Bits = 24 });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "6($t1)", Dest = "$t4", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
                conversionState.Append(new ASMOps.Sll() { Src = "$t0", Dest = "$t0", Bits = 16 });
                conversionState.Append(new ASMOps.Or() { Src1 = "$t4", Src2 = "$t0", Dest = "$t0" });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "4($t1)", Dest = "$t4", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
                conversionState.Append(new ASMOps.Sll() { Src = "$t0", Dest = "$t0", Bits = 8 });
                conversionState.Append(new ASMOps.Or() { Src1 = "$t4", Src2 = "$t0", Dest = "$t0" });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "5($t1)", Dest = "$t4", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
                conversionState.Append(new ASMOps.Or() { Src1 = "$t4", Src2 = "$t0", Dest = "$t0" });

                // Push the loaded value
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t0" });

                //Assume half word misaligned
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "3($t1)", Dest = "$t0", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
                conversionState.Append(new ASMOps.Sll() { Src = "$t0", Dest = "$t0", Bits = 24 });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "2($t1)", Dest = "$t4", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
                conversionState.Append(new ASMOps.Sll() { Src = "$t0", Dest = "$t0", Bits = 16 });
                conversionState.Append(new ASMOps.Or() { Src1 = "$t4", Src2 = "$t0", Dest = "$t0" });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "1($t1)", Dest = "$t4", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
                conversionState.Append(new ASMOps.Sll() { Src = "$t0", Dest = "$t0", Bits = 8 });
                conversionState.Append(new ASMOps.Or() { Src1 = "$t4", Src2 = "$t0", Dest = "$t0" });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "0($t1)", Dest = "$t4", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
                conversionState.Append(new ASMOps.Or() { Src1 = "$t4", Src2 = "$t0", Dest = "$t0" });

                // Push the loaded value
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t0" });
            }

            conversionState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                sizeOnStackInBytes = bytesToLoad == 8 ? 8 : 4,
                isFloat = false,
                isGCManaged = false
            });
        }
    }
}
