using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Kernel.Compiler.Architectures.x86_64
{
    public class Call : ILOps.Call
    {
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            int MetadataToken = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
            MethodBase methodToCall = aScannerState.CurrentMethod.Module.ResolveMethod(MetadataToken);
            if (methodToCall is MethodInfo)
            {
                string methodToCallID = aScannerState.GetMethodID((MethodInfo)methodToCall);

                result.AppendLine(string.Format("call {0}", methodToCallID));

                //The return value will be stored in RAX BUT(!) IL is written from a 32-bit perspective so it uses pop commands
                //as though the return value were on the stack
                //So we must store in our stack info that there "is" an item on the "stack" but it is actually in RAX
                StackFrame currFrame = aScannerState.StackFrames.Peek();
                currFrame.Stack.Push(new StackItem()
                    {
                        sizeOnStackInBytes = 8,
                        register = "rax"
                    });
            }
            else if (methodToCall is ConstructorInfo)
            {
                throw new Exception("Constructors not supported yet!");
            }

            return result.ToString().Trim();
        }
    }
}
