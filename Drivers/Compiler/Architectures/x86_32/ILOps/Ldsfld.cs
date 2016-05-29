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
    public class Ldsfld : IL.ILOps.Ldsfld
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
            FieldInfo theField = conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveField(metadataToken);

            switch ((OpCodes)theOp.opCode.Value)
            {
                case OpCodes.Ldsfld:
                {
                    TypeInfo theTypeInfo = conversionState.TheILLibrary.GetTypeInfo(theField.FieldType);
                    int size = theTypeInfo.SizeOnStackInBytes;
                    bool isFloat = Utilities.IsFloat(theField.FieldType);

                    conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                    {
                        isFloat = isFloat,
                        sizeOnStackInBytes = size == 8 ? 8 : 4,
                        isGCManaged = theTypeInfo.IsGCManaged,
                        isValue = theTypeInfo.IsValueType
                    });
                }
                    break;
                case OpCodes.Ldsflda:
                    conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                    {
                        isFloat = false,
                        sizeOnStackInBytes = 4,
                        isGCManaged = false,
                        isValue = false
                    });
                    break;
            }
        }

        /// <summary>
        ///     See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        ///     Thrown when loading a static float field.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Load static field

            //Load the metadata token used to get the field info
            int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
            //Get the field info for the field to load
            FieldInfo theField = conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveField(metadataToken);
            //Get the ID (i.e. ASM label) of the field to load
            string fieldID = conversionState.GetFieldInfo(theField.DeclaringType, theField.Name).ID;

            conversionState.AddExternalLabel(fieldID);

            //Load the field or field address
            switch ((OpCodes)theOp.opCode.Value)
            {
                case OpCodes.Ldsfld:
                {
                    TypeInfo theTypeInfo = conversionState.TheILLibrary.GetTypeInfo(theField.FieldType);
                    int size = theTypeInfo.IsValueType ? theTypeInfo.SizeOnHeapInBytes : theTypeInfo.SizeOnStackInBytes;
                    bool isFloat = Utilities.IsFloat(theField.FieldType);

                    if (isFloat)
                    {
                        //SUPPORT - floats
                        throw new NotSupportedException("Loading static fields of type float not supported yet!");
                    }

                    if (size == 1)
                    {
                        conversionState.Append(new ASMOps.Xor {Src = "EAX", Dest = "EAX"});
                        conversionState.Append(new Mov
                        {
                            Size = OperandSize.Byte,
                            Src = "[" + fieldID + "]",
                            Dest = "AL"
                        });
                        conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EAX"});
                    }
                    else if (size == 2)
                    {
                        conversionState.Append(new ASMOps.Xor {Src = "EAX", Dest = "EAX"});
                        conversionState.Append(new Mov
                        {
                            Size = OperandSize.Word,
                            Src = "[" + fieldID + "]",
                            Dest = "AX"
                        });
                        conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EAX"});
                    }
                    else
                    {
                        for (int i = size; i > 0;)
                        {
                            int diff = i%4;
                            diff = diff == 0 ? 4 : diff;
                            i -= diff;
                            switch (diff)
                            {
                                case 1:
                                    conversionState.Append(new ASMOps.Xor {Src = "EAX", Dest = "EAX"});
                                    conversionState.Append(new Mov
                                    {
                                        Size = OperandSize.Byte,
                                        Src = "[" + fieldID + " + " + i + "]",
                                        Dest = "AL"
                                    });
                                    conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EAX"});
                                    break;
                                case 2:
                                    conversionState.Append(new ASMOps.Xor {Src = "EAX", Dest = "EAX"});
                                    conversionState.Append(new Mov
                                    {
                                        Size = OperandSize.Word,
                                        Src = "[" + fieldID + " + " + i + "]",
                                        Dest = "AX"
                                    });
                                    conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EAX"});
                                    break;
                                case 3:
                                    conversionState.Append(new ASMOps.Xor {Src = "EAX", Dest = "EAX"});
                                    conversionState.Append(new Mov
                                    {
                                        Size = OperandSize.Byte,
                                        Src = "[" + fieldID + " + " + i + "]",
                                        Dest = "AL"
                                    });
                                    conversionState.Append(new Push {Size = OperandSize.Word, Src = "AX"});
                                    conversionState.Append(new Mov
                                    {
                                        Size = OperandSize.Word,
                                        Src = "[" + fieldID + " + " + (i + 1) + "]",
                                        Dest = "AX"
                                    });
                                    conversionState.Append(new Push {Size = OperandSize.Word, Src = "AX"});
                                    break;
                                default:
                                    conversionState.Append(new Mov
                                    {
                                        Size = OperandSize.Dword,
                                        Src = "[" + fieldID + " + " + i + "]",
                                        Dest = "EAX"
                                    });
                                    conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EAX"});
                                    break;
                            }
                        }
                    }

                    conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                    {
                        isFloat = isFloat,
                        sizeOnStackInBytes = theTypeInfo.SizeOnStackInBytes,
                        isGCManaged = theTypeInfo.IsGCManaged,
                        isValue = theTypeInfo.IsValueType
                    });
                }
                    break;
                case OpCodes.Ldsflda:
                    //Load the address of the field i.e. address of the ASM label
                    conversionState.Append(new Push {Size = OperandSize.Dword, Src = fieldID});

                    conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                    {
                        isFloat = false,
                        sizeOnStackInBytes = 4,
                        isGCManaged = false,
                        isValue = false
                    });
                    break;
            }
        }
    }
}