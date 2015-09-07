using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Drivers.Compiler.IL;

namespace Drivers.Compiler.Architectures.MIPS32
{
    /// <summary>
    /// See base class documentation.
    /// </summary>
    public class Ldtoken : IL.ILOps.Ldtoken
    {
        public override void PerformStackOperations(ILPreprocessState conversionState, ILOp theOp)
        {
            try
            {
                conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = false,
                    sizeOnStackInBytes = 4,
                    isGCManaged = false
                });
            }
            catch
            {
                throw new NotSupportedException("The metadata token specifies a fieldref or methodref which isn't supported yet!");
            }
        }

        /// <summary>
        /// See base class documentation.
        /// </summary>
        /// <param name="theOp">See base class documentation.</param>
        /// <param name="conversionState">See base class documentation.</param>
        /// <returns>See base class documentation.</returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown when the metadata token is not for method metadata.
        /// </exception>
        public override void Convert(ILConversionState conversionState, ILOp theOp)
        {
            //Load token i.e. typeref
            //It should also support methodref and fieldrefs

            int metadataToken = Utilities.ReadInt32(theOp.ValueBytes, 0);
            try
            {
                Type theType = conversionState.Input.TheMethodInfo.UnderlyingInfo.Module.ResolveType(metadataToken);
                if (theType == null)
                {
                    throw new NullReferenceException();
                }
                string typeTableId = conversionState.TheILLibrary.GetTypeInfo(theType).ID;
                conversionState.AddExternalLabel(typeTableId);

                conversionState.Append(new ASMOps.La() { Dest = "$t4", Label = typeTableId });
                conversionState.Append(new ASMOps.Push() { Size = ASMOps.OperandSize.Word, Src = "$t4" });

                conversionState.CurrentStackFrame.Stack.Push(new StackItem()
                {
                    isFloat = false,
                    sizeOnStackInBytes = 4,
                    isGCManaged = false
                });
            }
            catch
            {
                throw new NotSupportedException("The metadata token specifies a fieldref or methodref which isn't supported yet!");
            }
        }
    }
}
