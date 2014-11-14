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
using System.Reflection;
using Kernel.Debug.Data;

namespace Kernel.Compiler.Architectures.x86_32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Isinst : ILOps.Isinst
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            // Test if the object provided inherits from the specified class
            // 1. Pop object ref
            // 2. Load object type
            // 3. Test if object type == provided type:
            //      3.1 True: Push object ref and continue
            //      3.2 False: 
            //      3.2.1. Move to base type
            //      3.2.2. Test if base type null:
            //      3.2.2.1   True: Push null and continue
            //      3.2.2.2   False: Jump back to (3)

            // 1. Pop object ref
            result.AppendLine("pop dword eax");

            // 2. Load object type
            result.AppendLine("mov dword ebx, [eax]");
            
            string Label_3 = string.Format("{0}.IL_{1}_Point3",
                                            aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                                            anILOpInfo.Position);

            // 3. Test if object type == provided type:
            int metadataToken = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
            Type theType = aScannerState.CurrentILChunk.Method.Module.ResolveType(metadataToken);
            string TestTypeId = aScannerState.GetTypeIdString(aScannerState.GetTypeID(theType));

            result.AppendLine("mov dword ecx, " + TestTypeId);

            result.AppendLine(Label_3 + ":");
            result.AppendLine("cmp ebx, ecx");

            string Label_False1 = string.Format("{0}.IL_{1}_False1",
                                                aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                                                anILOpInfo.Position);
            string Label_False2 = string.Format("{0}.IL_{1}_False2",
                                                aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                                                anILOpInfo.Position);
            
            string Label_End = string.Format("{0}.IL_{1}_End",
                                                aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                                                anILOpInfo.Position);

            result.AppendLine("jne " + Label_False1);

            //      3.1 True: Push object ref and continue
            result.AppendLine("push dword eax");
            result.AppendLine("jmp " + Label_End);

            //      3.2 False: 
            result.AppendLine(Label_False1 + ":");

            //      3.2.1. Move to base type
            int baseTypeOffset = 0;
            #region Offset calculation
            {
                DB_Type typeDBType = DebugDatabase.GetType(aScannerState.GetTypeID(aScannerState.TypeClass));

                //Get the child links of the type (i.e. the fields of the type)
                List<DB_ComplexTypeLink> allChildLinks = typeDBType.ChildTypes.OrderBy(x => x.ParentIndex).ToList();
                //Get the DB type information for the field we want to load
                DB_ComplexTypeLink theTypeLink = (from links in typeDBType.ChildTypes
                                                  where links.FieldId == "TheBaseType"
                                                  select links).First();
                //Get all the fields that come before the field we want to load
                //This is so we can calculate the offset (in memory, in bytes) from the start of the object
                allChildLinks = allChildLinks.Where(x => x.ParentIndex < theTypeLink.ParentIndex).ToList();
                //Calculate the offset
                baseTypeOffset = allChildLinks.Sum(x => x.ChildType.IsValueType ? x.ChildType.BytesSize : x.ChildType.StackBytesSize);
            }
            #endregion
            result.AppendLine(string.Format("mov dword ebx, [ebx+{0}]", baseTypeOffset));

            //      3.2.2. Test if base type null:
            result.AppendLine("cmp ebx, 0");
            //      3.2.2.2   False: Jump back to (3)
            result.AppendLine("jne " + Label_3);
            
            //      3.2.2.1   True: Push null and continue
            result.AppendLine("push dword 0");
            
            result.AppendLine(Label_End + ":");

            return result.ToString().Trim();
        }
    }
}
