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

            string Label_3 = string.Format("{0}.IL_{1}_Point3",
                                            aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                                            anILOpInfo.Position);

            string Label_False1 = string.Format("{0}.IL_{1}_False1",
                                                aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                                                anILOpInfo.Position);
            string Label_False2 = string.Format("{0}.IL_{1}_False2",
                                                aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                                                anILOpInfo.Position);

            string Label_End = string.Format("{0}.IL_{1}_End",
                                                aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                                                anILOpInfo.Position);

            // Test if the object provided inherits from the specified class
            // 1. Pop object ref
            // 1.1. Test if object ref is null:
            // 1.1.1 True: Push null and continue
            // 1.1.2 False: Go to 2
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

            // 1.1. Test if object ref is null:
            result.AppendLine("cmp eax, 0");

            result.AppendLine("jne " + Label_False1);

            // 1.1.1 True: Push null and continue
            result.AppendLine("push dword 0");
            result.AppendLine("jmp " + Label_End);

            // 1.1.2 False: Go to 2
            result.AppendLine(Label_False1 + ":");

            // 2. Load object type
            GlobalMethods.InsertPageFaultDetection(result, aScannerState, "eax", 0, (OpCodes)anILOpInfo.opCode.Value);
            result.AppendLine("mov dword ebx, [eax]");
            

            // 3. Test if object type == provided type:
            int metadataToken = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
            Type theType = aScannerState.CurrentILChunk.Method.Module.ResolveType(metadataToken);
            string TestTypeId = aScannerState.GetTypeIdString(aScannerState.GetTypeID(theType));

            result.AppendLine("mov dword ecx, " + TestTypeId);

            result.AppendLine(Label_3 + ":");
            result.AppendLine("cmp ebx, ecx");


            result.AppendLine("jne " + Label_False2);

            //      3.1 True: Push object ref and continue
            result.AppendLine("push dword eax");
            result.AppendLine("jmp " + Label_End);

            //      3.2 False: 
            result.AppendLine(Label_False2 + ":");

            //      3.2.1. Move to base type
            int baseTypeOffset = aScannerState.GetTypeFieldOffset("TheBaseType");
            GlobalMethods.InsertPageFaultDetection(result, aScannerState, "ebx", baseTypeOffset, (OpCodes)anILOpInfo.opCode.Value);
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
