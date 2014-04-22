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
    public class Pop : ILOps.Pop
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
            
            StackItem theItem = aScannerState.CurrentStackFrame.Stack.Pop();

            if (theItem.isNewGCObject)
            {
                //Decrement ref count

                //Get the ID of method to call as it will be labelled in the output ASM.
                string methodID = aScannerState.GetMethodID(aScannerState.DecrementRefCountMethod);
                //Append the actual call
                result.AppendLine(string.Format("call {0}", methodID));
            }

            result.AppendLine(string.Format("add esp, {0}", theItem.sizeOnStackInBytes));
        
            return result.ToString().Trim();
        }
    }
}
