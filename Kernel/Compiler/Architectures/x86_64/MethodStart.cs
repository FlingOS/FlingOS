using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.Architectures.x86_64
{
    public class MethodStart : ILOps.MethodStart
    {
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            result.AppendLine("; Method Start"); //DEBUG INFO
            result.AppendLine("push rbp");
            result.AppendLine("mov rbp, rsp");

            StackFrame currFrame = aScannerState.StackFrames.Peek();
            currFrame.Stack.Push(new StackItem()
            {
                sizeOnStackInBytes = 8
            });

            return result.ToString().Trim();
        }
    }
}
