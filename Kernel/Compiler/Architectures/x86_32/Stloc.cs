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
    public class Stloc : ILOps.Stloc
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown if the value to store is floating point.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            UInt16 localIndex = 0;
            switch ((ILOps.ILOp.OpCodes)anILOpInfo.opCode.Value)
            {
                case OpCodes.Stloc:
                    localIndex = (UInt16)Utils.ReadInt16(anILOpInfo.ValueBytes, 0);
                    break;
                case OpCodes.Stloc_0:
                    localIndex = 0;
                    break;
                case OpCodes.Stloc_1:
                    localIndex = 1;
                    break;
                case OpCodes.Stloc_2:
                    localIndex = 2;
                    break;
                case OpCodes.Stloc_3:
                    localIndex = 3;
                    break;
                case OpCodes.Stloc_S:
                    localIndex = (UInt16)anILOpInfo.ValueBytes[0];
                    break;
            }

            int bytesOffset = 0;
            for (int i = 0; i < aScannerState.CurrentILChunk.LocalVariables.Count && i <= localIndex; i++)
            {
                bytesOffset += aScannerState.CurrentILChunk.LocalVariables.ElementAt(i).sizeOnStackInBytes;
            }
            StackItem theItem = aScannerState.CurrentStackFrame.Stack.Pop();
            if (theItem.isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Float locals not supported yet!");
            }

            int locSize = aScannerState.CurrentILChunk.LocalVariables.ElementAt(localIndex).sizeOnStackInBytes;
            if(locSize == 0)
            {
                result.AppendLine("; 0 pop size");
            }
            else if (locSize == 8)
            {
                result.AppendLine(string.Format("pop dword [ebp-{0}]", bytesOffset));
                result.AppendLine(string.Format("pop dword [ebp-{0}]", bytesOffset - 4));
            }
            else if(locSize <= 4)
            {
                string poppedLocalSize = Utils.GetOpSizeString(locSize);
                result.AppendLine(string.Format("pop {0} [ebp-{1}]", poppedLocalSize, bytesOffset));
            }
            else
            {
                for(int i = 0; i < locSize; i++)
                {
                    result.AppendLine(string.Format("pop byte [ebp-{0}]", bytesOffset - i));
                }
            }

            return result.ToString().Trim();
        }
    }
}
