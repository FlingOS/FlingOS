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
    public class MethodEnd : IL.ILOps.MethodEnd
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            Type retType = (conversionState.Input.TheMethodInfo.IsConstructor ?
                typeof(void) : ((MethodInfo)conversionState.Input.TheMethodInfo.UnderlyingInfo).ReturnType);
            Types.TypeInfo retTypeInfo = conversionState.TheILLibrary.GetTypeInfo(retType);
            if (retTypeInfo.SizeOnStackInBytes != 0)
            {
                conversionState.CurrentStackFrame.Stack.Pop();
            }
        }

        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown when the return value is a float or the size on the stack
        /// in bytes is not 4 or 8 bytes.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Store the return value
            //Get the return type
            Type retType = (conversionState.Input.TheMethodInfo.IsConstructor ? 
                typeof(void) : ((MethodInfo)conversionState.Input.TheMethodInfo.UnderlyingInfo).ReturnType);
            Types.TypeInfo retTypeInfo = conversionState.TheILLibrary.GetTypeInfo(retType);
            //Get the size of the return type on stack
            int retSize = retTypeInfo.SizeOnStackInBytes;
            //If the size isn't 0 (i.e. isn't "void" which has no return value)
            if (retSize != 0)
            {
                //Pop the return value off our stack
                StackItem retItem = conversionState.CurrentStackFrame.Stack.Pop();

                //If it is float, well, we don't support it yet...
                if (retItem.isFloat)
                {
                    //SUPPORT - floats
                    throw new NotSupportedException("Floats return type not supported yet!");
                }
                //Otherwise, store the return value at [ebp+8]
                //[ebp+8] because that is last "argument"
                //      - read the calling convention spec
                else if (retSize == 4)
                {
                    conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Dword, Dest = "EAX" });
                    conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Src = "[EBP+8]", Dest = "EAX" });
                }
                else if (retSize == 8)
                {
                    conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Dword, Dest = "EAX" });
                    conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Src = "[EBP+8]", Dest = "EAX" });

                    conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Dword, Dest = "EAX" });
                    conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Src = "[EBP+12]", Dest = "EAX" });
                }
                else
                {
                    throw new NotSupportedException("Return type size not supported / invalid!");
                }
            }

            //Once return value is off the stack, remove the locals
            //Deallocate stack space for locals
            //Only bother if there are any locals
            if (conversionState.Input.TheMethodInfo.LocalInfos.Count > 0)
            {
                //Get the total size of all locals
                int totalBytes = 0;
                foreach (Types.VariableInfo aLocal in conversionState.Input.TheMethodInfo.LocalInfos)
                {
                    totalBytes += aLocal.TheTypeInfo.SizeOnStackInBytes;
                }
                //Move esp past the locals
                conversionState.Append(new ASMOps.Add() { Src = totalBytes.ToString(), Dest = "ESP" });
            }

            //Restore ebp to previous method's ebp
            conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Dword, Dest = "EBP" });
            //This pop also takes last value off the stack which
            //means top item is the return address
            //So ret command can now be correctly executed.
        }
    }
}
