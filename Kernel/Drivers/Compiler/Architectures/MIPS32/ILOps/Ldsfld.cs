using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.MIPS32
{
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
                        Types.TypeInfo theTypeInfo = conversionState.TheILLibrary.GetTypeInfo(theField.FieldType);
                        int size = theTypeInfo.SizeOnStackInBytes;
                        bool isFloat = Utilities.IsFloat(theField.FieldType);

                        conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                        {
                            isFloat = isFloat,
                            sizeOnStackInBytes = (size == 8 ? 8 : 4),
                            isGCManaged = theTypeInfo.IsGCManaged
                        });
                    }
                    break;
                case OpCodes.Ldsflda:
                    conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                    {
                        isFloat = false,
                        sizeOnStackInBytes = 4,
                        isGCManaged = false
                    });
                    break;
            }
        }

        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown when loading a static float field.
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
                        Types.TypeInfo theTypeInfo = conversionState.TheILLibrary.GetTypeInfo(theField.FieldType);
                        int size = /*theTypeInfo.IsValueType ? theTypeInfo.SizeOnHeapInBytes : */theTypeInfo.SizeOnStackInBytes;
                        bool isFloat = Utilities.IsFloat(theField.FieldType);

                        if (isFloat)
                        {
                            //SUPPORT - floats
                            throw new NotSupportedException("Loading static fields of type float not supported yet!");
                        }

                        if (size == 1)
                        {
                            conversionState.Append(new ASMOps.Xor() { Src1 = "$t0", Src2 = "$t0", Dest = "$t0" });
                            conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "0(" + fieldID + ")", Dest = "$t0" });
                            conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t0" });
                        }
                        else if (size == 2)
                        {
                            conversionState.Append(new ASMOps.Xor() { Src1 = "$t0", Src2 = "$t0", Dest = "$t0" });
                            conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "0(" + fieldID + ")", Dest = "$t0" });
                            conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t0" });
                        }
                        else if (size == 4)
                        {
                            conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "0(" + fieldID + ")", Dest = "$t0" });
                            conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t0" });
                        }
                        else if (size == 8)
                        {
                            conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "4(" + fieldID + ")", Dest = "$t0" });
                            conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t0" });
                            conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "0(" + fieldID + ")", Dest = "$t0" });
                            conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t0" });
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException("Loading static field that has stack size greater than 8 not supported!");
                        }

                        conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                        {
                            isFloat = isFloat,
                            sizeOnStackInBytes = (size == 8 ? 8 : 4),
                            isGCManaged = theTypeInfo.IsGCManaged
                        });
                    }
                    break;
                case OpCodes.Ldsflda:
                    //Load the address of the field i.e. address of the ASM label
                    conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = fieldID });

                    conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                    {
                        isFloat = false,
                        sizeOnStackInBytes = 4,
                        isGCManaged = false
                    });
                    break;
            }
        }
    }
}
