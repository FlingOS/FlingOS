using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.MIPS32
{
    public class Stsfld : IL.ILOps.Stsfld
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            conversionState.CurrentStackFrame.Stack.Pop();
        }
        
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if the value to store is floating point.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
            FieldInfo theField = conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveField(metadataToken);
            Types.FieldInfo theFieldInfo = conversionState.GetFieldInfo(theField.DeclaringType, theField.Name);
            Types.TypeInfo theFieldTypeInfo = conversionState.TheILLibrary.GetTypeInfo(theFieldInfo.FieldType);

            string fieldId = theFieldInfo.ID;
            int size = /*theFieldTypeInfo.IsValueType ? theFieldTypeInfo.SizeOnHeapInBytes : */theFieldTypeInfo.SizeOnStackInBytes;
            bool isFloat = Utilities.IsFloat(theField.FieldType);

            conversionState.AddExternalLabel(fieldId);

            StackItem value = conversionState.CurrentStackFrame.Stack.Pop();

            if (isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Storing static fields of type float not supported yet!");
            }

            if (size == 1)
            {
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "$t1", Dest = "0(" + fieldId + ")", DestIsMemory = true });
            }
            else if (size == 2)
            {
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "$t1", Dest = "0(" + fieldId + ")", DestIsMemory = true });
            }
            else if (size == 4)
            {
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "$t0", Dest = "0(" + fieldId + ")", DestIsMemory = true });
            }
            else if (size == 8)
            {
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "$t0", Dest = "0(" + fieldId + ")", DestIsMemory = true });
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t0" });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "$t0", Dest = "4(" + fieldId + ")", DestIsMemory = true });
            }
            else
            {
                throw new ArgumentOutOfRangeException("Storing static field that has stack size greater than 8 not supported!");
            }

            if (value.sizeOnStackInBytes - size > 0)
            {
                conversionState.Append(new ASMOps.Add() { Src1 = (value.sizeOnStackInBytes - size).ToString(), Src2 = "$sp", Dest = "$sp" });
            }
        }
    }
}
