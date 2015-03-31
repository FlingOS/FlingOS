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
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.x86
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Ldlen : IL.ILOps.Ldlen
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if field to load is a floating value or the field to load
        /// is not of size 4 or 8 bytes.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            conversionState.CurrentStackFrame.Stack.Pop();

            Types.TypeInfo arrayTypeInfo = conversionState.GetArrayTypeInfo();
            int lengthOffset = conversionState.TheILLibrary.GetFieldInfo(arrayTypeInfo, "length").OffsetInBytes;

            int currOpPosition = conversionState.PositionOf(theOp);

            conversionState.AddExternalLabel(conversionState.GetThrowNullReferenceExceptionMethodInfo().ID);
            
            // 1. Check array reference is not null
            //      1.1. Move array ref into eax
            //      1.2. Compare eax (array ref) to 0
            //      1.3. If not zero, jump to continue execution further down
            //      1.4. Otherwise, call Exceptions.ThrowNullReferenceException
            // 2. Load array length

            //string ContinueExecutionLabelBase = string.Format("{0}.IL_{1}_ContinueExecution",
            //        conversionState.GetMethodID(conversionState.CurrentILChunk.Method),
            //        theOp.Position);
            //string ContinueExecutionLabel1 = ContinueExecutionLabelBase + "1";

            //      1.1. Move array ref into eax
            GlobalMethods.InsertPageFaultDetection(conversionState, "esp", 0, (OpCodes)theOp.opCode.Value);
            conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Src = "[ESP]", Dest = "EAX" });
            //      1.2. Compare eax (array ref) to 0
            conversionState.Append(new ASMOps.Cmp() { Arg1 = "EAX", Arg2 = "0" });
            //      1.3. If not zero, jump to continue execution further down
            conversionState.Append(new ASMOps.Jmp() { JumpType = ASMOps.JmpOp.JumpNotZero, DestILPosition = currOpPosition, Extension = "ContinueExecution1" });
            //      1.4. Otherwise, call Exceptions.ThrowNullReferenceException
            conversionState.Append(new ASMOps.Call() { Target = conversionState.GetThrowNullReferenceExceptionMethodInfo().ID });
            conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "ContinueExecution1" });

            //2. Load array length
            //  - Pop array ref
            conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Dword, Dest = "ECX" });
            //  - Load length from array ref
            GlobalMethods.InsertPageFaultDetection(conversionState, "ecx", lengthOffset, (OpCodes)theOp.opCode.Value);
            conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Src = "[ECX+" + lengthOffset.ToString() + "]", Dest = "EAX" });
            //  - Push array length
            conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Dword, Src = "EAX" });

            conversionState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                isFloat = false,
                sizeOnStackInBytes = 4,
                isGCManaged = false
            });
        }
    }
}
