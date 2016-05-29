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
using Drivers.Compiler.Architectures.x86.ASMOps;
using Drivers.Compiler.IL;
using Drivers.Compiler.Types;

namespace Drivers.Compiler.Architectures.x86
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
            TypeInfo elemTypeInfo = null;
            //bool pushValue = true;
            int sizeOnHeap = 4;
            int sizeOnStack = 4;
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
                    elemTypeInfo = conversionState.TheILLibrary.GetTypeInfo(elementType);
                    sizeOnStack = elemTypeInfo.SizeOnStackInBytes;
                    sizeOnHeap = elemTypeInfo.IsValueType
                        ? elemTypeInfo.SizeOnHeapInBytes
                        : elemTypeInfo.SizeOnStackInBytes;
                }
                    break;

                case OpCodes.Stelem_R4:
                case OpCodes.Stelem_R8:
                    //TODO: Support floats
                    throw new NotSupportedException("Stelem op variant not supported yet!");

                case OpCodes.Stelem_I1:
                    sizeOnHeap = 1;
                    elementType = typeof(sbyte);
                    break;
                case OpCodes.Stelem_I2:
                    sizeOnHeap = 2;
                    elementType = typeof(short);
                    break;

                case OpCodes.Stelem_Ref:
                    elementType = null;
                    break;

                case OpCodes.Stelem_I4:
                    elementType = typeof(int);
                    break;

                case OpCodes.Stelem_I8:
                    sizeOnHeap = 8;
                    sizeOnStack = 8;
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
            // 0. Value to store (dword or 2 dwords)
            // 1. Index of element to get as Int32 (dword)
            // 2. Array object reference as address (dword)

            TypeInfo arrayTypeInfo = conversionState.GetArrayTypeInfo();

            // 1. Check array reference is not null
            //      1.1. Move array ref into EAX
            //      1.2. Compare EAX (array ref) to 0
            //      1.3. If not zero, jump to continue execution further down
            //      1.4. Otherwise, call Exceptions.ThrowNullReferenceException

            //      1.1. Move array ref into EAX
            conversionState.Append(new Mov
            {
                Size = OperandSize.Dword,
                Src = "[ESP+" + (sizeOnStack + 4) + "]",
                Dest = "EAX"
            });
            //      1.2. Compare EAX (array ref) to 0
            conversionState.Append(new Cmp {Arg1 = "EAX", Arg2 = "0"});
            //      1.3. If not zero, jump to continue execution further down
            conversionState.Append(new Jmp
            {
                JumpType = JmpOp.JumpNotZero,
                DestILPosition = currOpPosition,
                Extension = "Continue1"
            });
            //      1.4. Otherwise, call Exceptions.ThrowNullReferenceException
            conversionState.Append(new ASMOps.Call {Target = "GetEIP"});
            conversionState.AddExternalLabel("GetEIP");
            conversionState.Append(new ASMOps.Call
            {
                Target = conversionState.GetThrowNullReferenceExceptionMethodInfo().ID
            });
            conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "Continue1"});

            // 2. Check array element type is correct
            //      2.1. Move element type ref into EAX
            //      2.2. Move element type ref from array object into EBX
            //      2.3. Compare EAX to EBX
            //      2.4. If the same, jump to continue execution further down
            //      2.5. Otherwise, call Exceptions.ThrowArrayTypeMismatchException

            //string ContinueExecutionLabel2 = ContinueExecutionLabelBase + "2";
            ////      2.1. Move element type ref into EAX
            //if (elementType != null)
            //{
            //    result.AppendLine(string.Format("mov EAX, {0}", conversionState.GetTypeIdString(conversionState.GetTypeID(elementType))));
            //}
            //else
            //{
            //    //Should be the same for all classes since they are (indirectly) derived from ObjectWithType
            //    int typeOffset = conversionState.GetFieldOffset(arrayDBType, "_Type");

            //    //      - Move value (which is a ref) into EAX
            //    GlobalMethods.CheckAddrFromRegister(conversionState, "ESP", 0);
            //    result.AppendLine("mov EAX, [ESP]");
            //    //      - Move value type ref (from value (ref)) into EAX
            //    GlobalMethods.CheckAddrFromRegister(conversionState, "EAX", typeOffset);
            //    result.AppendLine(string.Format("mov EAX, [EAX+{0}]", typeOffset));
            //}
            ////      2.2. Move element type ref from array object into EBX
            ////              - Move array ref into EBX
            //GlobalMethods.CheckAddrFromRegister(conversionState, "ESP", sizeToPop == 8 ? 12 : 8);
            //result.AppendLine(string.Format("mov EBX, [ESP+{0}]", sizeToPop == 8 ? 12 : 8));
            ////              - Move elemType ref ([EBX+offset]) into EBX
            int elemTypeOffset = conversionState.TheILLibrary.GetFieldInfo(arrayTypeInfo, "elemType").OffsetInBytes;
            //GlobalMethods.CheckAddrFromRegister(conversionState, "EBX", elemTypeOffset);
            //result.AppendLine(string.Format("mov EBX, [EBX+{0}]", elemTypeOffset));
            ////      2.3. Compare EAX to EBX
            //result.AppendLine("cmp EAX, EBX");
            ////      2.4. If the same, jump to continue execution further down
            //result.AppendLine("je " + ContinueExecutionLabel2);
            ////      2.5. Otherwise, call Exceptions.ThrowArrayTypeMismatchException
            //result.AppendLine(string.Format("call {0}", conversionState.GetMethodID(conversionState.ThrowArrayTypeMismatchExceptionMethod)));
            //result.AppendLine(ContinueExecutionLabel2 + ":");

            // 3. Check index to get is > -1 and < array length
            //      3.1. Move index into EAX
            //      3.2. Move array length into EBX
            //      3.2. Compare EAX to 0
            //      3.3. Jump if greater than to next test condition (3.5)
            //      3.4. Otherwise, call Exceptions.ThrowIndexOutOfRangeException
            //      3.5. Compare EAX to EBX
            //      3.6. Jump if less than to continue execution further down
            //      3.7. Otherwise, call Exceptions.ThrowIndexOutOfRangeException

            //      3.1. Move index into EAX
            conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "[ESP+" + sizeOnStack + "]", Dest = "EAX"});
            //      3.2. Move array length into ECX
            //              - Calculate the offset of the field from the start of the array object
            int lengthOffset = conversionState.TheILLibrary.GetFieldInfo(arrayTypeInfo, "length").OffsetInBytes;
            //              - Move array ref into EBX
            conversionState.Append(new Mov
            {
                Size = OperandSize.Dword,
                Src = "[ESP+" + (sizeOnStack + 4) + "]",
                Dest = "EBX"
            });
            //              - Move length value ([EBX+offset]) into EBX
            conversionState.Append(new Mov
            {
                Size = OperandSize.Dword,
                Src = "[EBX+" + lengthOffset + "]",
                Dest = "EBX"
            });
            //      3.2. Compare EAX to 0
            conversionState.Append(new Cmp {Arg1 = "EAX", Arg2 = "0"});
            //      3.3. Jump if greater than to next test condition (3.5)
            conversionState.Append(new Jmp
            {
                JumpType = JmpOp.JumpGreaterThanEqual,
                DestILPosition = currOpPosition,
                Extension = "Continue3_1"
            });
            //      3.4. Otherwise, call Exceptions.ThrowIndexOutOfRangeException
            conversionState.Append(new ASMOps.Call
            {
                Target = conversionState.GetThrowIndexOutOfRangeExceptionMethodInfo().ID
            });
            conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "Continue3_1"});
            //      3.5. Compare EAX to EBX
            conversionState.Append(new Cmp {Arg1 = "EAX", Arg2 = "EBX"});
            //      3.6. Jump if less than to continue execution further down
            conversionState.Append(new Jmp
            {
                JumpType = JmpOp.JumpLessThan,
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
            //      4.1. Move index into EDX
            //      4.2. Move array ref into EAX
            //      4.3. Move element type ref (from array ref) into EAX
            //      4.4. Push EAX
            //      4.5. Move IsValueType (from element ref type) into EAX
            //      4.6. If IsValueType, continue to 4.7., else goto 4.10.
            //      4.7. Pop EAX
            //      4.8. Move Size (from element type ref) into EAX
            //      4.9. Skip over 4.9. and 4.10.
            //      4.10. Pop EAX
            //      4.11. Move StackSize (from element type ref) into EAX
            //      4.12. Mulitply EAX by EDX (index by element size)
            //      4.13. Move array ref into EDX
            //      4.14. Add enough to go past Kernel.Framework.Array fields
            //      4.15. Add EAX and EBX (array ref + fields + (index * element size))


            //      4.1. Move index into EDX
            conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "[ESP+" + sizeOnStack + "]", Dest = "EDX"});
            //      4.2. Move array ref into EAX
            conversionState.Append(new Mov
            {
                Size = OperandSize.Dword,
                Src = "[ESP+" + (sizeOnStack + 4) + "]",
                Dest = "EAX"
            });
            //      4.3. Move element type ref (from array ref) into EAX
            conversionState.Append(new Mov
            {
                Size = OperandSize.Dword,
                Src = "[EAX+" + elemTypeOffset + "]",
                Dest = "EAX"
            });
            //      4.4. Push EAX
            conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EAX"});
            //      4.5. Move IsValueType (from element ref type) into EAX
            int isValueTypeOffset = conversionState.GetTypeFieldOffset("IsValueType");
            conversionState.Append(new Mov
            {
                Size = OperandSize.Byte,
                Src = "[EAX+" + isValueTypeOffset + "]",
                Dest = "AL"
            });
            //      4.6. If IsValueType, continue to 4.7., else goto 4.9.
            conversionState.Append(new Test {Arg1 = "EAX", Arg2 = "1"});
            conversionState.Append(new Jmp
            {
                JumpType = JmpOp.JumpZero,
                DestILPosition = currOpPosition,
                Extension = "Continue4_1"
            });
            //      4.7. Pop EAX
            conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EAX"});
            //      4.8. Move Size (from element type ref) into EAX
            int sizeOffset = conversionState.GetTypeFieldOffset("Size");
            conversionState.Append(new Mov
            {
                Size = OperandSize.Dword,
                Src = "[EAX+" + sizeOffset + "]",
                Dest = "EAX"
            });
            //      4.9. Skip over 4.9. and 4.10.
            conversionState.Append(new Jmp
            {
                JumpType = JmpOp.Jump,
                DestILPosition = currOpPosition,
                Extension = "Continue4_2"
            });
            //      4.10. Pop EAX
            conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "Continue4_1"});
            conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EAX"});
            //      4.11. Move StackSize (from element type ref) into EAX
            int stackSizeOffset = conversionState.GetTypeFieldOffset("StackSize");
            conversionState.Append(new Mov
            {
                Size = OperandSize.Dword,
                Src = "[EAX+" + stackSizeOffset + "]",
                Dest = "EAX"
            });
            //      4.12. Mulitply EAX by EDX (index by element size)
            conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "Continue4_2"});
            conversionState.Append(new ASMOps.Mul {Arg = "EDX"});
            //      4.13. Move array ref into EDX
            conversionState.Append(new Mov
            {
                Size = OperandSize.Dword,
                Dest = "EDX",
                Src = "[ESP+" + (sizeOnStack + 4) + "]"
            });
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

            conversionState.Append(new ASMOps.Add {Src = allFieldsOffset.ToString(), Dest = "EDX"});
            //      4.15. Add EAX and EDX (array ref + fields + (index * element size))
            conversionState.Append(new ASMOps.Add {Src = "EDX", Dest = "EAX"});

            // 5. Pop the element from the stack to array
            //      5.1. Pop value bytes from stack to array
            //      5.2. Add 8 to ESP to remove Index and Array ref
            for (int i = 0; i < sizeOnStack; i += 4)
            {
                conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "ECX"});

                switch (sizeOnHeap)
                {
                    case 1:
                        conversionState.Append(new Mov {Size = OperandSize.Byte, Src = "CL", Dest = "[EAX+" + i + "]"});
                        break;
                    case 2:
                        conversionState.Append(new Mov {Size = OperandSize.Word, Src = "CX", Dest = "[EAX+" + i + "]"});
                        break;
                    case 3:
                        conversionState.Append(new Mov {Size = OperandSize.Byte, Src = "CL", Dest = "[EAX+" + i + "]"});
                        conversionState.Append(new ASMOps.Shr {Src = "16", Dest = "ECX"});
                        conversionState.Append(new Mov
                        {
                            Size = OperandSize.Word,
                            Src = "CX",
                            Dest = "[EAX+" + (i + 1) + "]"
                        });
                        break;
                    default:
                        conversionState.Append(new Mov
                        {
                            Size = OperandSize.Dword,
                            Src = "ECX",
                            Dest = "[EAX+" + i + "]"
                        });
                        break;
                }

                sizeOnHeap -= 4;
            }

            //      5.2. Add 8 to ESP to remove Index and Array ref
            conversionState.Append(new ASMOps.Add {Src = "8", Dest = "ESP"});

            //      5.2. Pop index, array ref and value from our stack
            conversionState.CurrentStackFrame.GetStack(theOp).Pop();
            conversionState.CurrentStackFrame.GetStack(theOp).Pop();
            conversionState.CurrentStackFrame.GetStack(theOp).Pop();
        }
    }
}