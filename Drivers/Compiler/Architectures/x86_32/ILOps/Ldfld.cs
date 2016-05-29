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
using System.Reflection;
using Drivers.Compiler.Architectures.x86.ASMOps;
using Drivers.Compiler.IL;
using TypeInfo = Drivers.Compiler.Types.TypeInfo;

namespace Drivers.Compiler.Architectures.x86
{
    /// <summary>
    ///     See base class documentation.
    /// </summary>
    public class Ldfld : IL.ILOps.Ldfld
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
            FieldInfo theField = conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveField(metadataToken);

            bool valueisFloat = Utilities.IsFloat(theField.FieldType);
            TypeInfo fieldTypeInfo = conversionState.TheILLibrary.GetTypeInfo(theField.FieldType);
            int stackSize = fieldTypeInfo.SizeOnStackInBytes;

            StackItem objPointer = conversionState.CurrentStackFrame.GetStack(theOp).Pop();

            if ((OpCodes)theOp.opCode.Value == OpCodes.Ldflda)
            {
                conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                {
                    isFloat = false,
                    sizeOnStackInBytes = 4,
                    isGCManaged = false,
                    isValue = false
                });
            }
            else
            {
                conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                {
                    isFloat = valueisFloat,
                    sizeOnStackInBytes = stackSize,
                    isGCManaged = fieldTypeInfo.IsGCManaged,
                    isValue = fieldTypeInfo.IsValueType
                });
            }
        }

        /// <summary>
        ///     See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        ///     Thrown if field to load is a floating value or the field to load
        ///     is not of size 4 or 8 bytes.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Get the field's token that is used to get FieldInfo from the assembly
            int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
            //Get the field info from the referencing assembly
            FieldInfo theField = conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveField(metadataToken);
            //Get the database type information about the object that contains the field
            TypeInfo objTypeInfo = conversionState.TheILLibrary.GetTypeInfo(theField.DeclaringType);
            int offset = conversionState.TheILLibrary.GetFieldInfo(objTypeInfo, theField.Name).OffsetInBytes;

            //Is the value to load a floating pointer number?
            bool valueisFloat = Utilities.IsFloat(theField.FieldType);
            //Get the size of the value to load (in bytes, as it will appear on the stack)
            TypeInfo fieldTypeInfo = conversionState.TheILLibrary.GetTypeInfo(theField.FieldType);
            int fieldStackSize = fieldTypeInfo.SizeOnStackInBytes;
            int fieldMemSize = theField.FieldType.IsValueType ? fieldTypeInfo.SizeOnHeapInBytes : fieldStackSize;

            //Pop the object pointer from our stack
            StackItem objStackItem = conversionState.CurrentStackFrame.GetStack(theOp).Pop();

            //If the value to load is a float, erm...abort...
            if (valueisFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Loading fields of type float not supported yet!");
            }

            if (objStackItem.isValue)
            {
                // Address = ESP + Offset to field

                if ((OpCodes)theOp.opCode.Value == OpCodes.Ldflda)
                {
                    //Error - How can we load the address of a field which is no longer on the stack??
                    throw new NotSupportedException(
                        "Can't load address of field of value type instance that's no longer on the stack!");
                }
                // Move field to top of stack
                // Remove everything else

                // Stack before:
                //  TOP - Value bytes...
                //      - Wanted value bytes
                //      - ...value bytes
                //      - ...rest of stack...

                // Stack afterwards:
                //  TOP - Wanted value bytes    = ESP - Size of object
                //      - Padding bytes         = ESP - Size of object + size of field bytes
                //      - ...rest of stack...

                // Copy value as efficiently as possible (to ESP-Size of object)

                int i = fieldMemSize;
                for (; i >= 4;)
                {
                    i -= 4;
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Dword,
                        Dest = "EBX",
                        Src = "[ESP+" + (offset + i) + "]"
                    });
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Dword,
                        Dest = "[ESP+" + (objStackItem.sizeOnStackInBytes - fieldStackSize + i) + "]",
                        Src = "EBX"
                    });
                }
                for (; i >= 2;)
                {
                    i -= 2;
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Word,
                        Dest = "BX",
                        Src = "[ESP+" + (offset + i) + "]"
                    });
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Word,
                        Dest = "[ESP+" + (objStackItem.sizeOnStackInBytes - fieldStackSize + i) + "]",
                        Src = "BX"
                    });
                }
                for (; i >= 1;)
                {
                    i -= 1;
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Byte,
                        Dest = "BL",
                        Src = "[ESP+" + (offset + i) + "]"
                    });
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Byte,
                        Dest = "[ESP+" + (objStackItem.sizeOnStackInBytes - fieldStackSize + i) + "]",
                        Src = "BL"
                    });
                }

                // Move in padding 0s
                int paddingSize = fieldStackSize - fieldMemSize;
                switch (paddingSize)
                {
                    case 1:
                        conversionState.Append(new Mov
                        {
                            Size = OperandSize.Byte,
                            Dest = "[ESP+" + (objStackItem.sizeOnStackInBytes - paddingSize) + "]",
                            Src = "0"
                        });
                        break;
                    case 2:
                        conversionState.Append(new Mov
                        {
                            Size = OperandSize.Word,
                            Dest = "[ESP+" + (objStackItem.sizeOnStackInBytes - paddingSize) + "]",
                            Src = "0"
                        });
                        break;
                    case 3:
                        conversionState.Append(new Mov
                        {
                            Size = OperandSize.Byte,
                            Dest = "[ESP+" + (objStackItem.sizeOnStackInBytes - paddingSize) + "]",
                            Src = "0"
                        });
                        conversionState.Append(new Mov
                        {
                            Size = OperandSize.Word,
                            Dest = "[ESP+" + (objStackItem.sizeOnStackInBytes - paddingSize + 1) + "]",
                            Src = "0"
                        });
                        break;
                }

                // Remove everything else
                int sizeToRemove = objStackItem.sizeOnStackInBytes - fieldStackSize;
                conversionState.Append(new ASMOps.Add {Dest = "ESP", Src = sizeToRemove.ToString()});

                conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                {
                    isFloat = valueisFloat,
                    sizeOnStackInBytes = fieldStackSize,
                    isGCManaged = fieldTypeInfo.IsGCManaged,
                    isValue = fieldTypeInfo.IsValueType
                });
            }
            else
            {
                //Pop object pointer
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "ECX"});
                if ((OpCodes)theOp.opCode.Value == OpCodes.Ldflda)
                {
                    conversionState.Append(new ASMOps.Add {Src = offset.ToString(), Dest = "ECX"});
                    conversionState.Append(new Push {Size = OperandSize.Dword, Src = "ECX"});

                    conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                    {
                        isFloat = false,
                        sizeOnStackInBytes = 4,
                        isGCManaged = false,
                        isValue = false
                    });
                }
                else
                {
                    //Push value at pointer+offset
                    int sizeNotInMem = fieldStackSize - fieldMemSize;
                    int sizeToSub = sizeNotInMem/2*2; //Rounds down
                    for (int i = 0; i < sizeToSub; i += 2)
                    {
                        conversionState.Append(new Push {Size = OperandSize.Word, Src = "0"});
                    }
                    for (int i = fieldMemSize + fieldMemSize%2; i > 0; i -= 2)
                    {
                        if (sizeToSub != sizeNotInMem)
                        {
                            conversionState.Append(new Mov {Size = OperandSize.Word, Src = "0", Dest = "AX"});
                            conversionState.Append(new Mov
                            {
                                Size = OperandSize.Byte,
                                Src = "[ECX+" + (offset + i - 2) + "]",
                                Dest = "AL"
                            });
                            conversionState.Append(new Push {Size = OperandSize.Word, Src = "AX"});
                        }
                        else
                        {
                            conversionState.Append(new Mov
                            {
                                Size = OperandSize.Word,
                                Src = "[ECX+" + (offset + i - 2) + "]",
                                Dest = "AX"
                            });
                            conversionState.Append(new Push {Size = OperandSize.Word, Src = "AX"});
                        }
                    }

                    conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                    {
                        isFloat = valueisFloat,
                        sizeOnStackInBytes = fieldStackSize,
                        isGCManaged = fieldTypeInfo.IsGCManaged,
                        isValue = fieldTypeInfo.IsValueType
                    });
                }
            }
        }
    }
}