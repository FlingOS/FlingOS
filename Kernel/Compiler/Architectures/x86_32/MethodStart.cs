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

namespace Kernel.Compiler.Architectures.x86_32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class MethodStart : ILOps.MethodStart
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

            //Push the previous method's ebp
            result.AppendLine("push dword ebp");
            //Set ebp for this method
            //See calling convention spec - this allows easy access of
            //args and locals within the method without having to track
            //temporary values (which would be a nightmare with the
            //exception handling implmentation that the kernel uses!)
            result.AppendLine("mov dword ebp, esp");

            //Allocate stack space for locals
            //Only bother if there are any locals
            if (aScannerState.CurrentILChunk.LocalVariables.Count > 0)
            {
                int totalBytes = 0;
                foreach (StackItem aLocal in aScannerState.CurrentILChunk.LocalVariables)
                {
                    totalBytes += aLocal.sizeOnStackInBytes;
                }
                //We do not use "sub esp, X" (see below) because that leaves
                //junk memory - we need memory to be "initialised" to 0 
                //so that local variables are null unless properly initialised.
                //This prevents errors in the GC.
                for (int i = 0; i < totalBytes / 4; i++)
                {
                    result.AppendLine("push dword 0");
                }
                //result.AppendLine(string.Format("sub esp, {0}", totalBytes));
            }

            return result.ToString().Trim();
        }
    }
}
