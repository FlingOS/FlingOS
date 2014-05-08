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

            StackItem itemA = aScannerState.CurrentStackFrame.Stack.Pop();
            StackItem itemB = aScannerState.CurrentStackFrame.Stack.Pop();

            if (itemA.isFloat || itemB.isFloat)
            {
                throw new NotSupportedException("Switching floats not supported!");
            }
            else if (itemA.sizeOnStackInBytes != 4 || itemB.sizeOnStackInBytes != 4)
            {
                throw new NotSupportedException("Switching non int32 values not supported!");
            }

            result.AppendLine("mov eax, [esp]");
            result.AppendLine("mov ebx, [esp+4]");
            result.AppendLine("mov [esp+4], eax");
            result.AppendLine("mov [esp], ebx");

            aScannerState.CurrentStackFrame.Stack.Push(itemA);
            aScannerState.CurrentStackFrame.Stack.Push(itemB);

            return result.ToString().Trim();
        }
    }
}
