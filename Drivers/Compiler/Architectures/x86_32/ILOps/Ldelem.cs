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
                    elementType = typeof(short);
                    break;

                case OpCodes.Ldelem_U1:
                    sizeToPush = 1;
                    elementType = typeof(byte);
                    break;
                case OpCodes.Ldelem_U2:
                    sizeToPush = 2;
                    elementType = typeof(ushort);
                    break;

                case OpCodes.Ldelem_Ref:
                    elementType = null;
                    break;

                case OpCodes.Ldelem_U4:
                    elementType = typeof(uint);
                    break;

                case OpCodes.Ldelem_I4:
                    elementType = typeof(int);
                    break;

                case OpCodes.Ldelem_I8:
                    sizeToPush = 8;
                    elementType = typeof(long);
                    break;
            }

            TypeInfo elemTypeInfo = elementType == null ? null : conversionState.TheILLibrary.GetTypeInfo(elementType);

            //      5.2. Pop index and array ref from our stack
            conversionState.CurrentStackFrame.GetStack(theOp).Pop();
            conversionState.CurrentStackFrame.GetStack(theOp).Pop();
            //      5.3. Push element onto our stack
            conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
            {
                sizeOnStackInBytes = sizeToPush > 4 ? 8 : 4,
                isFloat = isFloat,
                isNewGCObject = false,
                isGCManaged = pushValue ? elementType == null || elemTypeInfo.IsGCManaged : false,
                isValue = pushValue ? elementType != null && elemTypeInfo.IsValueType : false
            });
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
            int currOpPosition = conversionState.PositionOf(theOp);

            conversionState.AddExternalLabel(conversionState.GetThrowNullReferenceExceptionMethodInfo().ID);
            conversionState.AddExternalLabel(conversionState.GetThrowIndexOutOfRangeExceptionMethodInfo().ID);

            Type elementType = null;
            TypeInfo elemTypeInfo = null;
            bool pushValue = true;
            int sizeToPush = 4;
            int sizeToLoad = 4;
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
                    elemTypeInfo = conversionState.TheILLibrary.GetTypeInfo(elementType);
                    sizeToLoad = elemTypeInfo.IsValueType
                        ? elemTypeInfo.SizeOnHeapInBytes
                        : elemTypeInfo.SizeOnStackInBytes;
                    sizeToPush = elemTypeInfo.SizeOnStackInBytes;
                }
                    break;

                case OpCodes.Ldelema:
                {
                    signExtend = false;
                    //Load the metadata token used to get the type info
                    int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
                    //Get the type info for the element type
                    elementType = conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveType(metadataToken);
                    elemTypeInfo = conversionState.TheILLibrary.GetTypeInfo(elementType);
                    sizeToPush = 4;
                    sizeToLoad = 0;
                    pushValue = false;
                }
                    break;

                case OpCodes.Ldelem_R4:
                case OpCodes.Ldelem_R8:
                    //TODO: Support floats
                    throw new NotSupportedException("Ldelem op variant not supported yet!");

                case OpCodes.Ldelem_I1:
                    sizeToPush = 4;
                    sizeToLoad = 1;
                    elementType = typeof(sbyte);
                    elemTypeInfo = conversionState.TheILLibrary.GetTypeInfo(elementType);
                    break;
                case OpCodes.Ldelem_I2:
                    sizeToPush = 4;
                    sizeToLoad = 2;
                    elementType = typeof(short);
                    elemTypeInfo = conversionState.TheILLibrary.GetTypeInfo(elementType);
                    break;

                case OpCodes.Ldelem_U1:
                    sizeToPush = 4;
                    sizeToLoad = 1;
                    signExtend = false;
                    elementType = typeof(byte);
                    elemTypeInfo = conversionState.TheILLibrary.GetTypeInfo(elementType);
                    break;
                case OpCodes.Ldelem_U2:
                    sizeToPush = 4;
                    sizeToLoad = 2;
                    signExtend = false;
                    elementType = typeof(ushort);
                    elemTypeInfo = conversionState.TheILLibrary.GetTypeInfo(elementType);
                    break;

                case OpCodes.Ldelem_Ref:
                    signExtend = false;
                    elementType = null;
                    elemTypeInfo = null;
                    sizeToPush = 4;
                    sizeToLoad = 4;
                    break;

                case OpCodes.Ldelem_U4:
                    signExtend = false;
                    elementType = typeof(uint);
                    elemTypeInfo = conversionState.TheILLibrary.GetTypeInfo(elementType);
                    sizeToPush = 4;
                    sizeToLoad = 4;
                    break;

                case OpCodes.Ldelem_I4:
                    elementType = typeof(int);
                    elemTypeInfo = conversionState.TheILLibrary.GetTypeInfo(elementType);
                    sizeToPush = 4;
                    sizeToLoad = 4;
                    break;

                case OpCodes.Ldelem_I8:
                    sizeToPush = 8;
                    sizeToLoad = 8;
                    elementType = typeof(long);
                    elemTypeInfo = conversionState.TheILLibrary.GetTypeInfo(elementType);
                    break;
            }

            if (isFloat)
            {
                //TODO: Support floats
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
            // 0. Index of element to get as Int32 (dword)
            // 1. Array object reference as address (dword)

            TypeInfo arrayTypeInfo = conversionState.GetArrayTypeInfo();

            // 1. Check array reference is not null
            //      1.1. Move array ref into EAX
            //      1.2. Compare EAX (array ref) to 0
            //      1.3. If not zero, jump to continue execution further down
            //      1.4. Otherwise, call Exceptions.ThrowNullReferenceException

            //      1.1. Move array ref into EAX
            conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "[ESP+4]", Dest = "EAX"});
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
            int elemTypeOffset = conversionState.TheILLibrary.GetFieldInfo(arrayTypeInfo, "elemType").OffsetInBytes;

            //if (elementType != null)
            //{
            //    result.AppendLine(string.Format("mov EAX, {0}", conversionState.GetTypeIdString(conversionState.GetTypeID(elementType))));
            //    //      2.2. Move element type ref from array object into EBX
            //    //              - Calculate the offset of the field from the start of the array object
            //    //              - Move array ref into EBX
            //GlobalMethods.CheckAddrFromRegister(result, conversionState, "ESP", 4);
            //    result.AppendLine("mov EBX, [ESP+4]");
            //    //              - Move elemType ref ([EBX+offset]) into EBX
            //    GlobalMethods.CheckAddrFromRegister(result, conversionState, "EBX", elemTypeOffset);
            //    result.AppendLine(string.Format("mov EBX, [EBX+{0}]", elemTypeOffset));
            //    //      2.3. Compare EAX to EBX
            //    result.AppendLine("cmp EAX, EBX");
            //    //      2.4. If the same, jump to continue execution further down
            //    result.AppendLine("je " + ContinueExecutionLabel2);
            //    //      2.5. Otherwise, call Exceptions.ThrowArrayTypeMismatchException
            //    result.AppendLine(string.Format("call {0}", conversionState.GetMethodID(conversionState.ThrowArrayTypeMismatchExceptionMethod)));
            //    result.AppendLine(ContinueExecutionLabel2 + ":");
            //}

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
            conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "[ESP]", Dest = "EAX"});
            //      3.2. Move array length into ECX
            //              - Calculate the offset of the field from the start of the array object
            int lengthOffset = conversionState.TheILLibrary.GetFieldInfo(arrayTypeInfo, "length").OffsetInBytes;

            //              - Move array ref into EBX
            conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "[ESP+4]", Dest = "EBX"});
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
            //      4.1. Pop index into EBX
            //      4.2. Move element size into EAX
            //      4.3. Mulitply EAX by EBX (index by element size)
            //      4.4. Move array ref into EBX
            //      4.5. Add enough to go past Kernel.Framework.Array fields
            //      4.6. Add EAX and EBX (array ref + fields + (index * element size))

            //      4.1. Pop index into EBX
            conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EBX"});
            //      4.2. Move element size into EAX
            conversionState.Append(new Mov {Size = OperandSize.Dword, Src = sizeToLoad.ToString(), Dest = "EAX"});
            //      4.3. Mulitply EAX by EBX (index by element size)
            conversionState.Append(new ASMOps.Mul {Arg = "EBX"});
            //      4.4. Pop array ref into EBX
            conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EBX"});
            //      4.5. Add enough to go past Kernel.Framework.Array fields
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
            conversionState.Append(new ASMOps.Add {Src = allFieldsOffset.ToString(), Dest = "EBX"});
            //      4.6. Add EAX and EBX (array ref + fields + (index * element size))
            conversionState.Append(new ASMOps.Add {Src = "EBX", Dest = "EAX"});

            // 5. Push the element onto the stack
            //      5.1. Push value at [EAX] (except for LdElemA op in which case just push address)
            if (pushValue)
            {
                switch (sizeToLoad)
                {
                    case 1:
                        conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "0", Dest = "EBX"});
                        conversionState.Append(new Mov {Size = OperandSize.Byte, Src = "[EAX]", Dest = "BL"});
                        if (signExtend)
                        {
                            throw new NotSupportedException("Sign extend byte to 4 bytes in LdElem not supported!");
                        }
                        conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EBX"});
                        break;
                    case 2:
                        conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "0", Dest = "EBX"});
                        conversionState.Append(new Mov {Size = OperandSize.Word, Src = "[EAX]", Dest = "BX"});
                        if (signExtend)
                        {
                            conversionState.Append(new Cwde());
                        }
                        conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EBX"});
                        break;
                    default:
                        int additionalSpace = sizeToPush - sizeToLoad;
                        int overhangBytes = (4 - additionalSpace)%4;
                        if (additionalSpace > 0)
                        {
                            // Note: The difference will always be < 4 because the only reason the two would be different is a value type
                            //          (i.e. struct) that has had stack size padded to a multiple of 4.
                            switch (overhangBytes)
                            {
                                case 1:
                                    conversionState.Append(new Push {Size = OperandSize.Word, Src = "0"});
                                    conversionState.Append(new ASMOps.Xor {Src = "EBX", Dest = "EBX"});
                                    conversionState.Append(new Mov
                                    {
                                        Size = OperandSize.Byte,
                                        Src = "[EAX+" + (sizeToLoad - 1) + "]",
                                        Dest = "BL"
                                    });
                                    conversionState.Append(new Push {Size = OperandSize.Word, Src = "BX"});
                                    break;
                                case 2:
                                    conversionState.Append(new Push {Size = OperandSize.Word, Src = "0"});
                                    conversionState.Append(new Mov
                                    {
                                        Size = OperandSize.Word,
                                        Src = "[EAX+" + (sizeToLoad - 2) + "]",
                                        Dest = "BX"
                                    });
                                    conversionState.Append(new Push {Size = OperandSize.Word, Src = "BX"});
                                    break;
                                case 3:
                                    conversionState.Append(new ASMOps.Xor {Src = "EBX", Dest = "EBX"});
                                    conversionState.Append(new Mov
                                    {
                                        Size = OperandSize.Byte,
                                        Src = "[EAX+" + (sizeToLoad - 1) + "]",
                                        Dest = "BL"
                                    });
                                    conversionState.Append(new Push {Size = OperandSize.Word, Src = "BX"});
                                    conversionState.Append(new Mov
                                    {
                                        Size = OperandSize.Word,
                                        Src = "[EAX+" + (sizeToLoad - 3) + "]",
                                        Dest = "BX"
                                    });
                                    conversionState.Append(new Push {Size = OperandSize.Word, Src = "BX"});
                                    break;
                            }
                        }


                        for (int i = sizeToLoad - overhangBytes - 4; i >= 0; i -= 4)
                        {
                            conversionState.Append(new Mov
                            {
                                Size = OperandSize.Dword,
                                Src = "[EAX+" + i + "]",
                                Dest = "EBX"
                            });
                            conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EBX"});
                        }
                        break;
                }
            }
            else
            {
                conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EAX"});
            }

            //      5.2. Pop index and array ref from our stack
            conversionState.CurrentStackFrame.GetStack(theOp).Pop();
            conversionState.CurrentStackFrame.GetStack(theOp).Pop();
            //      5.3. Push element onto our stack
            conversionState.CurrentStackFrame.GetStack(theOp).Push(new StackItem
            {
                sizeOnStackInBytes = sizeToPush,
                isFloat = isFloat,
                isNewGCObject = false,
                isGCManaged = pushValue ? elementType == null || elemTypeInfo.IsGCManaged : false,
                isValue = pushValue ? elementType != null && elemTypeInfo.IsValueType : false
            });
        }
    }
}