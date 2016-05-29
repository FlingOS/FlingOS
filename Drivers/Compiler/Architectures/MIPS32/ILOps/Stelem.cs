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
using System.Linq;
using Drivers.Compiler.Architectures.MIPS32.ASMOps;
using Drivers.Compiler.IL;
using Drivers.Compiler.Types;

namespace Drivers.Compiler.Architectures.MIPS32
{
    /// <summary>
    ///     See base class documentation.
    /// </summary>
    public class Stelem : IL.ILOps.Stelem
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            conversionState.CurrentStackFrame.GetStack(theOp).Pop();
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
        ///     Thrown if constant is a floating point number.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            Type elementType = null;
            //bool pushValue = true;
            int sizeToPop = 4;
            bool isFloat = false;

            int currOpPosition = conversionState.PositionOf(theOp);

            conversionState.AddExternalLabel(conversionState.GetThrowNullReferenceExceptionMethodInfo().ID);
            conversionState.AddExternalLabel(conversionState.GetThrowIndexOutOfRangeExceptionMethodInfo().ID);

            switch ((OpCodes)theOp.opCode.Value)
            {
                case OpCodes.Stelem:
                {
                    //Load the metadata token used to get the type info
                    int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    //Get the type info for the element type
                    elementType = conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveType(metadataToken);
                }
                    break;

                case OpCodes.Stelem_R4:
                case OpCodes.Stelem_R8:
                    //TODO: Support floats
                    throw new NotSupportedException("Stelem op variant not supported yet!");

                case OpCodes.Stelem_I1:
                    sizeToPop = 1;
                    elementType = typeof(sbyte);
                    break;
                case OpCodes.Stelem_I2:
                    sizeToPop = 2;
                    elementType = typeof(short);
                    break;

                case OpCodes.Stelem_Ref:
                    elementType = null;
                    break;

                case OpCodes.Stelem_I4:
                    elementType = typeof(int);
                    break;

                case OpCodes.Stelem_I8:
                    sizeToPop = 8;
                    elementType = typeof(long);
                    break;
            }

            if (isFloat)
            {
                //TODO: Support floats
                throw new NotSupportedException("StElem for floats not supported yet!");
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
            // 5. Pop the value from the stack into the element

            //Stack setup upon entering this op: (top-most downwards)
            // 0. Value to store (word or 2 words)
            // 1. Index of element to get as Int32 (word)
            // 2. Array object reference as address (word)

            TypeInfo arrayTypeInfo = conversionState.GetArrayTypeInfo();

            // 1. Check array reference is not null
            //      1.1. Move array ref into $t0
            //      1.2. Compare $t0 (array ref) to 0
            //      1.3. If not zero, jump to continue execution further down
            //      1.4. Otherwise, call Exceptions.ThrowNullReferenceException

            //      1.1. Move array ref into $t0
            conversionState.Append(new Mov
            {
                Size = OperandSize.Word,
                Src = (sizeToPop == 8 ? 12 : 8) + "($sp)",
                Dest = "$t0",
                MoveType = Mov.MoveTypes.SrcMemoryToDestReg
            });
            //      1.2. Compare $t0 (array ref) to 0
            //      1.3. If not zero, jump to continue execution further down
            conversionState.Append(new Branch
            {
                BranchType = BranchOp.BranchNotZero,
                Src1 = "$t0",
                DestILPosition = currOpPosition,
                Extension = "Continue1"
            });
            //      1.4. Otherwise, call Exceptions.Throw1NullReferenceException
            conversionState.Append(new ASMOps.Call {Target = "GetEIP"});
            conversionState.AddExternalLabel("GetEIP");
            conversionState.Append(new ASMOps.Call
            {
                Target = conversionState.GetThrowNullReferenceExceptionMethodInfo().ID
            });
            conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "Continue1"});

            // 2. Check array element type is correct
            //      2.1. Move element type ref into $t0
            //      2.2. Move element type ref from array object into $t1
            //      2.3. Compare $t0 to $t1
            //      2.4. If the same, jump to continue execution further down
            //      2.5. Otherwise, call Exceptions.ThrowArrayTypeMismatchException

            //string ContinueExecutionLabel2 = ContinueExecutionLabelBase + "2";
            ////      2.1. Move element type ref into $t0
            //if (elementType != null)
            //{
            //    result.AppendLine(string.Format("mov $t0, {0}", conversionState.GetTypeIdString(conversionState.GetTypeID(elementType))));
            //}
            //else
            //{
            //    //Should be the same for all classes since they are (indirectly) derived from ObjectWithType
            //    int typeOffset = conversionState.GetFieldOffset(arrayDBType, "_Type");

            //    //      - Move value (which is a ref) into $t0
            //    GlobalMethods.CheckAddrFromRegister(conversionState, "$sp", 0);
            //    result.AppendLine("mov $t0, 0($sp)");
            //    //      - Move value type ref (from value (ref)) into $t0
            //    GlobalMethods.CheckAddrFromRegister(conversionState, "$t0", typeOffset);
            //    result.AppendLine(string.Format("mov $t0, {0}($t0)", typeOffset));
            //}
            ////      2.2. Move element type ref from array object into $t1
            ////              - Move array ref into $t1
            //GlobalMethods.CheckAddrFromRegister(conversionState, "$sp", sizeToPop == 8 ? 12 : 8);
            //result.AppendLine(string.Format("mov $t1, {0}($sp)", sizeToPop == 8 ? 12 : 8));
            ////              - Move elemType ref (offset($t1)) into $t1
            int elemTypeOffset = conversionState.TheILLibrary.GetFieldInfo(arrayTypeInfo, "elemType").OffsetInBytes;
            //GlobalMethods.CheckAddrFromRegister(conversionState, "$t1", elemTypeOffset);
            //result.AppendLine(string.Format("mov $t1, {0}($t1)", elemTypeOffset));
            ////      2.3. Compare $t0 to $t1
            //result.AppendLine("cmp $t0, $t1");
            ////      2.4. If the same, jump to continue execution further down
            //result.AppendLine("je " + ContinueExecutionLabel2);
            ////      2.5. Otherwise, call Exceptions.ThrowArrayTypeMismatchException
            //result.AppendLine(string.Format("call {0}", conversionState.GetMethodID(conversionState.ThrowArrayTypeMismatchExceptionMethod)));
            //result.AppendLine(ContinueExecutionLabel2 + ":");

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
            conversionState.Append(new Mov
            {
                Size = OperandSize.Word,
                Src = (sizeToPop == 8 ? 8 : 4) + "($sp)",
                Dest = "$t0",
                MoveType = Mov.MoveTypes.SrcMemoryToDestReg
            });
            //      3.2. Move array length into $t2
            //              - Calculate the offset of the field from the start of the array object
            int lengthOffset = conversionState.TheILLibrary.GetFieldInfo(arrayTypeInfo, "length").OffsetInBytes;
            //              - Move array ref into $t1
            conversionState.Append(new Mov
            {
                Size = OperandSize.Word,
                Src = (sizeToPop == 8 ? 12 : 8) + "($sp)",
                Dest = "$t1",
                MoveType = Mov.MoveTypes.SrcMemoryToDestReg
            });
            //              - Move length value (offset($t1)) into $t1
            //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = lengthOffset.ToString() + "($t1)", Dest = "$t1" });
            GlobalMethods.LoadData(conversionState, theOp, "$t1", "$t1", lengthOffset, 4);
            //      3.2. Compare $t0 to 0
            //      3.3. Jump if greater than to next test condition (3.5)
            conversionState.Append(new Branch
            {
                BranchType = BranchOp.BranchGreaterThanEqual,
                Src1 = "$t0",
                Src2 = "$zero",
                DestILPosition = currOpPosition,
                Extension = "Continue3_1"
            });
            //      3.4. Otherwise, call Exceptions.ThrowIndexOutOfRangeException
            conversionState.Append(new ASMOps.Call
            {
                Target = conversionState.GetThrowIndexOutOfRangeExceptionMethodInfo().ID
            });
            conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "Continue3_1"});
            //      3.5. Compare $t0 to $t1
            //      3.6. Jump if less than to continue execution further down
            conversionState.Append(new Branch
            {
                BranchType = BranchOp.BranchLessThan,
                Src1 = "$t0",
                Src2 = "$t1",
                DestILPosition = currOpPosition,
                Extension = "Continue3_2"
            });
            //      3.7. Otherwise, call Exceptions.ThrowIndexOutOfRangeException
            conversionState.Append(new ASMOps.Call
            {
                Target = conversionState.GetThrowIndexOutOfRangeExceptionMethodInfo().ID
            });
            conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "Continue3_2"});

            // 4. Calculate address of element
            //      4.0. Pop value into $t2:$t1
            //      4.1. Pop index into $t3
            //      4.2. Pop array ref into $t0
            //      4.3. Move element type ref (from array ref) into $t0
            //      4.4. Push $t0
            //      4.5. Move IsValueType (from element ref type) into $t0
            //      4.6. If IsValueType, continue to 4.6., else goto 4.9.
            //      4.7. Pop $t0
            //      4.8. Move Size (from element type ref) into $t0
            //      4.9. Skip over 4.9. and 4.10.
            //      4.10. Pop $t0
            //      4.11. Move StackSize (from element type ref) into $t0
            //      4.12. Mulitply $t0 by $t3 (index by element size)
            //      4.13. Move array ref into $t3
            //      4.14. Add enough to go past Kernel.Framework.Array fields
            //      4.15. Add $t0 and $t1 (array ref + fields + (index * element size))

            //      4.0. Pop value into $t2:$t1
            conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t2"});
            if (sizeToPop == 8)
            {
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t1"});
            }
            //      4.1. Pop index into $t3
            conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t3"});
            //      4.2. Move array ref into $t0
            conversionState.Append(new Mov
            {
                Size = OperandSize.Word,
                Src = "0($sp)",
                Dest = "$t0",
                MoveType = Mov.MoveTypes.SrcMemoryToDestReg
            });
            //      4.3. Move element type ref (from array ref) into $t0
            //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = elemTypeOffset.ToString() + "($t0)", Dest = "$t0" });
            GlobalMethods.LoadData(conversionState, theOp, "$t0", "$t0", elemTypeOffset, 4);
            //      4.4. Push $t0
            conversionState.Append(new Push {Size = OperandSize.Word, Src = "$t0"});
            //      4.5. Move IsValueType (from element ref type) into $t0
            int isValueTypeOffset = conversionState.GetTypeFieldOffset("IsValueType");
            //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = isValueTypeOffset.ToString() + "($t0)", Dest = "$t0" });
            GlobalMethods.LoadData(conversionState, theOp, "$t0", "$t0", isValueTypeOffset, 1);
            //      4.6. If IsValueType, continue to 4.7., else goto 4.9.
            conversionState.Append(new ASMOps.And {Src1 = "$t0", Src2 = "1", Dest = "$t4"});
            conversionState.Append(new Branch
            {
                BranchType = BranchOp.BranchZero,
                Src1 = "$t4",
                DestILPosition = currOpPosition,
                Extension = "Continue4_1"
            });
            //      4.7. Pop $t0
            conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t0"});
            //      4.8. Move Size (from element type ref) into $t0
            int sizeOffset = conversionState.GetTypeFieldOffset("Size");
            //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = sizeOffset.ToString() + "($t0)", Dest = "$t0" });
            GlobalMethods.LoadData(conversionState, theOp, "$t0", "$t0", sizeOffset, 4);
            //      4.9. Skip over 4.9. and 4.10.
            conversionState.Append(new Branch
            {
                BranchType = BranchOp.Branch,
                DestILPosition = currOpPosition,
                Extension = "Continue4_2"
            });
            //      4.10. Pop $t0
            conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "Continue4_1"});
            conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t0"});
            //      4.11. Move StackSize (from element type ref) into $t0
            int stackSizeOffset = conversionState.GetTypeFieldOffset("StackSize");
            //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = stackSizeOffset.ToString() + "($t0)", Dest = "$t0" });
            GlobalMethods.LoadData(conversionState, theOp, "$t0", "$t0", stackSizeOffset, 4);
            //      4.12. Mulitply $t0 by $t3 (index by element size)
            conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "Continue4_2"});
            conversionState.Append(new ASMOps.Mul {Src1 = "$t3", Src2 = "$t0"});
            conversionState.Append(new Mflo {Dest = "$t0"});
            //      4.13. Pop array ref into $t3
            conversionState.Append(new ASMOps.Pop {Size = OperandSize.Word, Dest = "$t3"});
            //      4.14. Add enough to go past Kernel.Framework.Array fields
            int allFieldsOffset = 0;

            #region Offset calculation

            {
                FieldInfo highestOffsetFieldInfo =
                    arrayTypeInfo.FieldInfos.Where(x => !x.IsStatic).OrderByDescending(x => x.OffsetInBytes).First();
                TypeInfo fieldTypeInfo =
                    conversionState.TheILLibrary.GetTypeInfo(highestOffsetFieldInfo.UnderlyingInfo.FieldType);
                allFieldsOffset = highestOffsetFieldInfo.OffsetInBytes +
                                  (fieldTypeInfo.IsValueType
                                      ? fieldTypeInfo.SizeOnHeapInBytes
                                      : fieldTypeInfo.SizeOnStackInBytes);
            }

            #endregion

            conversionState.Append(new ASMOps.Add {Src1 = "$t3", Src2 = allFieldsOffset.ToString(), Dest = "$t3"});
            //      4.15. Add $t0 and $t3 (array ref + fields + (index * element size))
            conversionState.Append(new ASMOps.Add {Src1 = "$t3", Src2 = "$t0", Dest = "$t0"});

            // 5. Pop the element from the stack to array
            //      5.1. Move value in $t1:$t2 to 0($t0)
            if (sizeToPop == 8)
            {
                //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "$t2", Dest = "0($t0)" });
                GlobalMethods.StoreData(conversionState, theOp, "$t0", "$t2", 0, 4);
                //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "$t1", Dest = "4($t0)" });
                GlobalMethods.StoreData(conversionState, theOp, "$t0", "$t1", 4, 4);
            }
            else if (sizeToPop == 4)
            {
                //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "$t2", Dest = "0($t0)" });
                GlobalMethods.StoreData(conversionState, theOp, "$t0", "$t2", 0, 4);
            }
            else if (sizeToPop == 2)
            {
                //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Halfword, Src = "$t2", Dest = "0($t0)" });
                GlobalMethods.StoreData(conversionState, theOp, "$t0", "$t2", 0, 2);
            }
            else if (sizeToPop == 1)
            {
                //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "$t2", Dest = "0($t0)" });
                GlobalMethods.StoreData(conversionState, theOp, "$t0", "$t2", 0, 1);
            }

            //      5.2. Pop index, array ref and value from our stack
            conversionState.CurrentStackFrame.GetStack(theOp).Pop();
            conversionState.CurrentStackFrame.GetStack(theOp).Pop();
            conversionState.CurrentStackFrame.GetStack(theOp).Pop();
        }
    }
}