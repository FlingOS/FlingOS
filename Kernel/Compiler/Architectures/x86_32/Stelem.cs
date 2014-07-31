#region Copyright Notice
// ------------------------------------------------------------------------------ //
//                                                                                //
//               All contents copyright � Edward Nutting 2014                     //
//                                                                                //
//        You may not share, reuse, redistribute or otherwise use the             //
//        contents this file outside of the Fling OS project without              //
//        the express permission of Edward Nutting or other copyright             //
//        holder. Any changes (including but not limited to additions,            //
//        edits or subtractions) made to or from this document are not            //
//        your copyright. They are the copyright of the main copyright            //
//        holder for all Fling OS files. At the time of writing, this             //
//        owner was Edward Nutting. To be clear, owner(s) do not include          //
//        developers, contributors or other project members.                      //
//                                                                                //
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
            //    int typeOffset = 0;
            //    #region Offset calculation
            //    {
            //        //Get the child links of the type (i.e. the fields of the type)
            //        List<DB_ComplexTypeLink> allChildLinks = arrayDBType.ChildTypes.OrderBy(x => x.ParentIndex).ToList();
            //        //Get the DB type information for the field we want to load
            //        DB_ComplexTypeLink theTypeLink = (from links in arrayDBType.ChildTypes
            //                                          where links.FieldId == "_Type"
            //                                          select links).First();
            //        //Get all the fields that come before the field we want to load
            //        //This is so we can calculate the offset (in memory, in bytes) from the start of the object
            //        allChildLinks = allChildLinks.Where(x => x.ParentIndex < theTypeLink.ParentIndex).ToList();
            //        //Calculate the offset
            //        //We use StackBytesSize since fields that are reference types are only stored as a pointer
            //        typeOffset = allChildLinks.Sum(x => x.ChildType.StackBytesSize);
            //    }
            //    #endregion
            
            //    //      - Move value (which is a ref) into eax
            //    result.AppendLine("mov eax, [esp]");
            //    //      - Move value type ref (from value (ref)) into eax
            //    result.AppendLine(string.Format("mov eax, [eax+{0}]", typeOffset));
            //}
            ////      2.2. Move element type ref from array object into ebx
            ////              - Move array ref into ebx
            //result.AppendLine(string.Format("mov ebx, [esp+{0}]", sizeToPop == 8 ? 12 : 8));
            ////              - Move elemType ref ([ebx+offset]) into ebx
            int elemTypeOffset = 0;
            #region Offset calculation
            {
                //Get the child links of the type (i.e. the fields of the type)
                List<DB_ComplexTypeLink> allChildLinks = arrayDBType.ChildTypes.OrderBy(x => x.ParentIndex).ToList();
                //Get the DB type information for the field we want to load
                DB_ComplexTypeLink theTypeLink = (from links in arrayDBType.ChildTypes
                                                  where links.FieldId == "elemType"
                                                  select links).First();
                //Get all the fields that come before the field we want to load
                //This is so we can calculate the offset (in memory, in bytes) from the start of the object
                allChildLinks = allChildLinks.Where(x => x.ParentIndex < theTypeLink.ParentIndex).ToList();
                //Calculate the offset
                //We use StackBytesSize since fields that are reference types are only stored as a pointer
                elemTypeOffset = allChildLinks.Sum(x => x.ChildType.IsValueType ? x.ChildType.BytesSize : x.ChildType.StackBytesSize);
            }
            #endregion
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
            result.AppendLine(string.Format("mov eax, [esp+{0}]", sizeToPop == 8 ? 8 : 4));
            //      3.2. Move array length into ecx
            //              - Calculate the offset of the field from the start of the array object
            int lengthOffset = 0;
            #region Offset calculation
            {
                //Get the child links of the type (i.e. the fields of the type)
                List<DB_ComplexTypeLink> allChildLinks = arrayDBType.ChildTypes.OrderBy(x => x.ParentIndex).ToList();
                //Get the DB type information for the field we want to load
                DB_ComplexTypeLink theTypeLink = (from links in arrayDBType.ChildTypes
                                                  where links.FieldId == "length"
                                                  select links).First();
                //Get all the fields that come before the field we want to load
                //This is so we can calculate the offset (in memory, in bytes) from the start of the object
                allChildLinks = allChildLinks.Where(x => x.ParentIndex < theTypeLink.ParentIndex).ToList();
                //Calculate the offset
                //We use StackBytesSize since fields that are reference types are only stored as a pointer
                lengthOffset = allChildLinks.Sum(x => x.ChildType.IsValueType ? x.ChildType.BytesSize : x.ChildType.StackBytesSize);
            }
            #endregion
            //              - Move array ref into ebx
            result.AppendLine(string.Format("mov ebx, [esp+{0}]", sizeToPop == 8 ? 12 : 8));
            //              - Move length value ([ebx+offset]) into ebx
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
            result.AppendLine("mov eax, [esp]");
            //      4.3. Move element type ref (from array ref) into eax
            result.AppendLine(string.Format("mov eax, [eax+{0}]", elemTypeOffset));
            //      4.4. Push eax
            result.AppendLine("push eax");
            //      4.5. Move IsValueType (from element ref type) into eax
            int isValueTypeOffset = 0;
            #region Offset calculation
            {
                DB_Type typeDBType = DebugDatabase.GetType(aScannerState.GetTypeID(aScannerState.TypeClass));
                //Get the child links of the type (i.e. the fields of the type)
                List<DB_ComplexTypeLink> allChildLinks = typeDBType.ChildTypes.OrderBy(x => x.ParentIndex).ToList();
                //Get the DB type information for the field we want to load
                DB_ComplexTypeLink theTypeLink = (from links in typeDBType.ChildTypes
                                                  where links.FieldId == "IsValueType"
                                                  select links).First();
                //Get all the fields that come before the field we want to load
                //This is so we can calculate the offset (in memory, in bytes) from the start of the object
                allChildLinks = allChildLinks.Where(x => x.ParentIndex < theTypeLink.ParentIndex).ToList();
                //Calculate the offset
                //We use StackBytesSize since fields that are reference types are only stored as a pointer
                isValueTypeOffset = allChildLinks.Sum(x => x.ChildType.IsValueType ? x.ChildType.BytesSize : x.ChildType.StackBytesSize);
            }
            #endregion
            result.AppendLine(string.Format("mov eax, [eax+{0}]", isValueTypeOffset));
            //      4.6. If IsValueType, continue to 4.7., else goto 4.9.
            result.AppendLine("cmp eax, 0");
            result.AppendLine("jz " + ContinueExecutionLabel4_1);
            //      4.7. Pop eax
            result.AppendLine("pop eax");
            //      4.8. Move Size (from element type ref) into eax
            int sizeOffset = 0;
            #region Offset calculation
            {
                DB_Type typeDBType = DebugDatabase.GetType(aScannerState.GetTypeID(aScannerState.TypeClass));
                //Get the child links of the type (i.e. the fields of the type)
                List<DB_ComplexTypeLink> allChildLinks = typeDBType.ChildTypes.OrderBy(x => x.ParentIndex).ToList();
                //Get the DB type information for the field we want to load
                DB_ComplexTypeLink theTypeLink = (from links in typeDBType.ChildTypes
                                                  where links.FieldId == "Size"
                                                  select links).First();
                //Get all the fields that come before the field we want to load
                //This is so we can calculate the offset (in memory, in bytes) from the start of the object
                allChildLinks = allChildLinks.Where(x => x.ParentIndex < theTypeLink.ParentIndex).ToList();
                //Calculate the offset
                //We use StackBytesSize since fields that are reference types are only stored as a pointer
                sizeOffset = allChildLinks.Sum(x => x.ChildType.IsValueType ? x.ChildType.BytesSize : x.ChildType.StackBytesSize);
            }
            #endregion
            result.AppendLine(string.Format("mov eax, [eax+{0}]", sizeOffset));
            //      4.9. Skip over 4.9. and 4.10.
            result.AppendLine("jmp " + ContinueExecutionLabel4_2);
            //      4.10. Pop eax
            result.AppendLine(ContinueExecutionLabel4_1 + ":");
            result.AppendLine("pop eax");
            //      4.11. Move StackSize (from element type ref) into eax
            int stackSizeOffset = 0;
            #region Offset calculation
            {
                DB_Type typeDBType = DebugDatabase.GetType(aScannerState.GetTypeID(aScannerState.TypeClass));
                //Get the child links of the type (i.e. the fields of the type)
                List<DB_ComplexTypeLink> allChildLinks = typeDBType.ChildTypes.OrderBy(x => x.ParentIndex).ToList();
                //Get the DB type information for the field we want to load
                DB_ComplexTypeLink theTypeLink = (from links in typeDBType.ChildTypes
                                                  where links.FieldId == "StackSize"
                                                  select links).First();
                //Get all the fields that come before the field we want to load
                //This is so we can calculate the offset (in memory, in bytes) from the start of the object
                allChildLinks = allChildLinks.Where(x => x.ParentIndex < theTypeLink.ParentIndex).ToList();
                //Calculate the offset
                //We use StackBytesSize since fields that are reference types are only stored as a pointer
                stackSizeOffset = allChildLinks.Sum(x => x.ChildType.IsValueType ? x.ChildType.BytesSize : x.ChildType.StackBytesSize);
            }
            #endregion
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
                result.AppendLine("mov dword [eax], ecx");
                result.AppendLine("mov dword [eax+4], edx");
            }
            else if(sizeToPop == 4)
            {
                result.AppendLine("mov dword [eax], ecx");
            }
            else if (sizeToPop == 2)
            {
                result.AppendLine("mov word [eax], cx");
            }
            else if (sizeToPop == 1)
            {
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
