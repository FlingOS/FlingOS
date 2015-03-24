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
using System.Reflection;
using Kernel.Debug.Data;

namespace Kernel.Compiler.Architectures.x86_32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Ldlen : ILOps.Ldlen
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if field to load is a floating value or the field to load
        /// is not of size 4 or 8 bytes.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            aScannerState.CurrentStackFrame.Stack.Pop();

            DB_Type arrayDBType = DebugDatabase.GetType(aScannerState.GetTypeID(aScannerState.ArrayClass));
            int lengthOffset = aScannerState.GetFieldOffset(arrayDBType, "length");

            // 1. Check array reference is not null
            //      1.1. Move array ref into eax
            //      1.2. Compare eax (array ref) to 0
            //      1.3. If not zero, jump to continue execution further down
            //      1.4. Otherwise, call Exceptions.ThrowNullReferenceException
            // 2. Load array length

            string ContinueExecutionLabelBase = string.Format("{0}.IL_{1}_ContinueExecution",
                    aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                    anILOpInfo.Position);
            string ContinueExecutionLabel1 = ContinueExecutionLabelBase + "1";
            //      1.1. Move array ref into eax
            GlobalMethods.InsertPageFaultDetection(result, aScannerState, "esp", 0, (OpCodes)anILOpInfo.opCode.Value);
            result.AppendLine("mov eax, [esp]");
            //      1.2. Compare eax (array ref) to 0
            result.AppendLine("cmp eax, 0");
            //      1.3. If not zero, jump to continue execution further down
            result.AppendLine("jnz " + ContinueExecutionLabel1);
            //      1.4. Otherwise, call Exceptions.ThrowNullReferenceException
            result.AppendLine(string.Format("call {0}", aScannerState.GetMethodID(aScannerState.ThrowNullReferenceExceptionMethod)));
            result.AppendLine(ContinueExecutionLabel1 + ":");

            //2. Load array length
            //  - Pop array ref
            result.AppendLine("pop dword ecx");
            //  - Load length from array ref
            GlobalMethods.InsertPageFaultDetection(result, aScannerState, "ecx", lengthOffset, (OpCodes)anILOpInfo.opCode.Value);
            result.AppendLine(string.Format("mov eax, [ecx+{0}]", lengthOffset));
            //  - Push array length
            result.AppendLine("push dword eax");

            aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                isFloat = false,
                sizeOnStackInBytes = 4,
                isGCManaged = false
            });

            return result.ToString().Trim();
        }
    }
}
