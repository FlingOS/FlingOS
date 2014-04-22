using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.Architectures.x86_64
{
    public class Ret : ILOps.Ret
    {
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            result.AppendLine("; Method End"); //DEBUG INFO
            result.AppendLine("pop rbp");
            result.AppendLine("mov rsp, rbp");

            StackFrame currFrame = aScannerState.StackFrames.Peek();
            currFrame.Stack.Pop();

            result.AppendLine("ret");

            return result.ToString().Trim();
        }
    }
}
