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
    public class Stfld : IL.ILOps.Stfld
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            conversionState.CurrentStackFrame.GetStack(theOp).Pop();
            conversionState.CurrentStackFrame.GetStack(theOp).Pop();
        }

        /// <summary>
        ///     See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        ///     Thrown if the value to store is floating point or
        ///     if the value is not 4 or 8 bytes in size.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
            FieldInfo theField = conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveField(metadataToken);
            TypeInfo objTypeInfo = conversionState.TheILLibrary.GetTypeInfo(theField.DeclaringType);
            TypeInfo fieldTypeInfo = conversionState.TheILLibrary.GetTypeInfo(theField.FieldType);
            Types.FieldInfo theFieldInfo = conversionState.TheILLibrary.GetFieldInfo(objTypeInfo, theField.Name);


            int offset = theFieldInfo.OffsetInBytes;

            int stackSize = fieldTypeInfo.SizeOnStackInBytes;
            int memSize = fieldTypeInfo.IsValueType ? fieldTypeInfo.SizeOnHeapInBytes : stackSize;

            StackItem value = conversionState.CurrentStackFrame.GetStack(theOp).Pop();
            StackItem objPointer = conversionState.CurrentStackFrame.GetStack(theOp).Pop();

            if (value.isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Storing fields of type float is not supported yet!");
            }

            //Get object pointer
            conversionState.Append(new Mov
            {
                Size = OperandSize.Dword,
                Src = "[ESP+" + stackSize + "]",
                Dest = "ECX"
            });
            //Pop and mov value
            int i = 0;
            for (; i < memSize; i += 2)
            {
                if (memSize - i == 1)
                {
                    conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "AX"});
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Byte,
                        Src = "AL",
                        Dest = "[ECX+" + (offset + i) + "]"
                    });
                }
                else
                {
                    conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "AX"});
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Word,
                        Src = "AX",
                        Dest = "[ECX+" + (offset + i) + "]"
                    });
                }
            }
            //                                                       + 4 to pop object pointer
            conversionState.Append(new ASMOps.Add {Src = (i%4 + 4).ToString(), Dest = "ESP"});
        }
    }
}