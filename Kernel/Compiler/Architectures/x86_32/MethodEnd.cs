#region Copyright Notice
/// ------------------------------------------------------------------------------ ///
///                                                                                ///
///               All contents copyright � Edward Nutting 2014                     ///
///                                                                                ///
///        You may not share, reuse, redistribute or otherwise use the             ///
///        contents this file outside of the Fling OS project without              ///
///        the express permission of Edward Nutting or other copyright             ///
///        holder. Any changes (including but not limited to additions,            ///
///        edits or subtractions) made to or from this document are not            ///
///        your copyright. They are the copyright of the main copyright            ///
///        holder for all Fling OS files. At the time of writing, this             ///
///        owner was Edward Nutting. To be clear, owner(s) do not include          ///
///        developers, contributors or other project members.                      ///
///                                                                                ///
/// ------------------------------------------------------------------------------ ///
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
    public class MethodEnd : ILOps.MethodEnd
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown when the return value is a float or the size on the stack
        /// in bytes is not 4 or 8 bytes.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            //Store the return value
            //Get the return type
            Type retType = (aScannerState.CurrentILChunk.Method.IsConstructor || aScannerState.CurrentILChunk.Method is ConstructorInfo ?
                typeof(void) : ((MethodInfo)aScannerState.CurrentILChunk.Method).ReturnType);
            //Get the size of the return type on stack
            int retSize = Utils.GetNumBytesForType(retType);
            //If the size isn't 0 (i.e. isn't "void" which has no return value)
            if (retSize != 0)
            {
                //Pop the return value off our stack
                StackItem retItem = aScannerState.CurrentStackFrame.Stack.Pop();

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
                    result.AppendLine("pop dword eax");
                    result.AppendLine("mov [ebp+8], eax");
                }
                else if (retSize == 8)
                {
                    result.AppendLine("pop dword eax");
                    result.AppendLine("mov [ebp+8], eax");
                    result.AppendLine("pop dword eax");
                    result.AppendLine("mov [ebp+12], eax");
                }
                else
                {
                    throw new NotSupportedException("Return type size not supported / invalid!");
                }
            }

            //Once return value is off the stack, remove the locals
            //Deallocate stack space for locals
            //Only bother if there are any locals
            if (aScannerState.CurrentILChunk.LocalVariables.Count > 0)
            {
                //Get the total size of all locals
                int totalBytes = 0;
                foreach (StackItem aLocal in aScannerState.CurrentILChunk.LocalVariables)
                {
                    totalBytes += aLocal.sizeOnStackInBytes;
                }
                //Move esp past the locals
                result.AppendLine(string.Format("add esp, {0}", totalBytes));
            }

            //Restore ebp to previous method's ebp
            result.AppendLine("pop dword ebp");
            //This pop also takes last value off the stack which
            //means top item is the return address
            //So ret command can now be correctly executed.

            return result.ToString().Trim();
        }
    }
}
