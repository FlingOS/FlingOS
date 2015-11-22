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
using System.Reflection;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.x86
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Ldfld : IL.ILOps.Ldfld
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
            FieldInfo theField = conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveField(metadataToken);
            
            bool valueisFloat = Utilities.IsFloat(theField.FieldType);
            Types.TypeInfo fieldTypeInfo = conversionState.TheILLibrary.GetTypeInfo(theField.FieldType);
            int stackSize = fieldTypeInfo.SizeOnStackInBytes;
            
            StackItem objPointer = conversionState.CurrentStackFrame.Stack.Pop();

            if ((OpCodes)theOp.opCode.Value == OpCodes.Ldflda)
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
                conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = valueisFloat,
                    sizeOnStackInBytes = stackSize,
                    isGCManaged = fieldTypeInfo.IsGCManaged
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
        /// Thrown if field to load is a floating value or the field to load
        /// is not of size 4 or 8 bytes.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Get the field's token that is used to get FieldInfo from the assembly
            int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
            //Get the field info from the referencing assembly
            FieldInfo theField = conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveField(metadataToken);
            //Get the database type information about the object that contains the field
            Types.TypeInfo objTypeInfo = conversionState.TheILLibrary.GetTypeInfo(theField.DeclaringType);
            int offset = conversionState.TheILLibrary.GetFieldInfo(objTypeInfo, theField.Name).OffsetInBytes;

            //Is the value to load a floating pointer number?
            bool valueisFloat = Utilities.IsFloat(theField.FieldType);
            //Get the size of the value to load (in bytes, as it will appear on the stack)
            Types.TypeInfo fieldTypeInfo = conversionState.TheILLibrary.GetTypeInfo(theField.FieldType);
            int stackSize = fieldTypeInfo.SizeOnStackInBytes;
            int memSize = theField.FieldType.IsValueType ? fieldTypeInfo.SizeOnHeapInBytes : stackSize;

            //Pop the object pointer from our stack
            StackItem objPointer = conversionState.CurrentStackFrame.Stack.Pop();
            
            //If the value to load is a float, erm...abort...
            if (valueisFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Loading fields of type float not supported yet!");
            }

            //Pop object pointer
            conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Dword, Dest = "ECX" });
            if ((OpCodes)theOp.opCode.Value == OpCodes.Ldflda)
            {
                conversionState.Append(new ASMOps.Add() { Src = offset.ToString(), Dest = "ECX" });
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Dword, Src = "ECX" });

                conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = false,
                    sizeOnStackInBytes = 4,
                    isGCManaged = false
                });
            }
            else
            {
                //Push value at pointer+offset
                int sizeNotInMem = stackSize - memSize;
                int sizeToSub = (sizeNotInMem / 2) * 2; //Rounds down
                for (int i = 0; i < sizeToSub; i += 2)
                {
                    conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "0" });
                }
                for (int i = memSize + (memSize % 2); i > 0; i -= 2)
                {
                    if (sizeToSub != sizeNotInMem)
                    {
                        conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "0", Dest = "AX" });
                        conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "[ECX+" + (offset + i - 2).ToString() + "]", Dest = "AL" });
                        conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "AX" });
                    }
                    else
                    {
                        conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "[ECX+" + (offset + i - 2).ToString() + "]", Dest = "AX" });
                        conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "AX" });
                    }
                }

                conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = valueisFloat,
                    sizeOnStackInBytes = stackSize,
                    isGCManaged = fieldTypeInfo.IsGCManaged
                });
            }
        }
    }
}
