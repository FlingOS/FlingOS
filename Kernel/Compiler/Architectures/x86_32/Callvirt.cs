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
    public class Callvirt : ILOps.Callvirt
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if any argument or the return value is a floating point number.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            MethodBase methodToCall = anILOpInfo.MethodToCall;
            //The method to call is a method base
            //A method base can be either a method info i.e. a normal method
            //or a constructor method. The two types are treated separately.
            if(methodToCall is MethodInfo)
            {
                //Need to do callvirt related stuff to load address of method to call
                // - Check for invoke of a delegate - if so, treat rather differently from normal callvirt


                string call_Label = string.Format("{0}.IL_{1}_Call",
                                                aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                                                anILOpInfo.Position);

                if (typeof(Delegate).IsAssignableFrom(((MethodInfo)methodToCall).DeclaringType))
                {
                    //Callvirt to delegate method
                    // - We only support calls to Invoke at the moment
                    if (methodToCall.Name != "Invoke")
                    {
                        throw new NotSupportedException("Callvirt to Delegate method not supported! Method name: " + methodToCall.Name);
                    }
                    int bytesForAllParams = ((MethodInfo)methodToCall).GetParameters().Select(x => Utils.GetNumBytesForType(x.ParameterType)).Sum();
                    
                    // - Move into eax address of function to call from stack - delegate reference is function pointer

                    //All the parameters for the method that was called
                    List<Type> allParams = ((MethodInfo)methodToCall).GetParameters().Select(x => x.ParameterType).ToList();

                    int bytesForParams = allParams.Select(x => Utils.GetNumBytesForType(x)).Sum();
                    result.AppendLine(string.Format("mov dword eax, [esp+{0}]", bytesForParams));


                    //Allocate space on the stack for the return value as necessary
                    Type retType = ((MethodInfo)methodToCall).ReturnType;
                    StackItem returnItem = new StackItem()
                    {
                        isFloat = Utils.IsFloat(retType),
                        sizeOnStackInBytes = Utils.GetNumBytesForType(retType)
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
                        else if (returnItem.sizeOnStackInBytes == 4)
                        {
                            result.AppendLine("push dword 0");
                        }
                        else if (returnItem.sizeOnStackInBytes == 8)
                        {
                            result.AppendLine("push dword 0");
                            result.AppendLine("push dword 0");
                        }
                        else
                        {
                            throw new NotSupportedException("Invalid return stack operand size!");
                        }
                    }
                    


                    //Append the actual call
                    result.AppendLine("call eax");
                    


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
                        aScannerState.CurrentStackFrame.Stack.Pop();
                        //Add the size of the paramter to the total number of bytes to pop
                        bytesToAdd += Utils.GetNumBytesForType(aParam);
                    }
                        
                    //If there is a return value on the stack
                    if (returnItem.sizeOnStackInBytes != 0)
                    {
                        //We need to store the return value then pop all the params

                        //We now push the return value onto our stack as,
                        //after all is said and done below, it will be the 
                        //top item on the stack
                        aScannerState.CurrentStackFrame.Stack.Push(returnItem);

                        //SUPPORT - floats (with above)

                        //Pop the return value into the eax register
                        //We will push it back on after params are skipped over.
                        if (returnItem.sizeOnStackInBytes == 4)
                        {
                            result.AppendLine("pop dword eax");
                        }
                        else if (returnItem.sizeOnStackInBytes == 8)
                        {
                            result.AppendLine("pop dword eax");
                            result.AppendLine("pop dword edx");
                        }
                    }
                    //Skip over the params
                    result.AppendLine(string.Format("add esp, {0}", bytesToAdd));
                    //If necessary, push the return value onto the stack.
                    if (returnItem.sizeOnStackInBytes != 0)
                    {
                        //SUPPORT - floats (with above)

                        //The return value was stored in eax
                        //So push it back onto the stack
                        if (returnItem.sizeOnStackInBytes == 4)
                        {
                            result.AppendLine("push dword eax");
                        }
                        else if (returnItem.sizeOnStackInBytes == 8)
                        {
                            result.AppendLine("push dword edx");
                            result.AppendLine("push dword eax");
                        }
                    }
                }
                else
                {
                    //Normal callvirt
                    // - Get object ref from loaded args
                    // - Get type table entry from object ref
                    // - Get method table from type table entry
                    // - Scan method table for the method we want
                    //      - If found, load method address
                    // - Else, check for parent type method table
                    //      - If no parent type method table, throw exception
                    // - Else, scan parent type method table

                    string methodIDValueWanted = aScannerState.GetMethodIDValue((MethodInfo)methodToCall);
                    string loopTableEntries_Label = string.Format("{0}.IL_{1}_LoopMethodTable",
                                                    aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                                                    anILOpInfo.Position);
                    string notEqual_Label = string.Format("{0}.IL_{1}_NotEqual",
                                                    aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                                                    anILOpInfo.Position);
                    string endOfTable_Label = string.Format("{0}.IL_{1}_EndOfTable",
                                                    aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                                                    anILOpInfo.Position);
                    string notFound_Label = string.Format("{0}.IL_{1}_NotFound",
                                                    aScannerState.GetMethodID(aScannerState.CurrentILChunk.Method),
                                                    anILOpInfo.Position);
                    DB_Type declaringDBType = DebugDatabase.GetType(aScannerState.GetTypeID(methodToCall.DeclaringType));

                    //Get object ref
                    int bytesForAllParams = ((MethodInfo)methodToCall).GetParameters().Select(x => Utils.GetNumBytesForType(x.ParameterType)).Sum();
                    result.AppendLine(string.Format("mov dword eax, [esp+{0}]", bytesForAllParams));

                    //Get type ref
                    int typeOffset = aScannerState.GetFieldOffset(declaringDBType, "_Type");
                    result.AppendLine(string.Format("mov eax, [eax+{0}]", typeOffset));

                    //Get method table ref
                    int methodTablePtrOffset = aScannerState.GetTypeFieldOffset("MethodTablePtr");
                    result.AppendLine(string.Format("mov eax, [eax+{0}]", methodTablePtrOffset));

                    //Loop through entries
                    result.AppendLine(loopTableEntries_Label + ":");
                    //Load ID Val for current entry
                    result.AppendLine("mov ebx, [eax]");
                    //Compare to wanted ID value
                    result.AppendLine("cmp ebx, " + methodIDValueWanted);
                    //If equal, load method address into eax
                    result.AppendLine("jne " + notEqual_Label);
                    result.AppendLine("mov eax, [eax+4]");
                    result.AppendLine("jmp " + call_Label);
                    result.AppendLine(notEqual_Label + ":");
                    //Else, compare to 0 to check for end of table
                    result.AppendLine("cmp ebx, 0");
                    result.AppendLine("jz " + endOfTable_Label);
                    //Not 0? Move to next entry then loop again
                    result.AppendLine("add eax, 8");
                    result.AppendLine("jmp " + loopTableEntries_Label);
                    result.AppendLine(endOfTable_Label + ":");
                    //Compare address value to 0
                    //If not zero, there is a parent method table to check
                    result.AppendLine("mov ebx, [eax+4]");
                    result.AppendLine("cmp ebx, 0");
                    result.AppendLine("jz " + notFound_Label);
                    //Load parent method table and loop 
                    result.AppendLine("mov eax, ebx");
                    result.AppendLine("jmp " + loopTableEntries_Label);
                    result.AppendLine(notFound_Label + ":");
                    //Throw exception!
                    result.AppendLine(string.Format("call {0}", aScannerState.GetMethodID(aScannerState.ThrowNullReferenceExceptionMethod)));

                    result.AppendLine(call_Label + ":");

                    //Allocate space on the stack for the return value as necessary
                    Type retType = ((MethodInfo)methodToCall).ReturnType;
                    StackItem returnItem = new StackItem()
                    {
                        isFloat = Utils.IsFloat(retType),
                        sizeOnStackInBytes = Utils.GetNumBytesForType(retType)
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
                        else if (returnItem.sizeOnStackInBytes == 4)
                        {
                            result.AppendLine("push dword 0");
                        }
                        else if (returnItem.sizeOnStackInBytes == 8)
                        {
                            result.AppendLine("push dword 0");
                            result.AppendLine("push dword 0");
                        }
                        else
                        {
                            throw new NotSupportedException("Invalid return stack operand size!");
                        }
                    }


                    //Append the actual call
                    result.AppendLine("call eax");



                    //After a call, we need to remove the return value and parameters from the stack
                    //This is most easily done by just adding the total number of bytes for params and
                    //return value to the stack pointer (ESP register).

                    //Stores the number of bytes to add
                    int bytesToAdd = 0;
                    //All the parameters for the method that was called
                    List<Type> allParams = ((MethodInfo)methodToCall).GetParameters().Select(x => x.ParameterType).ToList();
                    //Go through each one
                    if (!methodToCall.IsStatic)
                    {
                        allParams.Insert(0, methodToCall.DeclaringType);
                    }
                    foreach (Type aParam in allParams)
                    {
                        //Pop the paramter off our stack 
                        //(Note: Return value was never pushed onto our stack. See above)
                        aScannerState.CurrentStackFrame.Stack.Pop();
                        //Add the size of the paramter to the total number of bytes to pop
                        bytesToAdd += Utils.GetNumBytesForType(aParam);
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
                            aScannerState.CurrentStackFrame.Stack.Push(returnItem);

                            //SUPPORT - floats (with above)

                            //Pop the return value into the eax register
                            //We will push it back on after params are skipped over.
                            if (returnItem.sizeOnStackInBytes == 4)
                            {
                                result.AppendLine("pop dword eax");
                            }
                            else if (returnItem.sizeOnStackInBytes == 8)
                            {
                                result.AppendLine("pop dword eax");
                                result.AppendLine("pop dword edx");
                            }
                        }
                        //Skip over the params
                        result.AppendLine(string.Format("add esp, {0}", bytesToAdd));
                        //If necessary, push the return value onto the stack.
                        if (returnItem.sizeOnStackInBytes != 0)
                        {
                            //SUPPORT - floats (with above)

                            //The return value was stored in eax
                            //So push it back onto the stack
                            if (returnItem.sizeOnStackInBytes == 4)
                            {
                                result.AppendLine("push dword eax");
                            }
                            else if (returnItem.sizeOnStackInBytes == 8)
                            {
                                result.AppendLine("push dword edx");
                                result.AppendLine("push dword eax");
                            }
                        }
                    }
                    //No params to skip over but we might still need to store return value
                    else if (returnItem.sizeOnStackInBytes != 0)
                    {
                        //The return value will be the top item on the stack.
                        //So all we need to do is push the return item onto our stack.
                        aScannerState.CurrentStackFrame.Stack.Push(returnItem);
                    }
                }
            }
            else if(methodToCall is ConstructorInfo)
            {
                throw new NotSupportedException("How the hell are we getting callvirts to constructor methods?!");
            }
            
            return result.ToString().Trim();
        }
    }
}
