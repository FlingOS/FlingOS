#region LICENSE
// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
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
using Kernel.Debug.Data;

namespace Kernel.Compiler.Architectures.x86_32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Stelem : ILOps.Stelem
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if constant is a floating point number.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            Type elementType = null;
            //bool pushValue = true;
            int sizeToPop = 4;
            bool isFloat = false;

            switch ((OpCodes)anILOpInfo.opCode.Value)
            {
                case OpCodes.Stelem:
                    {
                        //Load the metadata token used to get the type info
                        int metadataToken = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
                        //Get the type info for the element type
                        elementType = aScannerState.CurrentILChunk.Method.Module.ResolveType(metadataToken);
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

            string ContinueExecutionLabelBase = string.Format("{0}.IL_{1}_Store_ContinueExecution",
                    aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                    anILOpInfo.Position);
            DB_Type arrayDBType = DebugDatabase.GetType(aScannerState.GetTypeID(aScannerState.ArrayClass));

            // 1. Check array reference is not null
            //      1.1. Move array ref into eax
            //      1.2. Compare eax (array ref) to 0
            //      1.3. If not zero, jump to continue execution further down
            //      1.4. Otherwise, call Exceptions.ThrowNullReferenceException

            string ContinueExecutionLabel1 = ContinueExecutionLabelBase + "1";
            //      1.1. Move array ref into eax
            GlobalMethods.InsertPageFaultDetection(result, aScannerState, "esp", sizeToPop == 8 ? 12 : 8, (OpCodes)anILOpInfo.opCode.Value);
            result.AppendLine(string.Format("mov eax, [esp+{0}]", sizeToPop == 8 ? 12 : 8));
            //      1.2. Compare eax (array ref) to 0
            result.AppendLine("cmp eax, 0");
            //      1.3. If not zero, jump to continue execution further down
            result.AppendLine("jnz " + ContinueExecutionLabel1);
            //      1.4. Otherwise, call Exceptions.ThrowNullReferenceException
            result.AppendLine(string.Format("call {0}", aScannerState.GetMethodID(aScannerState.ThrowNullReferenceExceptionMethod)));
            result.AppendLine(ContinueExecutionLabel1 + ":");

            // 2. Check array element type is correct
            //      2.1. Move element type ref into eax
            //      2.2. Move element type ref from array object into ebx
            //      2.3. Compare eax to ebx
            //      2.4. If the same, jump to continue execution further down
            //      2.5. Otherwise, call Exceptions.ThrowArrayTypeMismatchException

            //string ContinueExecutionLabel2 = ContinueExecutionLabelBase + "2";
            ////      2.1. Move element type ref into eax
            //if (elementType != null)
            //{
            //    result.AppendLine(string.Format("mov eax, {0}", aScannerState.GetTypeIdString(aScannerState.GetTypeID(elementType))));
            //}
            //else
            //{
            //    //Should be the same for all classes since they are (indirectly) derived from ObjectWithType
            //    int typeOffset = aScannerState.GetFieldOffset(arrayDBType, "_Type");

            //    //      - Move value (which is a ref) into eax
            //    GlobalMethods.CheckAddrFromRegister(result, aScannerState, "esp", 0);
            //    result.AppendLine("mov eax, [esp]");
            //    //      - Move value type ref (from value (ref)) into eax
            //    GlobalMethods.CheckAddrFromRegister(result, aScannerState, "eax", typeOffset);
            //    result.AppendLine(string.Format("mov eax, [eax+{0}]", typeOffset));
            //}
            ////      2.2. Move element type ref from array object into ebx
            ////              - Move array ref into ebx
            //GlobalMethods.CheckAddrFromRegister(result, aScannerState, "esp", sizeToPop == 8 ? 12 : 8);
            //result.AppendLine(string.Format("mov ebx, [esp+{0}]", sizeToPop == 8 ? 12 : 8));
            ////              - Move elemType ref ([ebx+offset]) into ebx
            int elemTypeOffset = aScannerState.GetFieldOffset(arrayDBType, "elemType");
            //GlobalMethods.CheckAddrFromRegister(result, aScannerState, "ebx", elemTypeOffset);
            //result.AppendLine(string.Format("mov ebx, [ebx+{0}]", elemTypeOffset));
            ////      2.3. Compare eax to ebx
            //result.AppendLine("cmp eax, ebx");
            ////      2.4. If the same, jump to continue execution further down
            //result.AppendLine("je " + ContinueExecutionLabel2);
            ////      2.5. Otherwise, call Exceptions.ThrowArrayTypeMismatchException
            //result.AppendLine(string.Format("call {0}", aScannerState.GetMethodID(aScannerState.ThrowArrayTypeMismatchExceptionMethod)));
            //result.AppendLine(ContinueExecutionLabel2 + ":");

            // 3. Check index to get is > -1 and < array length
            //      3.1. Move index into eax
            //      3.2. Move array length into ebx
            //      3.2. Compare eax to 0
            //      3.3. Jump if greater than to next test condition (3.5)
            //      3.4. Otherwise, call Exceptions.ThrowIndexOutOfRangeException
            //      3.5. Compare eax to ebx
            //      3.6. Jump if less than to continue execution further down
            //      3.7. Otherwise, call Exceptions.ThrowIndexOutOfRangeException

            string ContinueExecutionLabel3_1 = ContinueExecutionLabelBase + "3_1";
            string ContinueExecutionLabel3_2 = ContinueExecutionLabelBase + "3_2";
            //      3.1. Move index into eax
            GlobalMethods.InsertPageFaultDetection(result, aScannerState, "esp", sizeToPop == 8 ? 8 : 4, (OpCodes)anILOpInfo.opCode.Value);
            result.AppendLine(string.Format("mov eax, [esp+{0}]", sizeToPop == 8 ? 8 : 4));
            //      3.2. Move array length into ecx
            //              - Calculate the offset of the field from the start of the array object
            int lengthOffset = aScannerState.GetFieldOffset(arrayDBType, "length");
            //              - Move array ref into ebx
            GlobalMethods.InsertPageFaultDetection(result, aScannerState, "esp", sizeToPop == 8 ? 12 : 8, (OpCodes)anILOpInfo.opCode.Value);
            result.AppendLine(string.Format("mov ebx, [esp+{0}]", sizeToPop == 8 ? 12 : 8));
            //              - Move length value ([ebx+offset]) into ebx
            GlobalMethods.InsertPageFaultDetection(result, aScannerState, "ebx", lengthOffset, (OpCodes)anILOpInfo.opCode.Value);
            result.AppendLine(string.Format("mov ebx, [ebx+{0}]", lengthOffset));
            //      3.2. Compare eax to 0
            result.AppendLine("cmp eax, 0");
            //      3.3. Jump if greater than to next test condition (3.5)
            result.AppendLine("jge " + ContinueExecutionLabel3_1);
            //      3.4. Otherwise, call Exceptions.ThrowIndexOutOfRangeException
            result.AppendLine(string.Format("call {0}", aScannerState.GetMethodID(aScannerState.ThrowIndexOutOfRangeExceptionMethod)));
            result.AppendLine(ContinueExecutionLabel3_1 + ":");
            //      3.5. Compare eax to ebx
            result.AppendLine("cmp eax, ebx");
            //      3.6. Jump if less than to continue execution further down
            result.AppendLine("jl " + ContinueExecutionLabel3_2);
            //      3.7. Otherwise, call Exceptions.ThrowIndexOutOfRangeException
            result.AppendLine(string.Format("call {0}", aScannerState.GetMethodID(aScannerState.ThrowIndexOutOfRangeExceptionMethod)));
            result.AppendLine(ContinueExecutionLabel3_2 + ":");

            // 4. Calculate address of element
            //      4.0. Pop value into ecx:edx
            //      4.1. Pop index into ebx
            //      4.2. Pop array ref into eax
            //      4.3. Move element type ref (from array ref) into eax
            //      4.4. Push eax
            //      4.5. Move IsValueType (from element ref type) into eax
            //      4.6. If IsValueType, continue to 4.6., else goto 4.9.
            //      4.7. Pop eax
            //      4.8. Move Size (from element type ref) into eax
            //      4.9. Skip over 4.9. and 4.10.
            //      4.10. Pop eax
            //      4.11. Move StackSize (from element type ref) into eax
            //      4.12. Mulitply eax by ebx (index by element size)
            //      4.13. Move array ref into ebx
            //      4.14. Add enough to go past Kernel.FOS_System.Array fields
            //      4.15. Add eax and ebx (array ref + fields + (index * element size))

            string ContinueExecutionLabel4_1 = ContinueExecutionLabelBase + "4_1";
            string ContinueExecutionLabel4_2 = ContinueExecutionLabelBase + "4_2";
            //      4.0. Pop value into ecx:edx
            result.AppendLine("pop ecx");
            if (sizeToPop == 8)
            {
                result.AppendLine("pop edx");
            }
            //      4.1. Pop index into ebx
            result.AppendLine("pop ebx");
            //      4.2. Move array ref into eax
            GlobalMethods.InsertPageFaultDetection(result, aScannerState, "esp", 0, (OpCodes)anILOpInfo.opCode.Value);
            result.AppendLine("mov eax, [esp]");
            //      4.3. Move element type ref (from array ref) into eax
            GlobalMethods.InsertPageFaultDetection(result, aScannerState, "eax", elemTypeOffset, (OpCodes)anILOpInfo.opCode.Value);
            result.AppendLine(string.Format("mov eax, [eax+{0}]", elemTypeOffset));
            //      4.4. Push eax
            result.AppendLine("push eax");
            //      4.5. Move IsValueType (from element ref type) into eax
            int isValueTypeOffset = aScannerState.GetTypeFieldOffset("IsValueType");
            GlobalMethods.InsertPageFaultDetection(result, aScannerState, "eax", isValueTypeOffset, (OpCodes)anILOpInfo.opCode.Value);
            result.AppendLine(string.Format("mov byte al, [eax+{0}]", isValueTypeOffset));
            // Zero-out the rest of eax
            result.AppendLine("and eax, 1");
            //      4.6. If IsValueType, continue to 4.7., else goto 4.9.
            result.AppendLine("cmp eax, 0");
            result.AppendLine("jz " + ContinueExecutionLabel4_1);
            //      4.7. Pop eax
            result.AppendLine("pop eax");
            //      4.8. Move Size (from element type ref) into eax
            int sizeOffset = aScannerState.GetTypeFieldOffset("Size");
            GlobalMethods.InsertPageFaultDetection(result, aScannerState, "eax", sizeOffset, (OpCodes)anILOpInfo.opCode.Value);
            result.AppendLine(string.Format("mov eax, [eax+{0}]", sizeOffset));
            //      4.9. Skip over 4.9. and 4.10.
            result.AppendLine("jmp " + ContinueExecutionLabel4_2);
            //      4.10. Pop eax
            result.AppendLine(ContinueExecutionLabel4_1 + ":");
            result.AppendLine("pop eax");
            //      4.11. Move StackSize (from element type ref) into eax
            int stackSizeOffset = aScannerState.GetTypeFieldOffset("StackSize");
            GlobalMethods.InsertPageFaultDetection(result, aScannerState, "eax", stackSizeOffset, (OpCodes)anILOpInfo.opCode.Value);
            result.AppendLine(string.Format("mov eax, [eax+{0}]", stackSizeOffset));
            //      4.12. Mulitply eax by ebx (index by element size)
            result.AppendLine(ContinueExecutionLabel4_2 + ":");
            result.AppendLine("mul ebx");
            //      4.13. Pop array ref into ebx
            result.AppendLine("pop ebx");
            //      4.14. Add enough to go past Kernel.FOS_System.Array fields
            int allFieldsOffset = 0;
            #region Offset calculation
            {
                //Get the child links of the type (i.e. the fields of the type)
                List<DB_ComplexTypeLink> allChildLinks = arrayDBType.ChildTypes.ToList();
                //Calculate the offset
                //We use StackBytesSize since fields that are reference types are only stored as a pointer
                allFieldsOffset = allChildLinks.Sum(x => x.ChildType.IsValueType ? x.ChildType.BytesSize : x.ChildType.StackBytesSize);
            }
            #endregion
            result.AppendLine(string.Format("add ebx, {0}", allFieldsOffset));
            //      4.15. Add eax and ebx (array ref + fields + (index * element size))
            result.AppendLine("add eax, ebx");

            // 5. Pop the element from the stack to array
            //      5.1. Move value in edx:ecx to [eax]
            if (sizeToPop == 8)
            {
                GlobalMethods.InsertPageFaultDetection(result, aScannerState, "eax", 0, (OpCodes)anILOpInfo.opCode.Value);
                result.AppendLine("mov dword [eax], ecx");
                GlobalMethods.InsertPageFaultDetection(result, aScannerState, "eax", 4, (OpCodes)anILOpInfo.opCode.Value);
                result.AppendLine("mov dword [eax+4], edx");
            }
            else if(sizeToPop == 4)
            {
                GlobalMethods.InsertPageFaultDetection(result, aScannerState, "eax", 0, (OpCodes)anILOpInfo.opCode.Value);
                result.AppendLine("mov dword [eax], ecx");
            }
            else if (sizeToPop == 2)
            {
                GlobalMethods.InsertPageFaultDetection(result, aScannerState, "eax", 0, (OpCodes)anILOpInfo.opCode.Value);
                result.AppendLine("mov word [eax], cx");
            }
            else if (sizeToPop == 1)
            {
                GlobalMethods.InsertPageFaultDetection(result, aScannerState, "eax", 0, (OpCodes)anILOpInfo.opCode.Value);
                result.AppendLine("mov byte [eax], cl");
            }

            //      5.2. Pop index, array ref and value from our stack
            aScannerState.CurrentStackFrame.Stack.Pop();
            aScannerState.CurrentStackFrame.Stack.Pop();
            aScannerState.CurrentStackFrame.Stack.Pop();


            return result.ToString().Trim();
        }
    }
}
