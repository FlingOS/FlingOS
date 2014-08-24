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

namespace Kernel.Compiler.Architectures.x86_32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Ldstr : ILOps.Ldstr
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

            //Load a string literal (- fixed string i.e. one programmed as "a string in code")

            //Get the string metadata token used to get the string from the assembly
            int StringMetadataToken = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
            //Get the value of the string to load
            string theString = aScannerState.CurrentILChunk.Method.Module.ResolveString(StringMetadataToken);
            //Add the string literal and get its ID
            string theStringID = aScannerState.AddStringLiteral(theString, anILOpInfo);

            //Push the address of the string (i.e. address of ID - ASM label)
            result.AppendLine(string.Format("push {0}", theStringID));

            aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                sizeOnStackInBytes = 4,
                isFloat = false
            });

            return result.ToString().Trim();
        }
    }
}
