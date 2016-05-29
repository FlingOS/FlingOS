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
using Drivers.Compiler.Architectures.MIPS32.ASMOps;
using Drivers.Compiler.IL;
using TypeInfo = Drivers.Compiler.Types.TypeInfo;

namespace Drivers.Compiler.Architectures.MIPS32
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
                    int size = /*theTypeInfo.IsValueType ? theTypeInfo.SizeOnHeapInBytes : */
                        theTypeInfo.SizeOnStackInBytes;
                    bool isFloat = Utilities.IsFloat(theField.FieldType);

                    if (isFloat)
                    {
                        //SUPPORT - floats
                        throw new NotSupportedException("Loading static fields of type float not supported yet!");
                    }

                    conversionState.Append(new La {Label = fieldID, Dest = "$t1"});

                    if (size == 1)
                    {
                        conversionState.Append(new ASMOps.Xor {Src1 = "$t0", Src2 = "$t0", Dest = "$t0"});
                        conversionState.Append(new Mov
                        {
                            Size = OperandSize.Byte,
                            Src = "0($t1)",
                            Dest = "$t0",
                            MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                        });
                        conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t0"});
                    }
                    else if (size == 2)
                    {
                        conversionState.Append(new ASMOps.Xor {Src1 = "$t0", Src2 = "$t0", Dest = "$t0"});
                        conversionState.Append(new Mov
                        {
                            Size = OperandSize.Halfword,
                            Src = "0($t1)",
                            Dest = "$t0",
                            MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                        });
                        conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t0"});
                    }
                    else if (size == 4)
                    {
                        conversionState.Append(new Mov
                        {
                            Size = OperandSize.Word,
                            Src = "0($t1)",
                            Dest = "$t0",
                            MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                        });
                        conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t0"});
                    }
                    else if (size == 8)
                    {
                        conversionState.Append(new Mov
                        {
                            Size = OperandSize.Word,
                            Src = "4($t1)",
                            Dest = "$t0",
                            MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                        });
                        conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t0"});
                        conversionState.Append(new Mov
                        {
                            Size = OperandSize.Word,
                            Src = "0($t1)",
                            Dest = "$t0",
                            MoveType = Mov.MoveTypes.SrcMemoryToDestReg
                        });
                        conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t0"});
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(
                            "Loading static field that has stack size greater than 8 not supported!");
                    }

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
                    //Load the address of the field i.e. address of the ASM label
                    conversionState.Append(new La {Label = fieldID, Dest = "$t4"});
                    conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t4"});

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