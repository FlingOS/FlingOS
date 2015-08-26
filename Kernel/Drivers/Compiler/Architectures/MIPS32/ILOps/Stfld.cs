using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.MIPS32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Stfld : IL.ILOps.Stfld
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            conversionState.CurrentStackFrame.Stack.Pop();
            conversionState.CurrentStackFrame.Stack.Pop();
        }

                /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if the value to store is floating point or
        /// if the value is not 4 or 8 bytes in size.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
            FieldInfo theField = conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveField(metadataToken);
            Types.TypeInfo objTypeInfo = conversionState.TheILLibrary.GetTypeInfo(theField.DeclaringType);
            Types.TypeInfo fieldTypeInfo = conversionState.TheILLibrary.GetTypeInfo(theField.FieldType);
            Types.FieldInfo theFieldInfo = conversionState.TheILLibrary.GetFieldInfo(objTypeInfo, theField.Name);
            

            int offset = theFieldInfo.OffsetInBytes;

            int stackSize = fieldTypeInfo.SizeOnStackInBytes;
            int memSize = fieldTypeInfo.IsValueType ? fieldTypeInfo.SizeOnHeapInBytes : stackSize;

            StackItem value = conversionState.CurrentStackFrame.Stack.Pop();
            StackItem objPointer = conversionState.CurrentStackFrame.Stack.Pop();
            
            if (value.isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Storing fields of type float not supported yet!");
            }

            //Get object pointer
            conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = stackSize.ToString() + "($sp)", Dest = "$t2", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
            //Pop and mov value
            for (int i = 0; i < memSize; i += 2)
            {
                if (memSize - i == 1)
                {
                    conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Halfword, Dest = "$t0" });
                    //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "$t0", Dest = (offset + i).ToString() + "($t2)" });
                    GlobalMethods.StoreData(conversionState, theOp, "$t2", "$t0", (offset + i), 1);
                }
                else
                {
                    conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Halfword, Dest = "$t0" });
                    //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "$t0", Dest = (offset + i).ToString() + "($t2)" });
                    GlobalMethods.StoreData(conversionState, theOp, "$t2", "$t0", (offset + i), 2);
                }
            }
            //                                                           Rounds down             || Pop object pointer
            conversionState.Append(new ASMOps.Add() { Src1 = "$sp", Src2 = ((((stackSize - memSize) / 2) * 2) + 4).ToString(), Dest = "$sp" });
        }
    }
}
