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

namespace Kernel.Compiler.Architectures.x86_32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Ldobj : ILOps.Ldobj
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown when loading a static float field.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            //Load static field

            //Load the metadata token used to get the type info
            int metadataToken = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
            //Get the type info for the object to load
            Type theType = aScannerState.CurrentILChunk.Method.Module.ResolveType(metadataToken);

            //Get the object size information
            int size = Utils.GetNumBytesForType(theType);

            //Load the object onto the stack
            result.AppendLine("pop dword ecx");
            for (int i = size - 4; i >= 0; i -= 4)
            {
                result.AppendLine(string.Format("mov dword eax, [ecx+{0}]", i));
                result.AppendLine("push dword eax");
            }
            int extra = size % 4;
            for (int i = extra - 1; i >= 0; i--)
            {
                result.AppendLine(string.Format("mov byte al, [ecx+{0}]", i));
                result.AppendLine("push byte al");
            }

            aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                isFloat = false,
                sizeOnStackInBytes = size
            });

            return result.ToString().Trim();
        }
    }
}
