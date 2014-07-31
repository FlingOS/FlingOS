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
    public class Ldloc : ILOps.Ldloc
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown when loading a float local is required as it currently hasn't been
        /// implemented.
        /// Also thrown if arguments are not of size 4 or 8 bytes.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            //Load local

            bool loadAddr = (ILOps.ILOp.OpCodes)anILOpInfo.opCode.Value == OpCodes.Ldloca ||
                            (ILOps.ILOp.OpCodes)anILOpInfo.opCode.Value == OpCodes.Ldloca_S;
            UInt16 localIndex = 0;
            switch ((ILOps.ILOp.OpCodes)anILOpInfo.opCode.Value)
            {
                case OpCodes.Ldloc:
                case OpCodes.Ldloca:
                    localIndex = (UInt16)Utils.ReadInt16(anILOpInfo.ValueBytes, 0);
                    break;
                case OpCodes.Ldloc_0:
                    localIndex = 0;
                    break;
                case OpCodes.Ldloc_1:
                    localIndex = 1;
                    break;
                case OpCodes.Ldloc_2:
                    localIndex = 2;
                    break;
                case OpCodes.Ldloc_3:
                    localIndex = 3;
                    break;
                case OpCodes.Ldloc_S:
                case OpCodes.Ldloca_S:
                    localIndex = (UInt16)anILOpInfo.ValueBytes[0];
                    break;
            }

            int bytesOffset = 0;
            for (int i = 0; i < aScannerState.CurrentILChunk.LocalVariables.Count && i <= localIndex; i++)
            {
                bytesOffset += aScannerState.CurrentILChunk.LocalVariables.ElementAt(i).sizeOnStackInBytes;
            }
            StackItem theLoc = aScannerState.CurrentILChunk.LocalVariables.ElementAt(localIndex);
            if (theLoc.isFloat)
            {
                //SUPPORT - floats
                throw new NotSupportedException("Float locals not supported yet!");
            }

            if (loadAddr)
            {
                result.AppendLine("mov eax, ebp");
                result.AppendLine(string.Format("sub eax, {0}", bytesOffset));
                result.AppendLine("push dword eax");

                aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = false,
                    sizeOnStackInBytes = 4
                });
            }
            else
            {
                int pushedLocalSizeVal = theLoc.sizeOnStackInBytes;

                if ((pushedLocalSizeVal % 4) != 0)
                {
                    throw new NotSupportedException("Invalid local bytes size!");
                }
                else
                {
                    for (int i = bytesOffset - (pushedLocalSizeVal - 4); i <= bytesOffset; i += 4)
                    {
                        result.AppendLine(string.Format("push dword [ebp-{0}]", i));
                    }
                }

                aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = theLoc.isFloat,
                    sizeOnStackInBytes = pushedLocalSizeVal
                });
            }

            return result.ToString().Trim();
        }
    }
}
