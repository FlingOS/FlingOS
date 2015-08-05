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
    public class Ldloc : IL.ILOps.Ldloc
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            bool loadAddr = (ILOp.OpCodes)theOp.opCode.Value == OpCodes.Ldloca ||
                            (ILOp.OpCodes)theOp.opCode.Value == OpCodes.Ldloca_S;
            UInt16 localIndex = 0;
            switch ((ILOp.OpCodes)theOp.opCode.Value)
            {
                case OpCodes.Ldloc:
                case OpCodes.Ldloca:
                    localIndex = (UInt16)Utilities.ReadInt16(theOp.ValueBytes, 0);
                    break;
                case OpCodes.Ldloc_0:
                    localIndex = 0;
                    break;
                case OpCodes.Ldloc_1:
                    localIndex = 1;
                    break;
                case OpCodes.Ldloc_2:
                    localIndex = 2;
                    break;
                case OpCodes.Ldloc_3:
                    localIndex = 3;
                    break;
                case OpCodes.Ldloc_S:
                case OpCodes.Ldloca_S:
                    localIndex = (UInt16)theOp.ValueBytes[0];
                    break;
            }

            Types.VariableInfo theLoc = conversionState.Input.TheMethodInfo.LocalInfos.ElementAt(localIndex);
            
            if (loadAddr)
            {
                conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = false,
                    sizeOnStackInBytes = 4,
                    isGCManaged = false
                });
            }
            else
            {
                int pushedLocalSizeVal = theLoc.TheTypeInfo.SizeOnStackInBytes;

                conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = Utilities.IsFloat(theLoc.UnderlyingType),
                    sizeOnStackInBytes = pushedLocalSizeVal,
                    isGCManaged = theLoc.TheTypeInfo.IsGCManaged
                });
            }
        }

        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown when loading a float local is required as it currently hasn't been
        /// implemented.
        /// Also thrown if arguments are not of size 4 or 8 bytes.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Load local

            bool loadAddr = (ILOp.OpCodes)theOp.opCode.Value == OpCodes.Ldloca ||
                            (ILOp.OpCodes)theOp.opCode.Value == OpCodes.Ldloca_S;
            UInt16 localIndex = 0;
            switch ((ILOp.OpCodes)theOp.opCode.Value)
            {
                case OpCodes.Ldloc:
                case OpCodes.Ldloca:
                    localIndex = (UInt16)Utilities.ReadInt16(theOp.ValueBytes, 0);
                    break;
                case OpCodes.Ldloc_0:
                    localIndex = 0;
                    break;
                case OpCodes.Ldloc_1:
                    localIndex = 1;
                    break;
                case OpCodes.Ldloc_2:
                    localIndex = 2;
                    break;
                case OpCodes.Ldloc_3:
                    localIndex = 3;
                    break;
                case OpCodes.Ldloc_S:
                case OpCodes.Ldloca_S:
                    localIndex = (UInt16)theOp.ValueBytes[0];
                    break;
            }

            int bytesOffset = 0;
            for (int i = 0; i < conversionState.Input.TheMethodInfo.LocalInfos.Count && i <= localIndex; i++)
            {
                bytesOffset += conversionState.Input.TheMethodInfo.LocalInfos[i].TheTypeInfo.SizeOnStackInBytes;
            }
            if (localIndex >= conversionState.Input.TheMethodInfo.LocalInfos.Count)
            {
            }
            Types.VariableInfo theLoc = conversionState.Input.TheMethodInfo.LocalInfos[localIndex];
            if (Utilities.IsFloat(theLoc.UnderlyingType))
            {
                //SUPPORT - floats
                throw new NotSupportedException("Float locals not supported yet!");
            }

            if (loadAddr)
            {
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "$fp", Dest = "$t0" });
                conversionState.Append(new ASMOps.Add() { Src2 = bytesOffset.ToString(), Src1 = "$t0", Dest = "$t0" });
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t0" });

                conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = false,
                    sizeOnStackInBytes = 4,
                    isGCManaged = false
                });
            }
            else
            {
                int pushedLocalSizeVal = theLoc.TheTypeInfo.SizeOnStackInBytes;

                if ((pushedLocalSizeVal % 4) != 0)
                {
                    throw new NotSupportedException("Invalid local bytes size!");
                }
                else
                {
                    for (int i = bytesOffset - (pushedLocalSizeVal - 4); i <= bytesOffset; i += 4)
                    {
                        conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "-" + i.ToString() + "($fp)", Dest = "$t0", SrcIsMemory = true });
                        conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t0" });
                    }
                }

                conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat =  Utilities.IsFloat(theLoc.UnderlyingType),
                    sizeOnStackInBytes = pushedLocalSizeVal,
                    isGCManaged = theLoc.TheTypeInfo.IsGCManaged
                });
            }
        }
    }
}
