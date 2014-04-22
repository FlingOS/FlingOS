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
    public class Sizeof : ILOps.Sizeof
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

            int metadataToken = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
            Type theType = aScannerState.CurrentILChunk.Method.Module.ResolveType(metadataToken);
            result.AppendLine(string.Format("push dword {0}", Utils.GetNumBytesForType(theType)));

            aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
            {
                isFloat = false,
                sizeOnStackInBytes = 4
            });

            return result.ToString().Trim();
        }
    }
}
