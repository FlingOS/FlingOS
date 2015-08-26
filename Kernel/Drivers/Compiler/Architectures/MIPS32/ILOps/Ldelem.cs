using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.MIPS32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Ldelem : IL.ILOps.Ldelem
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            Type elementType = null;
            bool pushValue = true;
            int sizeToPush = 4;
            bool isFloat = false;

            switch ((OpCodes)theOp.opCode.Value)
            {
                case OpCodes.Ldelem:
                    {
                        int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
                        elementType = conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveType(metadataToken);
                    }
                    break;

                case OpCodes.Ldelema:
                    {
                        int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
                        elementType = conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveType(metadataToken);

                        pushValue = false;
                    }
                    break;

                case OpCodes.Ldelem_R4:
                case OpCodes.Ldelem_R8:
                    throw new NotSupportedException("Ldelem op variant not supported yet!");

                case OpCodes.Ldelem_I1:
                    sizeToPush = 1;
                    elementType = typeof(sbyte);
                    break;
                case OpCodes.Ldelem_I2:
                    sizeToPush = 2;
                    elementType = typeof(Int16);
                    break;

                case OpCodes.Ldelem_U1:
                    sizeToPush = 1;
                    elementType = typeof(byte);
                    break;
                case OpCodes.Ldelem_U2:
                    sizeToPush = 2;
                    elementType = typeof(UInt16);
                    break;

                case OpCodes.Ldelem_Ref:
                    elementType = null;
                    break;

                case OpCodes.Ldelem_U4:
                    elementType = typeof(UInt32);
                    break;

                case OpCodes.Ldelem_I4:
                    elementType = typeof(Int32);
                    break;

                case OpCodes.Ldelem_I8:
                    sizeToPush = 8;
                    elementType = typeof(Int64);
                    break;
            }

            //      5.2. Pop index and array ref from our stack
            conversionState.CurrentStackFrame.Stack.Pop();
            conversionState.CurrentStackFrame.Stack.Pop();
            //      5.3. Push element onto our stack
            conversionState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                sizeOnStackInBytes = sizeToPush > 4 ? 8 : 4,
                isFloat = isFloat,
                isNewGCObject = false,
                isGCManaged = pushValue ? (elementType == null || conversionState.TheILLibrary.GetTypeInfo(elementType).IsGCManaged) : false
            });
        }

        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if constant is a floating point number.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            int currOpPosition = conversionState.PositionOf(theOp);

            conversionState.AddExternalLabel(conversionState.GetThrowNullReferenceExceptionMethodInfo().ID);
            conversionState.AddExternalLabel(conversionState.GetThrowIndexOutOfRangeExceptionMethodInfo().ID);

            Type elementType = null;
            bool pushValue = true;
            int sizeToPush = 4;
            bool signExtend = true;
            bool isFloat = false;

            switch ((OpCodes)theOp.opCode.Value)
            {
                case OpCodes.Ldelem:
                    {
                        signExtend = false;
                        //Load the metadata token used to get the type info
                        int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
                        //Get the type info for the element type
                        elementType = conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveType(metadataToken);
                    }
                    break;

                case OpCodes.Ldelema:
                    {
                        signExtend = false;
                        //Load the metadata token used to get the type info
                        int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
                        //Get the type info for the element type
                        elementType = conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveType(metadataToken);

                        pushValue = false;
                    }
                    break;

                case OpCodes.Ldelem_R4:
                case OpCodes.Ldelem_R8:
                    //TODO - Add more LdElem op variants support
                    throw new NotSupportedException("Ldelem op variant not supported yet!");

                case OpCodes.Ldelem_I1:
                    sizeToPush = 1;
                    elementType = typeof(sbyte);
                    break;
                case OpCodes.Ldelem_I2:
                    sizeToPush = 2;
                    elementType = typeof(Int16);
                    break;

                case OpCodes.Ldelem_U1:
                    sizeToPush = 1;
                    signExtend = false;
                    elementType = typeof(byte);
                    break;
                case OpCodes.Ldelem_U2:
                    sizeToPush = 2;
                    signExtend = false;
                    elementType = typeof(UInt16);
                    break;

                case OpCodes.Ldelem_Ref:
                    signExtend = false;
                    elementType = null;
                    break;

                case OpCodes.Ldelem_U4:
                    signExtend = false;
                    elementType = typeof(UInt32);
                    break;

                case OpCodes.Ldelem_I4:
                    elementType = typeof(Int32);
                    break;

                case OpCodes.Ldelem_I8:
                    sizeToPush = 8;
                    elementType = typeof(Int64);
                    break;
            }

            if (isFloat)
            {
                //TODO - Support floats
                throw new NotSupportedException("LdElem for floats not supported yet!");
            }

            //Get element from array and push the value onto the stack
            //                   (or for LdElemA push the address of the value)

            //This involves:
            // 1. Check array reference is not null
            //          - If it is, throw NullReferenceException
            // 2. Check array element type is correct
            //          - If not, throw ArrayTypeMismatchException
            // 3. Check index to get is > -1 and < array length
            //          - If not, throw IndexOutOfRangeException
            // 4. Calculate address of element
            // 5. Push the element onto the stack

            //Stack setup upon entering this op: (top-most downwards)
            // 0. Index of element to get as Int32 (word)
            // 1. Array object reference as address (word)

            Types.TypeInfo arrayTypeInfo = conversionState.GetArrayTypeInfo();

            // 1. Check array reference is not null
            //      1.1. Move array ref into $t0
            //      1.2. Compare $t0 (array ref) to 0
            //      1.3. If not zero, jump to continue execution further down
            //      1.4. Otherwise, call Exceptions.ThrowNullReferenceException

            //      1.1. Move array ref into $t0
            conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "4($sp)", Dest = "$t0", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
            //      1.2. Compare $t0 (array ref) to 0
            //      1.3. If not zero, jump to continue execution further down
            conversionState.Append(new ASMOps.Branch() { Src1 = "$t0", Src2 = "0", BranchType = ASMOps.BranchOp.BranchNotZero, DestILPosition = currOpPosition, Extension = "Continue1", UnsignedTest = true });
            //      1.4. Otherwise, call Exceptions.ThrowNullReferenceException
            conversionState.Append(new ASMOps.Call() { Target = "GetEIP" });
            conversionState.AddExternalLabel("GetEIP");
            conversionState.Append(new ASMOps.Call() { Target = conversionState.GetThrowNullReferenceExceptionMethodInfo().ID });
            conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "Continue1" });

            // 2. Check array element type is correct
            //      2.1. Move element type ref into $t0
            //      2.2. Move element type ref from array object into $t1
            //      2.3. Compare $t0 to $t1
            //      2.4. If the same, jump to continue execution further down
            //      2.5. Otherwise, call Exceptions.ThrowArrayTypeMismatchException

            //string ContinueExecutionLabel2 = ContinueExecutionLabelBase + "2";
            ////      2.1. Move element type ref into $t0
            int elemTypeOffset = conversionState.TheILLibrary.GetFieldInfo(arrayTypeInfo, "elemType").OffsetInBytes;

            // 3. Check index to get is > -1 and < array length
            //      3.1. Move index into $t0
            //      3.2. Move array length into $t1
            //      3.2. Compare $t0 to 0
            //      3.3. Jump if greater than to next test condition (3.5)
            //      3.4. Otherwise, call Exceptions.ThrowIndexOutOfRangeException
            //      3.5. Compare $t0 to $t1
            //      3.6. Jump if less than to continue execution further down
            //      3.7. Otherwise, call Exceptions.ThrowIndexOutOfRangeException

            //      3.1. Move index into $t0
            conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "0($sp)", Dest = "$t0", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
            //      3.2. Move array length into $t2
            //              - Calculate the offset of the field from the start of the array object
            int lengthOffset = conversionState.TheILLibrary.GetFieldInfo(arrayTypeInfo, "length").OffsetInBytes;

            //              - Move array ref into $t1
            conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "4($sp)", Dest = "$t1", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
            //              - Move length value (offset($t1)) into $t1
            //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = lengthOffset.ToString() + "($t1)", Dest = "$t1", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
            GlobalMethods.LoadData(conversionState, theOp, "$t1", "$t1", lengthOffset, 4);

            //      3.2. Compare $t0 to 0
            //      3.3. Jump if greater than to next test condition (3.5)
            conversionState.Append(new ASMOps.Branch() { Src1 = "$t0", Src2 = "0", BranchType = ASMOps.BranchOp.BranchGreaterThanEqual, DestILPosition = currOpPosition, Extension = "Continue3_1", UnsignedTest = false });
            //      3.4. Otherwise, call Exceptions.ThrowIndexOutOfRangeException
            conversionState.Append(new ASMOps.Call() { Target = conversionState.GetThrowIndexOutOfRangeExceptionMethodInfo().ID });
            conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "Continue3_1" });
            //      3.5. Compare $t0 to $t1
            //      3.6. Jump if less than to continue execution further down
            conversionState.Append(new ASMOps.Branch() { Src1 = "$t0", Src2 = "$t1", BranchType = ASMOps.BranchOp.BranchLessThan, DestILPosition = currOpPosition, Extension = "Continue3_2", UnsignedTest = false });
            //      3.7. Otherwise, call Exceptions.ThrowIndexOutOfRangeException
            conversionState.Append(new ASMOps.Call() { Target = conversionState.GetThrowIndexOutOfRangeExceptionMethodInfo().ID });
            conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "Continue3_2" });
            // 4. Calculate address of element
            //      4.1. Pop index into $t1
            //      4.2. Pop array ref into $t0
            //      4.3. Move element type ref (from array ref) into $t0
            //      4.4. Move IsValueType (from element ref type) into $t2
            //      4.5. If IsValueType, continue to 4.6., else goto 4.8.
            //      4.6. Move Size (from element type ref) into $t0
            //      4.7. Skip over 4.8.
            //      4.8. Move StackSize (from element type ref) into $t0
            //      4.9. Mulitply $t0 by $t1 (index by element size)
            //      4.10. Move array ref into $t1
            //      4.11. Add enough to go past Kernel.FOS_System.Array fields
            //      4.12. Add $t0 and $t1 (array ref + fields + (index * element size))

            //      4.1. Pop index into $t1
            conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t1" });
            //      4.2. Move array ref into $t0
            conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "0($sp)", Dest = "$t0", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
            //      4.3. Move element type ref (from array ref) into $t0
            //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = elemTypeOffset.ToString() + "($t0)", Dest = "$t0", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
            GlobalMethods.LoadData(conversionState, theOp, "$t0", "$t0", elemTypeOffset, 4);
            //      4.4. Move IsValueType (from element ref type) into $t2
            int isValueTypeOffset = conversionState.GetTypeFieldOffset("IsValueType");
            conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "0", Dest = "$t2", MoveType = ASMOps.Mov.MoveTypes.ImmediateToReg });
            //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = isValueTypeOffset.ToString() + "($t0)", Dest = "$t2", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
            GlobalMethods.LoadData(conversionState, theOp, "$t0", "$t2", isValueTypeOffset, 1);
            //      4.5. If IsValueType, continue to 4.6., else goto 4.8.
            conversionState.Append(new ASMOps.Branch() { Src1 = "$t2", Src2 = "0", BranchType = ASMOps.BranchOp.BranchZero, DestILPosition = currOpPosition, Extension = "Continue4_1", UnsignedTest = true });
            //      4.6. Move Size (from element type ref) into $t0
            int sizeOffset = conversionState.GetTypeFieldOffset("Size");
            //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = sizeOffset.ToString() + "($t0)", Dest = "$t0", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
            GlobalMethods.LoadData(conversionState, theOp, "$t0", "$t0", sizeOffset, 4);
            //      4.7. Skip over 4.8.
            conversionState.Append(new ASMOps.Branch() { BranchType = ASMOps.BranchOp.Branch, DestILPosition = currOpPosition, Extension = "Continue4_2" });
            //      4.8. Move StackSize (from element type ref) into $t0
            conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "Continue4_1" });
            int stackSizeOffset = conversionState.GetTypeFieldOffset("StackSize");
            //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = stackSizeOffset + "($t0)", Dest = "$t0", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
            GlobalMethods.LoadData(conversionState, theOp, "$t0", "$t0", stackSizeOffset, 4);
            //      4.9. Mulitply $t0 by $t1 (index by element size)
            conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "Continue4_2" });
            conversionState.Append(new ASMOps.Mul() { Src1 = "$t0", Src2 = "$t1", Signed = true });
            //      4.10. Pop array ref into $t1
            conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Word, Dest = "$t1" });
            //      4.11. Add enough to go past Kernel.FOS_System.Array fields
            int allFieldsOffset = 0;
            #region Offset calculation
            {
                Types.FieldInfo highestOffsetFieldInfo = arrayTypeInfo.FieldInfos.Where(x => !x.IsStatic).OrderByDescending(x => x.OffsetInBytes).First();
                Types.TypeInfo fieldTypeInfo = conversionState.TheILLibrary.GetTypeInfo(highestOffsetFieldInfo.UnderlyingInfo.FieldType);
                allFieldsOffset = highestOffsetFieldInfo.OffsetInBytes + (fieldTypeInfo.IsValueType ? fieldTypeInfo.SizeOnHeapInBytes : fieldTypeInfo.SizeOnStackInBytes);
            }
            #endregion
            conversionState.Append(new ASMOps.Add() { Src1 = "$t1", Src2 = allFieldsOffset.ToString(), Dest = "$t1" });
            //      4.12. Add $t0 and $t1 (array ref + fields + (index * element size))
            conversionState.Append(new ASMOps.Add() { Src1 = "$t1", Src2 = "$t0", Dest = "$t0" });

            // 5. Push the element onto the stack
            //      5.1. Push value at ($t0) (except for LdElemA op in which case just push address)
            if (pushValue)
            {
                switch (sizeToPush)
                {
                    case 1:
                        conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "0", Dest = "$t1", MoveType = ASMOps.Mov.MoveTypes.ImmediateToReg });
                        //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "0($t0)", Dest = "$t1", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
                        GlobalMethods.LoadData(conversionState, theOp, "$t0", "$t1", 0, 1);
                        if (signExtend)
                        {
                            throw new NotSupportedException("Sign extend byte to 4 bytes in LdElem not supported!");
                        }
                        break;
                    case 2:
                        conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "0", Dest = "$t1", MoveType = ASMOps.Mov.MoveTypes.ImmediateToReg });
                        //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Halfword, Src = "0($t0)", Dest = "$t1", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
                        GlobalMethods.LoadData(conversionState, theOp, "$t0", "$t1", 0, 2);
                        if (signExtend)
                        {
                            //Lookup! -R. 
                            //conversionState.Append(new ASMOps.Cwde());
                        }
                        break;
                    case 4:
                        //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "0($t0)", Dest = "$t1", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
                        GlobalMethods.LoadData(conversionState, theOp, "$t0", "$t1", 0, 4);
                        break;
                    case 8:
                        //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "0($t0)", Dest = "$t1", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
                        GlobalMethods.LoadData(conversionState, theOp, "$t0", "$t1", 0, 4);
                        //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "4($t0)", Dest = "$t2", MoveType = ASMOps.Mov.MoveTypes.SrcMemoryToDestReg });
                        GlobalMethods.LoadData(conversionState, theOp, "$t0", "$t2", 4, 4);
                        break;
                }
                if (sizeToPush == 8)
                {
                    conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t2" });
                }
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t1" });
            }
            else
            {
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t0" });
            }

            //      5.2. Pop index and array ref from our stack
            conversionState.CurrentStackFrame.Stack.Pop();
            conversionState.CurrentStackFrame.Stack.Pop();
            //      5.3. Push element onto our stack
            conversionState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                sizeOnStackInBytes = sizeToPush > 4 ? 8 : 4,
                isFloat = isFloat,
                isNewGCObject = false,
                isGCManaged = pushValue ? (elementType == null || conversionState.TheILLibrary.GetTypeInfo(elementType).IsGCManaged) : false
            });
        }
    }
}
