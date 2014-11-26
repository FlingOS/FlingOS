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
    public class Ldfld : ILOps.Ldfld
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

            //Get the field's token that is used to get FieldInfo from the assembly
            int metadataToken = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
            //Get the field info from the referencing assembly
            FieldInfo theField = aScannerState.CurrentILChunk.Method.Module.ResolveField(metadataToken);
            //Get the database type information about the object that contains the field
            DB_Type objDBType = DebugDatabase.GetType(aScannerState.GetTypeID(theField.DeclaringType));
            int offset = aScannerState.GetFieldOffset(objDBType, theField.Name);

            //Is the value to load a floating pointer number?
            bool valueisFloat = Utils.IsFloat(theField.FieldType);
            //Get the size of the value to load (in bytes, as it will appear on the stack)
            int stackSize = Utils.GetNumBytesForType(theField.FieldType);
            int memSize = theField.FieldType.IsValueType ? Utils.GetSizeForType(theField.FieldType) : stackSize;

            //Pop the object pointer from our stack
            StackItem objPointer = aScannerState.CurrentStackFrame.Stack.Pop();
            
            //If the value to load is a float, erm...abort...
            if (valueisFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Loading fields of type float not supported yet!");
            }

            //Pop object pointer
            result.AppendLine("pop dword ecx");
            if ((OpCodes)anILOpInfo.opCode.Value == OpCodes.Ldflda)
            {
                result.AppendLine(string.Format("add ecx, {0}", offset));
                result.AppendLine("push dword ecx");

                aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = false,
                    sizeOnStackInBytes = 4,
                    isGCManaged = false
                });
            }
            else
            {
                //Push value at pointer+offset
                int sizeNotInMem = stackSize - memSize;
                int sizeToSub = (sizeNotInMem / 2) * 2; //Rounds down
                for (int i = 0; i < sizeToSub; i += 2)
                {
                    result.AppendLine("push word 0");
                }
                for (int i = memSize + (memSize % 2); i > 0; i -= 2)
                {
                    if (sizeToSub != sizeNotInMem)
                    {
                        result.AppendLine("mov ax, 0");
                        result.AppendLine(string.Format("mov byte al, [ecx+{0}]", offset + i - 2));
                        result.AppendLine("push word ax");
                    }
                    else
                    {
                        result.AppendLine(string.Format("mov word ax, [ecx+{0}]", offset + i - 2));
                        result.AppendLine("push word ax");
                    }
                }

                aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = valueisFloat,
                    sizeOnStackInBytes = stackSize,
                    isGCManaged = Utils.IsGCManaged(theField.FieldType)
                });
            }

            return result.ToString().Trim();
        }
    }
}
