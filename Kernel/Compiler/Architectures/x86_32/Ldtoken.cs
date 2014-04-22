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
    public class Ldtoken : ILOps.Ldtoken
    {
        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="anILOpInfo">See base class documentation.</param>
        /// <param name="aScannerState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown when the metadata token is not for method metadata.
        /// </exception>
        public override string Convert(ILOpInfo anILOpInfo, ILScannerState aScannerState)
        {
            StringBuilder result = new StringBuilder();

            //Load token i.e. typeref
            //It shoudl also support methodref and fieldrefs

            int metadataToken = Utils.ReadInt32(anILOpInfo.ValueBytes, 0);
            try
            {
                Type theType = aScannerState.CurrentILChunk.Method.Module.ResolveType(metadataToken);
                if(theType == null)
                {
                    throw new NullReferenceException();
                }
                string typeTableId = aScannerState.GetTypeIdString(aScannerState.GetTypeID(theType));

                result.AppendLine(string.Format("push dword {0}", typeTableId));

                aScannerState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = false,
                    sizeOnStackInBytes = 4
                });
            }
            catch
            {
                throw new NotSupportedException("The metadata token specifies a fieldref or methodref which isn't supported yet!");
            }

            return result.ToString().Trim();
        }
    }
}
