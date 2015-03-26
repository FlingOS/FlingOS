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

namespace Kernel.Compiler.Architectures.x86_32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Newarr : ILOps.Newarr
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

            //Load the metadata token used to get the type info
            int metadataToken = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
            //Get the type info for the element type
            Type elementType = aScannerState.CurrentILChunk.Method.Module.ResolveType(metadataToken);
            
            //New array must:
            // - Allocate memory on the heap for the object
            //          - If no memory is left, throw a panic attack because we're out of memory...
            // - Call the specified constructor

            //The label to jump to if allocated memory isn't null
            //i.e. not out of memory.
            string NotNullLabel = string.Format("{0}.IL_{1}_NotNullMem",
                    aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                    anILOpInfo.Position);
            

            //Attempt to allocate memory on the heap for the new array
            //This involves:
            // - (Number of elements is already on the stack)
            // - Pushing the element type reference onto the stack
            // - Calling GC NewArr method
            // - Check the pointer == 0, if so, out of memory

            //Push type reference
            string typeIdStr = aScannerState.GetTypeIdString(aScannerState.GetTypeID(elementType));
            result.AppendLine(string.Format("push dword {0}", typeIdStr));
            //Push a dword for return value (i.e. new array pointer)
            result.AppendLine("push dword 0");
            //Get the GC.NewArr method ID (i.e. ASM label)
            string methodLabel = aScannerState.GetMethodID(aScannerState.NewArrMethod);
            //Call GC.NewArr
            result.AppendLine(string.Format("call {0}", methodLabel));
            //Pop the return value (i.e. new array pointer)
            result.AppendLine("pop dword eax");
            //Remove args from stack
            result.AppendLine("add esp, 8");
            //Check if pointer == 0?
            result.AppendLine("cmp eax, 0");
            //If it isn't 0, not out of memory so continue execution
            result.AppendLine(string.Format("jnz {0}", NotNullLabel));
            //If we are out of memory, we have a massive problem
            //Because it means we don't have space to create a new exception object
            //So ultimately we just have to throw a kernel panic
            //Throw a panic attack... ( :/ ) by calling kernel Halt(uint lastAddress)
            result.AppendLine("call GetEIP");
            result.AppendLine("push dword esp");
            result.AppendLine("push dword ebp");
            result.AppendLine("pushad");
            result.AppendLine("mov dword eax, 0xDEADBEEF");
            result.AppendLine("mov dword ebx, 0x2");
            result.AppendLine("mov dword ecx, 1");
            result.AppendLine("jmp method_System_Void_RETEND_Kernel_PreReqs_DECLEND_PageFaultDetection_NAMEEND___FAIL");

            result.AppendLine("call GetEIP");
            result.AppendLine(string.Format("call {0}", aScannerState.GetMethodID(aScannerState.HaltMethod)));
            //Insert the not null label
            result.AppendLine(NotNullLabel + ":");

            //Push new array pointer
            result.AppendLine("push dword eax");

            aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                isFloat = false,
                sizeOnStackInBytes = 4,
                isNewGCObject = true,
                isGCManaged = true
            });

            return result.ToString().Trim();
        }
    }
}
