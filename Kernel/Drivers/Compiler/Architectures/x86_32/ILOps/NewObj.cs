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
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.x86
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Newobj : IL.ILOps.Newobj
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            MethodBase constructorMethod = theOp.MethodToCall;
            Types.MethodInfo constructorMethodInfo = conversionState.TheILLibrary.GetMethodInfo(constructorMethod);
            Type objectType = constructorMethod.DeclaringType;

            if (typeof(Delegate).IsAssignableFrom(objectType))
            {
                StackItem funcPtrItem = conversionState.CurrentStackFrame.Stack.Pop(); ;
                conversionState.CurrentStackFrame.Stack.Pop();
                conversionState.CurrentStackFrame.Stack.Push(funcPtrItem);
                return;
            }

            ParameterInfo[] allParams = constructorMethod.GetParameters();
            foreach (ParameterInfo aParam in allParams)
            {
                conversionState.CurrentStackFrame.Stack.Pop();
            }

            conversionState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                isFloat = false,
                sizeOnStackInBytes = 4,
                isNewGCObject = true,
                isGCManaged = true
            });
        }

        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            MethodBase constructorMethod = theOp.MethodToCall;
            Types.MethodInfo constructorMethodInfo = conversionState.TheILLibrary.GetMethodInfo(constructorMethod);
            Type objectType = constructorMethod.DeclaringType;

            conversionState.AddExternalLabel(conversionState.GetHaltMethodInfo().ID);
            conversionState.AddExternalLabel(conversionState.GetNewObjMethodInfo().ID);
            conversionState.AddExternalLabel(constructorMethodInfo.ID);

            //New obj must:
            // - Ignore for creation of Delegates
            // - Allocate memory on the heap for the object
            //          - If no memory is left, throw a panic attack because we're out of memory...
            // - Call the specified constructor
            
            if (typeof(Delegate).IsAssignableFrom(objectType))
            {
                conversionState.Append(new ASMOps.Comment() { Text = "Ignore newobj calls for Delegates" });
                //Still need to: 
                // - Remove the "object" param but preserve the "function pointer"
                StackItem funcPtrItem = conversionState.CurrentStackFrame.Stack.Pop(); ;
                conversionState.CurrentStackFrame.Stack.Pop();
                conversionState.CurrentStackFrame.Stack.Push(funcPtrItem);

                GlobalMethods.InsertPageFaultDetection(conversionState, "esp", 0, (OpCodes)theOp.opCode.Value);
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Src = "[ESP]", Dest = "EAX" });
                GlobalMethods.InsertPageFaultDetection(conversionState, "esp", 4, (OpCodes)theOp.opCode.Value);
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Src = "EAX", Dest = "[ESP+4]" });
                conversionState.Append(new ASMOps.Add() { Src = "4", Dest = "ESP" });
                return;
            }
            
            int currOpPosition = conversionState.PositionOf(theOp);

            //Attempt to allocate memory on the heap for the new object
            //This involves:
            // - Pushing the type reference onto the stack
            // - Calling GC NewObj method
            // - Check the pointer == 0, if so, out of memory

            //Push type reference
            string typeIdStr = conversionState.TheILLibrary.GetTypeInfo(objectType).ID;
            conversionState.AddExternalLabel(typeIdStr);
            conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Dword, Src = typeIdStr });
            //Push a dword for return value (i.e. new object pointer)
            conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Dword, Src = "0" });
            //Get the GC.NewObj method ID (i.e. ASM label)
            string methodLabel = conversionState.GetNewObjMethodInfo().ID;
            //Call GC.NewObj
            conversionState.Append(new ASMOps.Call() { Target = methodLabel });
            //Pop the return value (i.e. new object pointer)
            conversionState.Append(new ASMOps.Pop() { Size = ASMOps.OperandSize.Dword, Dest = "EAX" });
            //Remove arg 0 from stack
            conversionState.Append(new ASMOps.Add() { Src = "4", Dest = "ESP" });
            //Check if pointer == 0?
            conversionState.Append(new ASMOps.Cmp() { Arg1 = "EAX", Arg2 = "0" });
            //If it isn't 0, not out of memory so continue execution
            conversionState.Append(new ASMOps.Jmp() { JumpType = ASMOps.JmpOp.JumpNotZero, DestILPosition = currOpPosition, Extension = "NotNullMem" });
            //If we are out of memory, we have a massive problem
            //Because it means we don't have space to create a new exception object
            //So ultimately we just have to throw a kernel panic
            //Throw a panic attack... ( :/ ) by calling kernel Halt(uint lastAddress)
            
            //result.AppendLine("call GetEIP");
            //result.AppendLine("push dword esp");
            //result.AppendLine("push dword ebp");
            //result.AppendLine("pushad");
            //result.AppendLine("mov dword eax, 0xDEADBEEF");
            //result.AppendLine("mov dword ebx, 0x1");
            //result.AppendLine("mov dword ecx, 1");
            //result.AppendLine("mov dword [staticfield_System_Boolean_Kernel_FOS_System_GC_Enabled], 1");
            //result.AppendLine("mov dword [staticfield_System_Boolean_Kernel_FOS_System_Heap_PreventAllocation], 0");
            //result.AppendLine("jmp method_System_Void_RETEND_Kernel_PreReqs_DECLEND_PageFaultDetection_NAMEEND___Fail");

            conversionState.Append(new ASMOps.Call() { Target = "GetEIP" });
            conversionState.AddExternalLabel("GetEIP");
            conversionState.Append(new ASMOps.Call() { Target = conversionState.GetHaltMethodInfo().ID });
            //Insert the not null label
            conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "NotNullMem" });

            //Call the specified constructor
            //This involves:
            // - Push empty dword onto stack
            // - Move all args down by one dword
            // - Move object reference into dword as first arg
            // - Call constructor
            conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Dword, Src = "0" });
            int sizeOfArgs = 0;
            ParameterInfo[] allParams = constructorMethod.GetParameters();
            foreach(ParameterInfo aParam in allParams)
            {
                sizeOfArgs += conversionState.TheILLibrary.GetTypeInfo(aParam.ParameterType).SizeOnStackInBytes;
                conversionState.CurrentStackFrame.Stack.Pop();
            }
            conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Src = "ESP", Dest = "EBX" });
            if (sizeOfArgs > 0)
            {
                if (sizeOfArgs % 4 != 0)
                {
                    throw new InvalidOperationException("sizeOfArgs not exact multiple of 4!");
                }

                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Src = (sizeOfArgs / 4).ToString(), Dest = "ECX" });
                conversionState.Append(new ASMOps.Label() { ILPosition = currOpPosition, Extension = "ShiftArgsLoop" });
                GlobalMethods.InsertPageFaultDetection(conversionState, "ebx", 4, (OpCodes)theOp.opCode.Value);
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Src = "[EBX+4]", Dest = "EDX" });
                GlobalMethods.InsertPageFaultDetection(conversionState, "ebx", 0, (OpCodes)theOp.opCode.Value);
                conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Dest = "[EBX]", Src = "EDX" });
                conversionState.Append(new ASMOps.Add() { Src = "4", Dest = "EBX" });
                conversionState.Append(new ASMOps.Loop() { ILPosition = currOpPosition, Extension = "ShiftArgsLoop" });
            }
            GlobalMethods.InsertPageFaultDetection(conversionState, "ebx", 0, (OpCodes)theOp.opCode.Value);
            conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Dest = "[EBX]", Src = "EAX" });
            conversionState.Append(new ASMOps.Call() { Target = constructorMethodInfo.ID });    
            //Only remove args from stack - we want the object pointer to remain on the stack
            conversionState.Append(new ASMOps.Add() { Src = sizeOfArgs.ToString(), Dest = "ESP" });

            conversionState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                isFloat = false,
                sizeOnStackInBytes = 4,
                isNewGCObject = true,
                isGCManaged = true
            });
        }
    }
}
