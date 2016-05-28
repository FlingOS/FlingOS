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
    public class Stsfld : IL.ILOps.Stsfld
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
            int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
            FieldInfo theField = conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveField(metadataToken);
            Types.FieldInfo theFieldInfo = conversionState.GetFieldInfo(theField.DeclaringType, theField.Name);
            TypeInfo theFieldTypeInfo = conversionState.TheILLibrary.GetTypeInfo(theFieldInfo.FieldType);

            string fieldId = theFieldInfo.ID;
            int size = /*theFieldTypeInfo.IsValueType ? theFieldTypeInfo.SizeOnHeapInBytes : */
                theFieldTypeInfo.SizeOnStackInBytes;
            bool isFloat = Utilities.IsFloat(theField.FieldType);

            conversionState.AddExternalLabel(fieldId);

            StackItem value = conversionState.CurrentStackFrame.GetStack(theOp).Pop();

            if (isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Storing static fields of type float not supported yet!");
            }

            conversionState.Append(new La {Dest = "$t4", Label = fieldId});

            if (size == 1)
            {
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t0"});
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Byte,
                    Src = "$t1",
                    Dest = "0($t4)",
                    MoveType = Mov.MoveTypes.SrcRegToDestMemory
                });
            }
            else if (size == 2)
            {
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t0"});
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Halfword,
                    Src = "$t1",
                    Dest = "0($t4)",
                    MoveType = Mov.MoveTypes.SrcRegToDestMemory
                });
            }
            else if (size == 4)
            {
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t0"});
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "$t0",
                    Dest = "0($t4)",
                    MoveType = Mov.MoveTypes.SrcRegToDestMemory
                });
            }
            else if (size == 8)
            {
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t0"});
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "$t0",
                    Dest = "0($t4)",
                    MoveType = Mov.MoveTypes.SrcRegToDestMemory
                });
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t0"});
                conversionState.Append(new Mov
                {
                    Size = OperandSize.Word,
                    Src = "$t0",
                    Dest = "4($t4)",
                    MoveType = Mov.MoveTypes.SrcRegToDestMemory
                });
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    "Storing static field that has stack size greater than 8 not supported!");
            }

            if (value.sizeOnStackInBytes - size > 0)
            {
                conversionState.Append(new ASMOps.Add
                {
                    Src1 = "$sp",
                    Src2 = (value.sizeOnStackInBytes - size).ToString(),
                    Dest = "$sp"
                });
            }
        }
    }
}