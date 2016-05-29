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
            int stackSize = fieldTypeInfo.SizeOnStackInBytes;
            int memSize = theField.FieldType.IsValueType ? fieldTypeInfo.SizeOnHeapInBytes : stackSize;

            //Pop the object pointer from our stack
            StackItem objPointer = conversionState.CurrentStackFrame.GetStack(theOp).Pop();

            //If the value to load is a float, erm...abort...
            if (valueisFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Loading fields of type float not supported yet!");
            }

            //Pop object pointer
            conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t2"});
            if ((OpCodes)theOp.opCode.Value == OpCodes.Ldflda)
            {
                conversionState.Append(new ASMOps.Add {Src1 = "$t2", Src2 = offset.ToString(), Dest = "$t2"});
                conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t2"});

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
                int sizeNotInMem = stackSize - memSize;
                int sizeToSub = sizeNotInMem/2*2; //Rounds down
                for (int i = 0; i < sizeToSub; i += 2)
                {
                    conversionState.Append(new Push {Size = OperandSize.Halfword, Src = "$zero"});
                }
                for (int i = memSize + memSize%2; i > 0; i -= 2)
                {
                    if (sizeToSub != sizeNotInMem)
                    {
                        conversionState.Append(new Mov
                        {
                            Size = OperandSize.Halfword,
                            Src = "0",
                            Dest = "$t0",
                            MoveType = Mov.MoveTypes.ImmediateToReg
                        });
                        //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = (offset + i - 2).ToString() + "($t2)", Dest = "$t0", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
                        GlobalMethods.LoadData(conversionState, theOp, "$t2", "$t0", offset + i - 2, 1);
                        conversionState.Append(new Push {Size = OperandSize.Halfword, Src = "$t0"});
                    }
                    else
                    {
                        //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Halfword, Src = (offset + i - 2).ToString() + "($t2)", Dest = "$t0", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
                        GlobalMethods.LoadData(conversionState, theOp, "$t2", "$t0", offset + i - 2, 2);
                        conversionState.Append(new Push {Size = OperandSize.Halfword, Src = "$t0"});
                    }
                }

                conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
                {
                    isFloat = valueisFloat,
                    sizeOnStackInBytes = stackSize,
                    isGCManaged = fieldTypeInfo.IsGCManaged,
                    isValue = fieldTypeInfo.IsValueType
                });
            }
        }
    }
}