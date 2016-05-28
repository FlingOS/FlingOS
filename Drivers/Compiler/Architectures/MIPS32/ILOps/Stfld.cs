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
                throw new NotSupportedException("Storing fields of type float not supported yet!");
            }

            //Get object pointer
            conversionState.Append(new Mov
            {
                Size = OperandSize.Word,
                Src = stackSize + "($sp)",
                Dest = "$t2",
                MoveType = Mov.MoveTypes.SrcMemoryToDestReg
            });
            //Pop and mov value
            for (int i = 0; i < memSize; i += 2)
            {
                if (memSize - i == 1)
                {
                    conversionState.Append(new ASMOps.Pop {Size = OperandSize.Halfword, Dest = "$t0"});
                    //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "$t0", Dest = (offset + i).ToString() + "($t2)" });
                    GlobalMethods.StoreData(conversionState, theOp, "$t2", "$t0", offset + i, 1);
                }
                else
                {
                    conversionState.Append(new ASMOps.Pop {Size = OperandSize.Halfword, Dest = "$t0"});
                    //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "$t0", Dest = (offset + i).ToString() + "($t2)" });
                    GlobalMethods.StoreData(conversionState, theOp, "$t2", "$t0", offset + i, 2);
                }
            }
            //                                                           Rounds down             || Pop object pointer
            conversionState.Append(new ASMOps.Add
            {
                Src1 = "$sp",
                Src2 = ((stackSize - memSize)/2*2 + 4).ToString(),
                Dest = "$sp"
            });
        }
    }
}