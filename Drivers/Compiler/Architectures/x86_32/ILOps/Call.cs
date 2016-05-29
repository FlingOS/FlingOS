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
using System.Reflection;
using Drivers.Compiler.Architectures.x86.ASMOps;
using Drivers.Compiler.IL;
using MethodInfo = Drivers.Compiler.Types.MethodInfo;
using TypeInfo = Drivers.Compiler.Types.TypeInfo;

namespace Drivers.Compiler.Architectures.x86
{
    /// <summary>
    ///     See base class documentation.
    /// </summary>
    public class Call : IL.ILOps.Call
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            MethodBase methodToCall = theOp.MethodToCall;
            MethodInfo methodToCallInfo = conversionState.TheILLibrary.GetMethodInfo(methodToCall);

            if (methodToCall is System.Reflection.MethodInfo)
            {
                Type retType = ((System.Reflection.MethodInfo)methodToCall).ReturnType;
                TypeInfo retTypeInfo = conversionState.TheILLibrary.GetTypeInfo(retType);
                StackItem returnItem = new StackItem
                {
                    isFloat = Utilities.IsFloat(retType),
                    sizeOnStackInBytes = retTypeInfo.SizeOnStackInBytes,
                    isGCManaged = retTypeInfo.IsGCManaged,
                    isValue = retTypeInfo.IsValueType
                };

                int bytesToAdd = 0;
                List<Type> allParams =
                    ((System.Reflection.MethodInfo)methodToCall).GetParameters().Select(x => x.ParameterType).ToList();
                if (!methodToCall.IsStatic)
                {
                    allParams.Insert(0, methodToCall.DeclaringType);
                }
                foreach (Type aParam in allParams)
                {
                    Stack<StackItem> stack = conversionState.CurrentStackFrame.GetStack(theOp);
                    stack.Pop();
                    bytesToAdd += conversionState.TheILLibrary.GetTypeInfo(aParam).SizeOnStackInBytes;
                }
                if (bytesToAdd > 0)
                {
                    if (returnItem.sizeOnStackInBytes != 0)
                    {
                        conversionState.CurrentStackFrame.GetStack(theOp).Push(returnItem);
                    }
                }
                else if (returnItem.sizeOnStackInBytes != 0)
                {
                    conversionState.CurrentStackFrame.GetStack(theOp).Push(returnItem);
                }
            }
            else if (methodToCall is ConstructorInfo)
            {
                ConstructorInfo aConstructor = (ConstructorInfo)methodToCall;
                if (aConstructor.IsStatic)
                {
                    //Static constructors do not have parameters or return values
                }
                else
                {
                    ParameterInfo[] allParams = methodToCall.GetParameters();
                    foreach (ParameterInfo aParam in allParams)
                    {
                        conversionState.CurrentStackFrame.GetStack(theOp).Pop();
                    }
                }
            }
        }

        /// <summary>
        ///     See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        ///     Thrown if any argument or the return value is a floating point number.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            MethodBase methodToCall = theOp.MethodToCall;
            MethodInfo methodToCallInfo = conversionState.TheILLibrary.GetMethodInfo(methodToCall);

            conversionState.AddExternalLabel(methodToCallInfo.ID);

            //The method to call is a method base
            //A method base can be either a method info i.e. a normal method
            //or a constructor method. The two types are treated separately.
            if (methodToCall is System.Reflection.MethodInfo)
            {
                //Allocate space on the stack for the return value as necessary
                Type retType = ((System.Reflection.MethodInfo)methodToCall).ReturnType;
                TypeInfo retTypeInfo = conversionState.TheILLibrary.GetTypeInfo(retType);
                StackItem returnItem = new StackItem
                {
                    isFloat = Utilities.IsFloat(retType),
                    sizeOnStackInBytes = retTypeInfo.SizeOnStackInBytes,
                    isGCManaged = retTypeInfo.IsGCManaged,
                    isValue = retTypeInfo.IsValueType
                };
                //We do not push the return value onto the stack unless it has size > 0
                //We do not push the return value onto our stack at this point - it is pushed after the call is done

                if (returnItem.sizeOnStackInBytes != 0)
                {
                    if (returnItem.isFloat)
                    {
                        //SUPPORT - floats
                        throw new NotSupportedException("Cannot handle float return values!");
                    }
                    if (returnItem.sizeOnStackInBytes == 4)
                    {
                        conversionState.Append(new Push {Size = OperandSize.Dword, Src = "0"});
                    }
                    else if (returnItem.sizeOnStackInBytes == 8)
                    {
                        conversionState.Append(new Push {Size = OperandSize.Dword, Src = "0"});
                        conversionState.Append(new Push {Size = OperandSize.Dword, Src = "0"});
                    }
                    else
                    {
                        throw new NotSupportedException("Invalid return stack operand size!");
                    }
                }

                //Append the actual call
                conversionState.Append(new ASMOps.Call {Target = methodToCallInfo.ID});

                //After a call, we need to remove the return value and parameters from the stack
                //This is most easily done by just adding the total number of bytes for params and
                //return value to the stack pointer (ESP register).

                //Stores the number of bytes to add
                int bytesToAdd = 0;
                //All the parameters for the method that was called
                List<Type> allParams =
                    ((System.Reflection.MethodInfo)methodToCall).GetParameters().Select(x => x.ParameterType).ToList();
                //Go through each one
                if (!methodToCall.IsStatic)
                {
                    allParams.Insert(0, methodToCall.DeclaringType);
                }
                foreach (Type aParam in allParams)
                {
                    //Pop the paramter off our stack 
                    //(Note: Return value was never pushed onto our stack. See above)
                    conversionState.CurrentStackFrame.GetStack(theOp).Pop();
                    //Add the size of the paramter to the total number of bytes to pop
                    bytesToAdd += conversionState.TheILLibrary.GetTypeInfo(aParam).SizeOnStackInBytes;
                }
                //If the number of bytes to add to skip over params is > 0
                if (bytesToAdd > 0)
                {
                    //If there is a return value on the stack
                    if (returnItem.sizeOnStackInBytes != 0)
                    {
                        //We need to store the return value then pop all the params

                        //We now push the return value onto our stack as,
                        //after all is said and done below, it will be the 
                        //top item on the stack
                        conversionState.CurrentStackFrame.GetStack(theOp).Push(returnItem);

                        //SUPPORT - floats (with above)

                        //Pop the return value into the eax register
                        //We will push it back on after params are skipped over.
                        if (returnItem.sizeOnStackInBytes == 4)
                        {
                            conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EAX"});
                        }
                        else if (returnItem.sizeOnStackInBytes == 8)
                        {
                            conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EAX"});
                            conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EDX"});
                        }
                    }
                    //Skip over the params
                    conversionState.Append(new ASMOps.Add {Src = bytesToAdd.ToString(), Dest = "ESP"});
                    //If necessary, push the return value onto the stack.
                    if (returnItem.sizeOnStackInBytes != 0)
                    {
                        //SUPPORT - floats (with above)

                        //The return value was stored in eax
                        //So push it back onto the stack
                        if (returnItem.sizeOnStackInBytes == 4)
                        {
                            conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EAX"});
                        }
                        else if (returnItem.sizeOnStackInBytes == 8)
                        {
                            conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EDX"});
                            conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EAX"});
                        }
                    }
                }
                //No params to skip over but we might still need to store return value
                else if (returnItem.sizeOnStackInBytes != 0)
                {
                    //The return value will be the top item on the stack.
                    //So all we need to do is push the return item onto our stack.
                    conversionState.CurrentStackFrame.GetStack(theOp).Push(returnItem);
                }
            }
            else if (methodToCall is ConstructorInfo)
            {
                ConstructorInfo aConstructor = (ConstructorInfo)methodToCall;
                if (aConstructor.IsStatic)
                {
                    //Static constructors do not have parameters or return values

                    //Append the actual call
                    conversionState.Append(new ASMOps.Call {Target = methodToCallInfo.ID});
                }
                else
                {
                    //Append the actual call
                    conversionState.Append(new ASMOps.Call {Target = methodToCallInfo.ID});

                    //After a call, we need to remove the parameters from the stack
                    //This is most easily done by just adding the total number of bytes for params
                    //to the stack pointer (ESP register).

                    //Stores the number of bytes to add
                    int bytesToAdd = 0;
                    //All the parameters for the method that was called
                    ParameterInfo[] allParams = methodToCall.GetParameters();
                    //Go through each one
                    foreach (ParameterInfo aParam in allParams)
                    {
                        //Pop the paramter off our stack 
                        //(Note: Return value was never pushed onto our stack. See above)
                        conversionState.CurrentStackFrame.GetStack(theOp).Pop();
                        //Add the size of the paramter to the total number of bytes to pop
                        bytesToAdd += conversionState.TheILLibrary.GetTypeInfo(aParam.ParameterType).SizeOnStackInBytes;
                    }
                    //Add 4 bytes for the instance ref
                    bytesToAdd += 4;
                    //If the number of bytes to add to skip over params is > 0
                    if (bytesToAdd > 0)
                    {
                        //Skip over the params
                        conversionState.Append(new ASMOps.Add {Src = bytesToAdd.ToString(), Dest = "ESP"});
                    }
                }
            }
        }
    }
}