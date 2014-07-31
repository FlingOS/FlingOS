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
    public class StackSwitch : ILOps.StackSwitch
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

            int dwordsToRotate = anILOpInfo.ValueBytes == null ? 2 : BitConverter.ToInt32(anILOpInfo.ValueBytes, 0);
            
            int bytesShift = 0;
            for (int i = 0; i < dwordsToRotate; i++)
            {
                if (i == 0)
                {
                    result.AppendLine(string.Format("mov eax, [esp+{0}]", bytesShift));
                    result.AppendLine(string.Format("mov dword ebx, [esp+{0}]", bytesShift + 4));
                    result.AppendLine(string.Format("mov dword [esp+{0}], ebx", bytesShift));
                }
                else if (i == dwordsToRotate - 1)
                {
                    result.AppendLine(string.Format("mov [esp+{0}], eax", bytesShift));
                }
                else
                {
                    result.AppendLine(string.Format("mov dword ebx, [esp+{0}]", bytesShift + 4));
                    result.AppendLine(string.Format("mov dword [esp+{0}], ebx", bytesShift));
                }
                bytesShift += 4;
            }

            return result.ToString().Trim();
        }
    }
}
