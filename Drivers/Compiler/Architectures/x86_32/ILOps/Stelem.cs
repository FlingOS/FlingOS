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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.x86
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Stelem : IL.ILOps.Stelem
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            conversionState.CurrentStackFrame.Stack.Pop();
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
        /// Thrown if constant is a floating point number.
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
                    //TODO - Add more StElem op variants support
                    throw new NotSupportedException("Stelem op variant not supported yet!");

                case OpCodes.Stelem_I1:
                    sizeToPop = 1;
                    elementType = typeof(sbyte);
                    break;
                case OpCodes.Stelem_I2:
                    sizeToPop = 2;
                    elementType = typeof(Int16);
                    break;

                case OpCodes.Stelem_Ref:
                    elementType = null;
                    break;

                case OpCodes.Stelem_I4:
                    elementType = typeof(Int32);
                    break;

                case OpCodes.Stelem_I8:
                    sizeToPop = 8;
                    elementType = typeof(Int64);
                    break;
            }

            if (isFloat)
            {
                //TODO - Support floats
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

            Types.TypeInfo arrayTypeInfo = conversionState.GetArrayTypeInfo();

            // 1. Check array reference is not null
            //      1.1. Move array ref into EAX
            //      1.2. Compare EAX (array ref) to 0
            //      1.3. If not zero, jump to continue execution further down
            //      1.4. Otherwise, call Exceptions.ThrowNullReferenceException

            //      1.1. Move array ref into EAX
            conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Src = "[ESP+" + (sizeToPop == 8 ? 12 : 8).ToString() + "]", Dest = "EAX" });
            //      1.2. Compare EAX (array ref) to 0
            conversionState.Append(new ASMOps.Cmp() { Arg1 = "EAX", Arg2 = "0" });
            //      1.3. If not zero, jump to continue execution further down
            conversionState.Append(new ASMOps.Jmp() { JumpType = ASMOps.JmpOp.JumpNotZero, DestILPosition = currOpPosition, Extension = "Continue1" });
            //      1.4. Otherwise, call Exceptions.ThrowNullReferenceException
            conversionState.Append(new ASMOps.Call() { Target = "GetEIP" });
            conversionState.AddExternalLabel("GetEIP");
            conversionState.Append(new ASMOps.Call() { Target = conversionState.GetThrowNullReferenceExceptionMethodInfo().ID });
            conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "Continue1" });

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
            conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Src = "[ESP+" + (sizeToPop == 8 ? 8 : 4).ToString() + "]", Dest = "EAX" });
            //      3.2. Move array length into ECX
            //              - Calculate the offset of the field from the start of the array object
            int lengthOffset = conversionState.TheILLibrary.GetFieldInfo(arrayTypeInfo, "length").OffsetInBytes;
            //              - Move array ref into EBX
            conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Src = "[ESP+" + (sizeToPop == 8 ? 12 : 8).ToString() + "]", Dest = "EBX" });
            //              - Move length value ([EBX+offset]) into EBX
            conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Src = "[EBX+" + lengthOffset.ToString() + "]", Dest = "EBX" });
            //      3.2. Compare EAX to 0
            conversionState.Append(new ASMOps.Cmp() { Arg1 = "EAX", Arg2 = "0" });
            //      3.3. Jump if greater than to next test condition (3.5)
            conversionState.Append(new ASMOps.Jmp() { JumpType = ASMOps.JmpOp.JumpGreaterThanEqual, DestILPosition = currOpPosition, Extension = "Continue3_1" });
            //      3.4. Otherwise, call Exceptions.ThrowIndexOutOfRangeException
            conversionState.Append(new ASMOps.Call() { Target = conversionState.GetThrowIndexOutOfRangeExceptionMethodInfo().ID });
            conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "Continue3_1" });
            //      3.5. Compare EAX to EBX
            conversionState.Append(new ASMOps.Cmp() { Arg1 = "EAX", Arg2 = "EBX" });
            //      3.6. Jump if less than to continue execution further down
            conversionState.Append(new ASMOps.Jmp() { JumpType = ASMOps.JmpOp.JumpLessThan, DestILPosition = currOpPosition, Extension = "Continue3_2" });
            //      3.7. Otherwise, call Exceptions.ThrowIndexOutOfRangeException
            conversionState.Append(new ASMOps.Call() { Target = conversionState.GetThrowIndexOutOfRangeExceptionMethodInfo().ID });
            conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "Continue3_2" });
            
            // 4. Calculate address of element
            //      4.0. Pop value into ECX:EBX
            //      4.1. Pop index into EDX
            //      4.2. Pop array ref into EAX
            //      4.3. Move element type ref (from array ref) into EAX
            //      4.4. Push EAX
            //      4.5. Move IsValueType (from element ref type) into EAX
            //      4.6. If IsValueType, continue to 4.6., else goto 4.9.
            //      4.7. Pop EAX
            //      4.8. Move Size (from element type ref) into EAX
            //      4.9. Skip over 4.9. and 4.10.
            //      4.10. Pop EAX
            //      4.11. Move StackSize (from element type ref) into EAX
            //      4.12. Mulitply EAX by EDX (index by element size)
            //      4.13. Move array ref into EDX
            //      4.14. Add enough to go past Kernel.FOS_System.Array fields
            //      4.15. Add EAX and EBX (array ref + fields + (index * element size))

            //      4.0. Pop value into ECX:EBX
            conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Dword, Dest = "ECX" });
            if (sizeToPop == 8)
            {
                conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Dword, Dest = "EBX" });
            }
            //      4.1. Pop index into EDX
            conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Dword, Dest = "EDX" });
            //      4.2. Move array ref into EAX
            conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Src = "[ESP]", Dest = "EAX" });
            //      4.3. Move element type ref (from array ref) into EAX
            conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Src = "[EAX+" + elemTypeOffset.ToString() + "]", Dest = "EAX" });
            //      4.4. Push EAX
            conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Dword, Src = "EAX" });
            //      4.5. Move IsValueType (from element ref type) into EAX
            int isValueTypeOffset = conversionState.GetTypeFieldOffset("IsValueType");
            conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "[EAX+" + isValueTypeOffset.ToString() + "]", Dest = "AL" });
            //      4.6. If IsValueType, continue to 4.7., else goto 4.9.
            conversionState.Append(new ASMOps.Test() { Arg1 = "EAX", Arg2 = "1" });
            conversionState.Append(new ASMOps.Jmp() { JumpType = ASMOps.JmpOp.JumpZero, DestILPosition = currOpPosition, Extension = "Continue4_1" });
            //      4.7. Pop EAX
            conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Dword, Dest = "EAX" });
            //      4.8. Move Size (from element type ref) into EAX
            int sizeOffset = conversionState.GetTypeFieldOffset("Size");
            conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Src = "[EAX+" + sizeOffset.ToString() + "]", Dest = "EAX" });
            //      4.9. Skip over 4.9. and 4.10.
            conversionState.Append(new ASMOps.Jmp() { JumpType = ASMOps.JmpOp.Jump, DestILPosition = currOpPosition, Extension = "Continue4_2" });
            //      4.10. Pop EAX
            conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "Continue4_1" });
            conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Dword, Dest = "EAX" });
            //      4.11. Move StackSize (from element type ref) into EAX
            int stackSizeOffset = conversionState.GetTypeFieldOffset("StackSize");
            conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Src = "[EAX+" + stackSizeOffset.ToString() + "]", Dest = "EAX" });
            //      4.12. Mulitply EAX by EDX (index by element size)
            conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "Continue4_2" });
            conversionState.Append(new ASMOps.Mul() { Arg = "EDX" });
            //      4.13. Pop array ref into EDX
            conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Dword, Dest = "EDX" });
            //      4.14. Add enough to go past Kernel.FOS_System.Array fields
            int allFieldsOffset = 0;
            #region Offset calculation
            {
                Types.FieldInfo highestOffsetFieldInfo = arrayTypeInfo.FieldInfos.Where(x => !x.IsStatic).OrderByDescending(x => x.OffsetInBytes).First();
                Types.TypeInfo fieldTypeInfo = conversionState.TheILLibrary.GetTypeInfo(highestOffsetFieldInfo.UnderlyingInfo.FieldType);
                allFieldsOffset = highestOffsetFieldInfo.OffsetInBytes + (fieldTypeInfo.IsValueType ? fieldTypeInfo.SizeOnHeapInBytes : fieldTypeInfo.SizeOnStackInBytes);
            }
            #endregion
            conversionState.Append(new ASMOps.Add() { Src = allFieldsOffset.ToString(), Dest = "EDX" });
            //      4.15. Add EAX and EDX (array ref + fields + (index * element size))
            conversionState.Append(new ASMOps.Add() { Src = "EDX", Dest = "EAX" });

            // 5. Pop the element from the stack to array
            //      5.1. Move value in EBX:ECX to [EAX]
            if (sizeToPop == 8)
            {
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Src = "ECX", Dest = "[EAX]" });
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Src = "EBX", Dest = "[EAX+4]" });
            }
            else if(sizeToPop == 4)
            {
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Src = "ECX", Dest = "[EAX]" });
            }
            else if (sizeToPop == 2)
            {
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Word, Src = "CX", Dest = "[EAX]" });
            }
            else if (sizeToPop == 1)
            {
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Byte, Src = "CL", Dest = "[EAX]" });
            }

            //      5.2. Pop index, array ref and value from our stack
            conversionState.CurrentStackFrame.Stack.Pop();
            conversionState.CurrentStackFrame.Stack.Pop();
            conversionState.CurrentStackFrame.Stack.Pop();
        }
    }
}
