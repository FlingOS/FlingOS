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
    public class Callvirt : IL.ILOps.Callvirt
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            MethodBase methodToCall = theOp.MethodToCall;
            MethodInfo methodToCallInfo = conversionState.TheILLibrary.GetMethodInfo(methodToCall);

            if (methodToCall is System.Reflection.MethodInfo)
            {
                if (typeof(Delegate).IsAssignableFrom(((System.Reflection.MethodInfo)methodToCall).DeclaringType))
                {
                    List<Type> allParams =
                        ((System.Reflection.MethodInfo)methodToCall).GetParameters()
                            .Select(x => x.ParameterType)
                            .ToList();

                    Type retType = ((System.Reflection.MethodInfo)methodToCall).ReturnType;
                    TypeInfo retTypeInfo = conversionState.TheILLibrary.GetTypeInfo(retType);
                    StackItem returnItem = new StackItem
                    {
                        isFloat = Utilities.IsFloat(retType),
                        sizeOnStackInBytes = retTypeInfo.SizeOnStackInBytes,
                        isGCManaged = retTypeInfo.IsGCManaged,
                        isValue = retTypeInfo.IsValueType
                    };


                    int bytesToAdd = 4;
                    foreach (Type aParam in allParams)
                    {
                        conversionState.CurrentStackFrame.GetStack(theOp).Pop();
                        bytesToAdd += conversionState.TheILLibrary.GetTypeInfo(aParam).SizeOnStackInBytes;
                    }

                    if (returnItem.sizeOnStackInBytes != 0)
                    {
                        conversionState.CurrentStackFrame.GetStack(theOp).Push(returnItem);
                    }
                }
                else
                {
                    string methodIDValueWanted = methodToCallInfo.IDValue.ToString();
                    int currOpPosition = conversionState.PositionOf(theOp);

                    TypeInfo declaringTypeInfo = conversionState.TheILLibrary.GetTypeInfo(methodToCall.DeclaringType);

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
                        ((System.Reflection.MethodInfo)methodToCall).GetParameters()
                            .Select(x => x.ParameterType)
                            .ToList();
                    if (!methodToCall.IsStatic)
                    {
                        allParams.Insert(0, methodToCall.DeclaringType);
                    }
                    foreach (Type aParam in allParams)
                    {
                        conversionState.CurrentStackFrame.GetStack(theOp).Pop();
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

            conversionState.AddExternalLabel(conversionState.GetThrowNullReferenceExceptionMethodInfo().ID);
            conversionState.AddExternalLabel(conversionState.GetObjectTypeMethodInfo().ID);

            //The method to call is a method base
            //A method base can be either a method info i.e. a normal method
            //or a constructor method. The two types are treated separately.
            if (methodToCall is System.Reflection.MethodInfo)
            {
                //Need to do callvirt related stuff to load address of method to call
                // - Check for invoke of a delegate - if so, treat rather differently from normal callvirt

                if (typeof(Delegate).IsAssignableFrom(((System.Reflection.MethodInfo)methodToCall).DeclaringType))
                {
                    //Callvirt to delegate method
                    // - We only support calls to Invoke at the moment
                    if (methodToCall.Name != "Invoke")
                    {
                        throw new NotSupportedException("Callvirt to Delegate method not supported! Method name: " +
                                                        methodToCall.Name);
                    }
                    int bytesForAllParams =
                        ((System.Reflection.MethodInfo)methodToCall).GetParameters()
                            .Select(x => conversionState.TheILLibrary.GetTypeInfo(x.ParameterType).SizeOnStackInBytes)
                            .Sum();

                    // - Move into eax address of function to call from stack - delegate reference is function pointer

                    //All the parameters for the method that was called
                    List<Type> allParams =
                        ((System.Reflection.MethodInfo)methodToCall).GetParameters()
                            .Select(x => x.ParameterType)
                            .ToList();

                    int bytesForParams =
                        allParams.Select(x => conversionState.TheILLibrary.GetTypeInfo(x).SizeOnStackInBytes).Sum();
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Dword,
                        Src = "[ESP+" + bytesForParams + "]",
                        Dest = "EAX"
                    });


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
                        for (int i = 0; i < returnItem.sizeOnStackInBytes; i += 4)
                        {
                            conversionState.Append(new Push {Size = OperandSize.Dword, Src = "0"});
                        }
                    }


                    //Append the actual call
                    conversionState.Append(new ASMOps.Call {Target = "EAX"});


                    //After a call, we need to remove the return value and parameters from the stack
                    //This is most easily done by just adding the total number of bytes for params and
                    //return value to the stack pointer (ESP register).

                    //Stores the number of bytes to add
                    // - Initially at least 4 for the delegate (method) ref/pointer
                    int bytesToAdd = 4;
                    //Go through all params that must be removed
                    foreach (Type aParam in allParams)
                    {
                        //Pop the paramter off our stack 
                        //(Note: Return value was never pushed onto our stack. See above)
                        conversionState.CurrentStackFrame.GetStack(theOp).Pop();
                        //Add the size of the paramter to the total number of bytes to pop
                        bytesToAdd += conversionState.TheILLibrary.GetTypeInfo(aParam).SizeOnStackInBytes;
                    }

                    //If there is a return value on the stack
                    if (returnItem.sizeOnStackInBytes != 0)
                    {
                        //We need to store the return value then pop all the params

                        //We now push the return value onto our stack as,
                        //after all is said and done below, it will be the 
                        //top item on the stack
                        conversionState.CurrentStackFrame.GetStack(theOp).Push(returnItem);

                        //SUPPORT - floats (with above)

                        //Pop the return value, shift it into final position                        
                        for (int i = 0; i < returnItem.sizeOnStackInBytes; i += 4)
                        {
                            int srcOffset = returnItem.sizeOnStackInBytes - i - 4;
                            int destOffset = bytesToAdd + srcOffset;

                            conversionState.Append(new Mov
                            {
                                Size = OperandSize.Dword,
                                Dest = "EAX",
                                Src = "[ESP+" + srcOffset + "]"
                            });
                            conversionState.Append(new Mov
                            {
                                Size = OperandSize.Dword,
                                Dest = "[ESP+" + destOffset + "]",
                                Src = "EAX"
                            });
                        }
                    }
                    //Skip over the params
                    conversionState.Append(new ASMOps.Add {Src = bytesToAdd.ToString(), Dest = "ESP"});
                }
                else
                {
                    //Normal callvirt
                    // - Get object ref from loaded args
                    // - Check object ref not null
                    // - Get type table entry from object ref
                    // - Get method table from type table entry
                    // - Scan method table for the method we want
                    //      - If found, load method address
                    // - Else, check for parent type method table
                    //      - If no parent type method table, throw exception
                    // - Else, scan parent type method table

                    string methodIDValueWanted = methodToCallInfo.IDValue.ToString();
                    int currOpPosition = conversionState.PositionOf(theOp);

                    TypeInfo declaringTypeInfo = conversionState.TheILLibrary.GetTypeInfo(methodToCall.DeclaringType);
                    //DB_Type declaringDBType = DebugDatabase.GetType(conversionState.GetTypeID(methodToCall.DeclaringType));

                    //Get object ref
                    int bytesForAllParams =
                        ((System.Reflection.MethodInfo)methodToCall).GetParameters()
                            .Select(x => conversionState.TheILLibrary.GetTypeInfo(x.ParameterType).SizeOnStackInBytes)
                            .Sum();
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Dword,
                        Src = "[ESP+" + bytesForAllParams + "]",
                        Dest = "EAX"
                    });

                    //Check object ref
                    conversionState.Append(new Cmp {Arg1 = "EAX", Arg2 = "0"});
                    conversionState.Append(new Jmp
                    {
                        JumpType = JmpOp.JumpNotZero,
                        DestILPosition = currOpPosition,
                        Extension = "NotNull"
                    });
                    conversionState.Append(new ASMOps.Call {Target = "GetEIP"});
                    conversionState.AddExternalLabel("GetEIP");
                    conversionState.Append(new ASMOps.Call
                    {
                        Target = conversionState.GetThrowNullReferenceExceptionMethodInfo().ID
                    });
                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "NotNull"});

                    //Get type ref
                    //int typeOffset = conversionState.TheILLibrary.GetFieldInfo(declaringTypeInfo, "_Type").OffsetInBytes;
                    //conversionState.Append(new ASMOps.Mov() { Size = ASMOps.OperandSize.Dword, Src = "[EAX+" + typeOffset.ToString() + "]", Dest = "EAX" });
                    conversionState.Append(new Push {Size = OperandSize.Dword, Src = "EAX"});
                    conversionState.Append(new Push {Size = OperandSize.Dword, Src = "0"});
                    conversionState.Append(new ASMOps.Call {Target = conversionState.GetObjectTypeMethodInfo().ID});
                    conversionState.Append(new ASMOps.Pop {Size = OperandSize.Dword, Dest = "EAX"});
                    conversionState.Append(new ASMOps.Add {Src = "4", Dest = "ESP"});

                    //Get method table ref
                    int methodTablePtrOffset = conversionState.GetTypeFieldOffset("MethodTablePtr");
                    conversionState.Append(new Mov
                    {
                        Size = OperandSize.Dword,
                        Src = "[EAX+" + methodTablePtrOffset + "]",
                        Dest = "EAX"
                    });

                    //Loop through entries
                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "LoopMethodTable"});
                    //Load ID Val for current entry
                    conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "[EAX]", Dest = "EBX"});
                    //Compare to wanted ID value
                    conversionState.Append(new Cmp {Arg1 = "EBX", Arg2 = methodIDValueWanted});
                    //If equal, load method address into EAX
                    conversionState.Append(new Jmp
                    {
                        JumpType = JmpOp.JumpNotEqual,
                        DestILPosition = currOpPosition,
                        Extension = "NotEqual"
                    });
                    conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "[EAX+4]", Dest = "EAX"});
                    conversionState.Append(new Jmp
                    {
                        JumpType = JmpOp.Jump,
                        DestILPosition = currOpPosition,
                        Extension = "Call"
                    });
                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "NotEqual"});
                    //Else, compare to 0 to check for end of table
                    conversionState.Append(new Cmp {Arg1 = "EBX", Arg2 = "0"});
                    conversionState.Append(new Jmp
                    {
                        JumpType = JmpOp.JumpZero,
                        DestILPosition = currOpPosition,
                        Extension = "EndOfTable"
                    });
                    //Not 0? Move to next entry then loop again
                    conversionState.Append(new ASMOps.Add {Src = "8", Dest = "EAX"});
                    conversionState.Append(new Jmp
                    {
                        JumpType = JmpOp.Jump,
                        DestILPosition = currOpPosition,
                        Extension = "LoopMethodTable"
                    });
                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "EndOfTable"});
                    //Compare address value to 0
                    //If not zero, there is a parent method table to check
                    conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "[EAX+4]", Dest = "EBX"});
                    conversionState.Append(new Cmp {Arg1 = "EBX", Arg2 = "0"});
                    conversionState.Append(new Jmp
                    {
                        JumpType = JmpOp.JumpZero,
                        DestILPosition = currOpPosition,
                        Extension = "NotFound"
                    });
                    //Load parent method table and loop 
                    conversionState.Append(new Mov {Size = OperandSize.Dword, Src = "EBX", Dest = "EAX"});
                    conversionState.Append(new Jmp
                    {
                        JumpType = JmpOp.Jump,
                        DestILPosition = currOpPosition,
                        Extension = "LoopMethodTable"
                    });
                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "NotFound"});
                    //Throw exception!
                    conversionState.Append(new ASMOps.Call {Target = "GetEIP"});
                    conversionState.AddExternalLabel("GetEIP");
                    conversionState.Append(new ASMOps.Call
                    {
                        Target = conversionState.GetThrowNullReferenceExceptionMethodInfo().ID
                    });

                    conversionState.Append(new Label {ILPosition = currOpPosition, Extension = "Call"});

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
                        for (int i = 0; i < returnItem.sizeOnStackInBytes; i += 4)
                        {
                            conversionState.Append(new Push {Size = OperandSize.Dword, Src = "0"});
                        }
                    }


                    //Append the actual call
                    conversionState.Append(new ASMOps.Call {Target = "EAX"});


                    //After a call, we need to remove the return value and parameters from the stack
                    //This is most easily done by just adding the total number of bytes for params and
                    //return value to the stack pointer (ESP register).

                    //Stores the number of bytes to add
                    int bytesToAdd = 0;
                    //All the parameters for the method that was called
                    List<Type> allParams =
                        ((System.Reflection.MethodInfo)methodToCall).GetParameters()
                            .Select(x => x.ParameterType)
                            .ToList();
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

                            //Pop the return value, shift it into final position                        
                            for (int i = 0; i < returnItem.sizeOnStackInBytes; i += 4)
                            {
                                int srcOffset = returnItem.sizeOnStackInBytes - i - 4;
                                int destOffset = bytesToAdd + srcOffset;

                                conversionState.Append(new Mov
                                {
                                    Size = OperandSize.Dword,
                                    Dest = "EAX",
                                    Src = "[ESP+" + srcOffset + "]"
                                });
                                conversionState.Append(new Mov
                                {
                                    Size = OperandSize.Dword,
                                    Dest = "[ESP+" + destOffset + "]",
                                    Src = "EAX"
                                });
                            }
                        }
                        //Skip over the params
                        conversionState.Append(new ASMOps.Add {Src = bytesToAdd.ToString(), Dest = "ESP"});
                    }
                    //No params to skip over but we might still need to store return value
                    else if (returnItem.sizeOnStackInBytes != 0)
                    {
                        //The return value will be the top item on the stack.
                        //So all we need to do is push the return item onto our stack.
                        conversionState.CurrentStackFrame.GetStack(theOp).Push(returnItem);
                    }
                }
            }
            else if (methodToCall is ConstructorInfo)
            {
                throw new NotSupportedException("How the hell are we getting callvirts to constructor methods?!");
            }
        }
    }
}