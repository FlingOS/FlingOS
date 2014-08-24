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
    public class Ldftn : ILOps.Ldftn
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

            //Get the ID (i.e. ASM label) of the method to load a pointer to
            string methodID = aScannerState.GetMethodID(anILOpInfo.MethodToCall);
            //If we want to load the pointer at a specified IL op number:
            if(anILOpInfo.LoadAtILOffset != int.MaxValue)
            {
                //Append the IL sub-label to the ID
                methodID += ".IL_" + anILOpInfo.LoadAtILOffset + "_0";

                //Note: This is used by try/catch/finally blocks for pushing pointers 
                //      to catch/finally handlers and filters
            }
            //Push the pointer to the function
            result.AppendLine(string.Format("push dword {0}", methodID));

            aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                isFloat = false,
                sizeOnStackInBytes = 4
            });

            return result.ToString().Trim();
        }
    }
}
