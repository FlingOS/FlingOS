using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kernel.Compiler.Architectures.x86_64
{
    public class Pop : ILOps.Pop
    {
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            //Just removes (pops) the top value off our stack
            //But we must check that our "top value on stack" is actually on the stack and not in a register

            StackFrame currFrame = aScannerState.StackFrames.Peek();
            //We assume there is an item on the stack. If there isn't, we have gotten out of sync with the IL.
            //If debugging this, you should assume that the IL code is correct and our compiler is wrong.
            //Microsoft are much more likely to be correct than we are ;)
            StackItem topItem = currFrame.Stack.Pop();
            if (topItem.register == null)
            {
                result.AppendLine("pop");
            }
            else
            {
                result.AppendLine(string.Format("; top-most stack item in register '{0}'", topItem.register)); //DEBUG INFO
            }

            return result.ToString().Trim();
        }
    }
}
