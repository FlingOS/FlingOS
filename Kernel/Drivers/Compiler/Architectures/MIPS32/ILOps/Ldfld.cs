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
            conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t2" });
            if ((OpCodes)theOp.opCode.Value == OpCodes.Ldflda)
            {
                conversionState.Append(new ASMOps.Add() { Src1 = offset.ToString(), Src2 = "$t2", Dest = "$t2" });
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t2" });

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
                    conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$zero" });
                }
                for (int i = memSize + (memSize % 2); i > 0; i -= 2)
                {
                    if (sizeToSub != sizeNotInMem)
                    {
                        conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Halfword, Src = "0", Dest = "$t0", MoveType = ASMOps.Mov.MoveTypes.ImmediateToReg });
                        GlobalMethods.LoadData(conversionState, theOp, "$t2", "$t1", (offset + i - 2), 1);
                        //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = (offset + i - 2).ToString() + "($t2)", Dest = "$t1", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
                        conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Halfword, Src = "$t0" });
                    }
                    else
                    {
                        GlobalMethods.LoadData(conversionState, theOp, "$t2", "$t0", (offset + i - 2), 2);
                        //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Halfword, Src = (offset + i - 2).ToString() + "($t2)", Dest = "$t0", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
                        conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Halfword, Src = "$t0" });
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
